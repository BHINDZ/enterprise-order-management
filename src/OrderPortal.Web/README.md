# Angular Frontend

Create the Angular application in this folder with the Angular CLI.

Suggested commands:

```bash
npm install -g @angular/cli
ng new order-portal-web --routing --style=scss
cd order-portal-web
ng generate service services/orders
ng generate component pages/dashboard
ng generate component pages/orders
```

Configure the API base URL as:

`https://localhost:5001/api`

Recommended screens:

- Login
- Dashboard
- Orders
- Create Order
- Order Details
- Customers
- Products/Plans
