import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/ejercicio1',
    pathMatch: 'full'
  },
  {
    path: 'ejercicio1',
    loadChildren: () => import('./features/ejercicio1/ejercicio1.routes').then(m => m.EJERCICIO1_ROUTES)
  },
  {
    path: 'ejercicio2',
    loadChildren: () => import('./features/ejercicio2/ejercicio2.routes').then(m => m.EJERCICIO2_ROUTES)
  }
];
