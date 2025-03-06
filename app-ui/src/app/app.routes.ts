import { Routes } from '@angular/router';
import { LoginComponent } from './features/user/login/login.component';
import { LayoutType } from './components/layouts/layout.types';
import { EventListComponent } from './features/events/event-list/event-list.component';
import { CanActivateAuth } from './guards/auth.guard';
import { CreateEventComponent } from './features/events/create-event/create-event.component';

// Routes and their respective layouts. If layout is left blank, empty-layout is used.
export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'event',
  },
  {
    path: 'event',
    component: EventListComponent,
    data: { layout: LayoutType.Main },
    canActivate: [CanActivateAuth]
  },
  {
    path: 'event/create',
    component: CreateEventComponent,
    data: { layout: LayoutType.Main },
    canActivate: [CanActivateAuth]
  },
  {
    path: 'login',
    component: LoginComponent,
    data: { layout: LayoutType.CenterColumn },
  },
];
