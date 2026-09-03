import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { StudentService } from '../../services/student.service';
import { Student } from '../../models/student.model';

@Component({
  selector: 'app-student-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './student-list.component.html',
  styleUrl: './student-list.component.css'
})
export class StudentListComponent implements OnInit {
  private studentService = inject(StudentService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  students: Student[] = [];
  filteredStudents: Student[] = [];
  searchTerm: string = '';
  loading: boolean = true;
  errorMessage: string = '';
  successMessage: string = '';

  // Estado del Modal de Confirmación de Eliminación
  deleteModalOpen: boolean = false;
  studentToDelete: Student | null = null;
  isDeleting: boolean = false;

  // Estado del Modal de Carga Global para Acciones de Botones
  actionLoading: boolean = false;
  actionLoadingTitle: string = '';
  actionLoadingDesc: string = '';

  ngOnInit(): void {
    this.loadStudents();
  }

  loadStudents(): void {
    this.loading = true;
    this.errorMessage = '';
    this.cdr.markForCheck();

    this.studentService.getStudents().subscribe({
      next: (data) => {
        this.students = data;
        this.filterStudents();
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.errorMessage = 'No se pudieron cargar los registros de estudiantes. Asegúrate de que el Backend esté activo.';
        this.loading = false;
        this.cdr.markForCheck();
        console.error(err);
      }
    });
  }

  filterStudents(): void {
    if (!this.searchTerm.trim()) {
      this.filteredStudents = [...this.students];
      this.cdr.markForCheck();
      return;
    }

    const term = this.searchTerm.toLowerCase().trim();
    this.filteredStudents = this.students.filter(s =>
      s.fullName.toLowerCase().includes(term) ||
      s.documentNumber.toLowerCase().includes(term) ||
      s.email.toLowerCase().includes(term)
    );
    this.cdr.markForCheck();
  }

  // --- Manejo del Modal de Eliminación ---
  openDeleteModal(student: Student): void {
    this.studentToDelete = student;
    this.deleteModalOpen = true;
    this.cdr.markForCheck();
  }

  closeDeleteModal(): void {
    if (this.isDeleting) return;
    this.deleteModalOpen = false;
    this.studentToDelete = null;
    this.cdr.markForCheck();
  }

  confirmDelete(): void {
    if (!this.studentToDelete) return;
    const student = this.studentToDelete;
    this.isDeleting = true;
    this.cdr.markForCheck();

    this.studentService.deleteStudent(student.id).subscribe({
      next: () => {
        this.isDeleting = false;
        this.deleteModalOpen = false;
        this.studentToDelete = null;
        this.successMessage = `El registro de "${student.fullName}" fue eliminado exitosamente.`;
        this.loadStudents();
        this.cdr.markForCheck();
        setTimeout(() => {
          this.successMessage = '';
          this.cdr.markForCheck();
        }, 5000);
      },
      error: (err) => {
        this.isDeleting = false;
        this.deleteModalOpen = false;
        this.studentToDelete = null;
        this.errorMessage = err.error?.message || 'Error al eliminar el estudiante.';
        this.cdr.markForCheck();
      }
    });
  }

  // --- Manejo de Navegación con Modal de Carga Inmediato ---
  goToRegister(): void {
    this.actionLoadingTitle = 'Iniciando Registro de Estudiante';
    this.actionLoadingDesc = 'Cargando catálogo de materias y profesores...';
    this.actionLoading = true;
    this.cdr.markForCheck();

    setTimeout(() => {
      this.router.navigate(['/registro']);
    }, 200);
  }

  goToEdit(student: Student): void {
    this.actionLoadingTitle = 'Cargando Formulario de Edición';
    this.actionLoadingDesc = `Preparando matrícula de ${student.fullName}...`;
    this.actionLoading = true;
    this.cdr.markForCheck();

    setTimeout(() => {
      this.router.navigate(['/estudiantes', student.id, 'editar']);
    }, 200);
  }

  goToClassmates(student: Student): void {
    this.actionLoadingTitle = 'Consultando Compañeros de Clase';
    this.actionLoadingDesc = `Accediendo a las asignaturas de ${student.fullName}...`;
    this.actionLoading = true;
    this.cdr.markForCheck();

    setTimeout(() => {
      this.router.navigate(['/estudiantes', student.id, 'companeros']);
    }, 200);
  }
}
