import { Routes } from '@angular/router';
import { LoginComponent } from './features/user/login/login.component';
import { LayoutType } from './components/layouts/layout.types';
import { EventListComponent } from './features/dashboard/event-list/event-list.component';
import { CanActivateAuth } from './guards/auth.guard';

// Routes and their respective layouts. If layout is left blank, empty-layout is used.
export const routes: Routes = [
  {
    path: '',
    component: EventListComponent,
    data: { layout: LayoutType.Empty },
    canActivate: [CanActivateAuth]
  },
  {
    path: 'login',
    component: LoginComponent,
    data: { layout: LayoutType.CenterColumn },
  },
];
