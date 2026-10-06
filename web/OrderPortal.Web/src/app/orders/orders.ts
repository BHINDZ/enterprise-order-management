import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  DestroyRef,
  OnInit,
  inject
} from '@angular/core';

import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { timer } from 'rxjs';

import { Order, OrderService } from '../services/order';
import { AuthService } from '../services/auth'; interface OrderNotification {
  id: number;
  type: 'success' | 'error' | 'info';
  title: string;
  message: string;
}

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [DatePipe, FormsModule],
  templateUrl: './orders.html',
  styleUrl: './orders.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Orders implements OnInit {

  private readonly orderService = inject(OrderService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly changeDetector = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);

  orders: Order[] = [];

  isLoading = false;
  isCreating = false;

  notifications: OrderNotification[] = [];

  private notificationId = 0;

  errorMessage = '';
  successMessage = '';

  customerName = '';
  productCode = 'FIBER-500';
  quantity = 1;

  get totalOrders(): number {
    return this.orders.length;
  }

  get submittedOrders(): number {
    return this.orders.filter(
      order => order.status === 1
    ).length;
  }

  get completedOrders(): number {
    return this.orders.filter(
      order => order.status === 3
    ).length;
  }

  ngOnInit(): void {
    this.loadOrders();
    this.startOrderStatusPolling();
  }

  /**
   * Loads the current orders from the API.
   */
  loadOrders(): void {

    this.isLoading = true;
    this.errorMessage = '';

    this.changeDetector.detectChanges();

    this.orderService.getOrders().subscribe({

      next: orders => {

        console.log('ORDERS LOADED:', orders);

        this.orders = [...orders];
        this.isLoading = false;

        this.changeDetector.detectChanges();
      },

      error: error => {

        console.error('LOAD ORDERS ERROR:', error);

        this.isLoading = false;

        if (error.status === 401) {
          this.authService.logout();
          this.router.navigate(['/login']);
          return;
        }

        this.errorMessage =
          'Unable to load orders.';

        this.changeDetector.detectChanges();
      }
    });
  }

  /**
   * Checks for order status changes every 3 seconds.
   *
   * The external telecom simulator sends a webhook
   * to the backend. This polling allows the Angular
   * dashboard to detect the updated status.
   */
  private startOrderStatusPolling(): void {

    timer(3000, 3000)
      .pipe(
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {

        // Don't interfere with the initial loading indicator.
        if (this.isLoading || this.isCreating) {
          return;
        }

        this.orderService.getOrders().subscribe({

          next: orders => {

            const previousOrders =
              this.orders;

            this.orders = [...orders];

            const statusChanges =
              this.getOrderStatusChanges(previousOrders, orders);

            if (statusChanges.length > 0) {
              statusChanges.forEach(change => {
                this.handleOrderStatusChange(change);
              });
            }

            this.changeDetector.detectChanges();
          },

          error: error => {

            console.error(
              'ORDER STATUS POLLING ERROR:',
              error
            );

            // Don't display a disruptive error message
            // for background polling failures.
          }
        });
      });
  }

  private getOrderStatusChanges(
    previousOrders: Order[],
    currentOrders: Order[]
  ): Array<{
    order: Order;
    previousStatus: number;
  }> {
    const changes: Array<{
      order: Order;
      previousStatus: number;
    }> = [];

    for (const currentOrder of currentOrders) {
      const previousOrder = previousOrders.find(
        order => order.id === currentOrder.id
      );

      if (
        previousOrder &&
        previousOrder.status !== currentOrder.status
      ) {
        changes.push({
          order: currentOrder,
          previousStatus: previousOrder.status
        });
      }
    }

    return changes;
  }

  private handleOrderStatusChange(change: {
    order: Order;
    previousStatus: number;
  }): void {
    const { order } = change;

    const status = this.getStatusText(order.status);

    console.log(
      `ORDER STATUS UPDATED BY WEBHOOK: #${order.id} → ${status}`
    );

    switch (status.toUpperCase()) {
      case 'COMPLETED':
        this.showNotification(
          'success',
          'Order Completed',
          `Order #${order.id} has been successfully completed.`
        );
        break;

      case 'FAILED':
        this.showNotification(
          'error',
          'Order Failed',
          `Order #${order.id} could not be completed.`
        );
        break;

      case 'PROCESSING':
        this.showNotification(
          'info',
          'Order Processing',
          `Order #${order.id} is now being processed.`
        );
        break;

      case 'SUBMITTED':
        this.showNotification(
          'info',
          'Order Submitted',
          `Order #${order.id} has been submitted successfully.`
        );
        break;
    }

    this.changeDetector.detectChanges();
  }


  /**
   * Determines whether an order status changed
   * between two API responses.
   */
  private hasOrderStatusChanged(
    previousOrders: Order[],
    currentOrders: Order[]
  ): boolean {

    for (const currentOrder of currentOrders) {

      const previousOrder =
        previousOrders.find(
          order => order.id === currentOrder.id
        );

      if (
        previousOrder &&
        previousOrder.status !== currentOrder.status
      ) {
        return true;
      }
    }

    return false;
  }

  private showNotification(
    type: OrderNotification['type'],
    title: string,
    message: string
  ): void {
    const notification: OrderNotification = {
      id: ++this.notificationId,
      type,
      title,
      message
    };

    this.notifications = [
      ...this.notifications,
      notification
    ];

    this.changeDetector.detectChanges();

    setTimeout(() => {
      this.removeNotification(notification.id);
    }, 5000);
  }

  removeNotification(id: number): void {
    this.notifications = this.notifications.filter(
      notification => notification.id !== id
    );

    this.changeDetector.detectChanges();
  }


  createOrder(): void {

    if (this.isCreating) {
      return;
    }

    this.errorMessage = '';
    this.successMessage = '';

    if (!this.customerName.trim()) {

      this.errorMessage =
        'Customer name is required.';

      this.changeDetector.detectChanges();

      return;
    }

    if (!this.productCode.trim()) {

      this.errorMessage =
        'Product code is required.';

      this.changeDetector.detectChanges();

      return;
    }

    if (this.quantity <= 0) {

      this.errorMessage =
        'Quantity must be greater than zero.';

      this.changeDetector.detectChanges();

      return;
    }

    console.log(
      'CREATE ORDER REQUEST:',
      {
        customerName: this.customerName.trim(),
        productCode: this.productCode,
        quantity: this.quantity
      }
    );

    this.isCreating = true;

    this.changeDetector.detectChanges();

    this.orderService.createOrder({

      customerName:
        this.customerName.trim(),

      productCode:
        this.productCode,

      quantity:
        this.quantity

    }).subscribe({

      next: order => {

        console.log(
          'CREATE ORDER SUCCESS:',
          order
        );

        this.orders = [
          order,
          ...this.orders
        ];

        this.successMessage =
          `Order #${order.id} submitted successfully. ` +
          `External Order ID: ${order.externalOrderId}`;

        this.customerName = '';
        this.productCode = 'FIBER-500';
        this.quantity = 1;

        this.isCreating = false;

        this.changeDetector.detectChanges();
      },

      error: error => {

        console.error(
          'CREATE ORDER ERROR:',
          error
        );

        this.isCreating = false;

        if (error.status === 401) {

          this.authService.logout();
          this.router.navigate(['/login']);

          return;
        }

        this.errorMessage =
          error.error?.detail ??
          'Unable to create the order.';

        this.changeDetector.detectChanges();
      }
    });
  }

  logout(): void {

    this.authService.logout();

    this.router.navigate(['/login']);
  }

  getStatusText(status: number): string {

    switch (status) {

      case 0:
        return 'Pending';

      case 1:
        return 'Submitted';

      case 2:
        return 'Processing';

      case 3:
        return 'Completed';

      case 4:
        return 'Failed';

      default:
        return 'Unknown';
    }
  }
}
