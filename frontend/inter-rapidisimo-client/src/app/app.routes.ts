import { Routes } from '@angular/router';
import { StudentListComponent } from './components/student-list/student-list.component';
import { StudentFormComponent } from './components/student-form/student-form.component';
import { StudentClassmatesComponent } from './components/student-classmates/student-classmates.component';

export const routes: Routes = [
  { path: '', redirectTo: 'estudiantes', pathMatch: 'full' },
  { path: 'estudiantes', component: StudentListComponent },
  { path: 'registro', component: StudentFormComponent },
  { path: 'estudiantes/:id/editar', component: StudentFormComponent },
  { path: 'estudiantes/:id/companeros', component: StudentClassmatesComponent },
  { path: '**', redirectTo: 'estudiantes' }
];
