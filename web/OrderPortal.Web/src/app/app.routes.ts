import { Routes } from '@angular/router';
import { Login } from './login/login';
import { Orders } from './orders/orders';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: 'login',
    component: Login
  },
  {
    path: 'orders',
    component: Orders
  }
];
