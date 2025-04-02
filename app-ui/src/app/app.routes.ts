import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { LayoutType } from './components/layouts/layout.types';
import { EventListComponent } from './features/events/event-list/event-list.component';
import { AuthGuard } from './guards/auth.guard';
import { CreateEventComponent } from './features/events/create-event/create-event.component';
import { CameraMainComponent } from './features/camera/camera-main/camera-main.component';
import { EventPreviewComponent } from './features/events/event-preview/event-preview.component';
import { LoginRedirectComponent } from './features/auth/login-redirect/login-redirect.component';
import { GroupPermissionGuard } from './guards/group.guard';
import { UserGroup } from './globals/userGroups';
import { NoAccessComponent } from './components/no-access/no-access.component';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { EventCalendarComponent } from './features/events/event-calendar/event-calendar.component';

// Routes and their respective layouts. If layout is left blank, empty-layout is used.
export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'event',
  },
  {
    path: 'redirect',
    component: LoginRedirectComponent,
    data: { layout: LayoutType.CenterColumn },
  },
  {
    path: 'event',
    component: EventListComponent,
    data: { layout: LayoutType.Main },
    canActivate: [AuthGuard],
  },
  {
    path: 'event/create',
    component: CreateEventComponent,
    data: {
      layout: LayoutType.Main,
      requiredGroup: UserGroup.EventAdmin,
    },
    canActivate: [AuthGuard, GroupPermissionGuard],
  },
  {
    path: 'calendar',
    component: EventCalendarComponent,
    data: {
      layout: LayoutType.Main,
    },
    canActivate: [AuthGuard],
  },
  {
    path: 'event/:eventId',
    component: EventPreviewComponent,
    data: { layout: LayoutType.Main },
    canActivate: [AuthGuard],
  },
  {
    path: 'login',
    component: LoginComponent,
    data: { layout: LayoutType.CenterColumn },
  },
  {
    path: 'camera',
    component: CameraMainComponent,
    data: { layout: LayoutType.Empty },
    canActivate: [AuthGuard],
  },
  {
    path: 'no-access',
    component: NoAccessComponent,
    data: { layout: LayoutType.CenterColumn },
  },
  {
    path: '**',
    component: NotFoundComponent,
    data: { layout: LayoutType.CenterColumn },
  },
];
