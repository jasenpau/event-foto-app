import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { LayoutType } from './components/layouts/layout.types';
import { EventListComponent } from './features/events/event-list/event-list.component';
import { AuthGuard } from './guards/auth.guard';
import { CameraMainComponent } from './features/camera/camera-main/camera-main.component';
import { EventPreviewComponent } from './features/events/event-preview/event-preview.component';
import { LoginRedirectComponent } from './features/auth/login-redirect/login-redirect.component';
import { GroupPermissionGuard } from './guards/group.guard';
import { UserGroup } from './globals/userGroups';
import { NoAccessComponent } from './components/no-access/no-access.component';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { EventCalendarComponent } from './features/events/event-calendar/event-calendar.component';
import { UserListComponent } from './features/user-admin/user-list/user-list.component';
import { GalleryViewComponent } from './features/gallery/gallery-view/gallery-view.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { InviteEntryComponent } from './features/auth/invite-entry/invite-entry.component';
import { WatermarkPageComponent } from './features/watermark/watermark-page/watermark-page.component';

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
    path: 'event/:eventId',
    component: EventPreviewComponent,
    data: { layout: LayoutType.Main },
    canActivate: [AuthGuard],
  },
  {
    path: 'gallery/:galleryId',
    component: GalleryViewComponent,
    data: { layout: LayoutType.Main },
    canActivate: [AuthGuard],
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
    path: 'users',
    component: UserListComponent,
    data: {
      layout: LayoutType.Main,
      requiredGroup: UserGroup.EventAdmin,
    },
    canActivate: [AuthGuard, GroupPermissionGuard],
  },
  {
    path: 'login',
    component: LoginComponent,
    data: { layout: LayoutType.CenterColumn },
  },
  {
    path: 'register',
    component: RegisterComponent,
    data: {
      layout: LayoutType.CenterColumn,
      ignoreActiveCheck: true,
    },
    canActivate: [AuthGuard],
  },
  {
    path: 'invite/:inviteKey',
    component: InviteEntryComponent,
    data: { layout: LayoutType.CenterColumn },
  },
  {
    path: 'camera',
    component: CameraMainComponent,
    data: {
      layout: LayoutType.Empty,
      requiredGroup: UserGroup.Photographer,
    },
    canActivate: [AuthGuard, GroupPermissionGuard],
  },
  {
    path: 'watermark',
    component: WatermarkPageComponent,
    data: {
      layout: LayoutType.Main,
    },
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
