import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { guestGuard } from './core/guards/guest-guard';
import { MainLayout } from './layout/main-layout/main-layout';
import { Dashboard } from './pages/dashboard/dashboard';
import { ForgotPassword } from './pages/forgot-password/forgot-password';
import { Login } from './pages/login/login';
import { MatrixEditor } from './pages/matrix-editor/matrix-editor';
import { Palette } from './pages/palette/palette';
import { Register } from './pages/register/register';
import { ResetPassword } from './pages/reset-password/reset-password';
import { VerifyEmail } from './pages/verify-email/verify-email';
import { SiteWork } from './pages/site-work/site-work';
import { VideoWork } from './pages/video-work/video-work';

export const routes: Routes = [
  { path: 'login', component: Login, canActivate: [guestGuard] },
  { path: 'register', component: Register, canActivate: [guestGuard] },
  { path: 'verify-email', component: VerifyEmail, canActivate: [guestGuard] },
  { path: 'forgot-password', component: ForgotPassword, canActivate: [guestGuard] },
  { path: 'reset-password', component: ResetPassword, canActivate: [guestGuard] },
  {
    path: '',
    component: MainLayout,
    canActivate: [authGuard],
    children: [
      { path: '', component: Dashboard },
      { path: 'palette', component: Palette },
      { path: 'works/:workId/matrix', component: MatrixEditor },
      { path: 'works/:workId/video', component: VideoWork },
      { path: 'works/:workId/site', component: SiteWork },
    ],
  },

  { path: '**', redirectTo: '' },
];
