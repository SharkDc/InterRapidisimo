import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { CourseService } from '../../services/course.service';
import { StudentService } from '../../services/student.service';
import { Course } from '../../models/course.model';
import { CreateStudentRequest, UpdateStudentRequest } from '../../models/student.model';

@Component({
  selector: 'app-student-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './student-form.component.html',
  styleUrl: './student-form.component.css'
})
export class StudentFormComponent implements OnInit {
  private courseService = inject(CourseService);
  private studentService = inject(StudentService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private cdr = inject(ChangeDetectorRef);

  isEditMode: boolean = false;
  studentId: number | null = null;

  // Form Model
  documentNumber: string = '';
  fullName: string = '';
  email: string = '';
  phone: string = '';
  selectedCourseIds: Set<number> = new Set<number>();

  // Data
  availableCourses: Course[] = [];
  loading: boolean = true;
  submitting: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';

  ngOnInit(): void {
    this.loadCoursesAndInit();
  }

  loadCoursesAndInit(): void {
    this.loading = true;
    this.cdr.markForCheck();
    this.courseService.getCourses().subscribe({
      next: (courses) => {
        this.availableCourses = courses;

        // Check if edit mode
        const idParam = this.route.snapshot.paramMap.get('id');
        if (idParam) {
          this.isEditMode = true;
          this.studentId = Number(idParam);
          this.loadStudentData(this.studentId);
        } else {
          this.loading = false;
          this.cdr.markForCheck();
        }
      },
      error: (err) => {
        this.errorMessage = 'No se pudieron cargar las materias disponibles del servidor.';
        this.loading = false;
        this.cdr.markForCheck();
        console.error(err);
      }
    });
  }

  loadStudentData(id: number): void {
    this.studentService.getStudentById(id).subscribe({
      next: (student) => {
        this.documentNumber = student.documentNumber;
        this.fullName = student.fullName;
        this.email = student.email;
        this.phone = student.phone;

        this.selectedCourseIds.clear();
        student.courses.forEach(c => this.selectedCourseIds.add(c.id));
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.errorMessage = 'No se pudo cargar la información del estudiante para edición.';
        this.loading = false;
        this.cdr.markForCheck();
        console.error(err);
      }
    });
  }

  /**
   * Verifica si una materia está seleccionada
   */
  isCourseSelected(courseId: number): boolean {
    return this.selectedCourseIds.has(courseId);
  }

  /**
   * Obtiene los IDs de los profesores cuyas materias ya están seleccionadas
   */
  getSelectedTeacherIds(): number[] {
    const teacherIds: number[] = [];
    for (const courseId of this.selectedCourseIds) {
      const course = this.availableCourses.find(c => c.id === courseId);
      if (course) {
        teacherIds.push(course.teacherId);
      }
    }
    return teacherIds;
  }

  /**
   * Regla 7: Determina si una materia debe deshabilitarse porque:
   * 1. Ya se seleccionó otra materia con el mismo profesor (conflicto de docente).
   * 2. O ya se alcanzaron las 3 materias máximas y esta materia no está seleccionada.
   */
  isCourseDisabled(course: Course): boolean {
    if (this.isCourseSelected(course.id)) {
      return false; // Siempre se puede deseleccionar una materia ya elegida
    }

    // Regla 5: Máximo 3 materias
    if (this.selectedCourseIds.size >= 3) {
      return true;
    }

    // Regla 7: El estudiante no podrá tener clases con el mismo profesor
    const selectedTeacherIds = this.getSelectedTeacherIds();
    return selectedTeacherIds.includes(course.teacherId);
  }

  /**
   * Razón por la cual la materia está deshabilitada (para feedback al usuario)
   */
  getDisabledReason(course: Course): string {
    if (this.isCourseSelected(course.id)) {
      return '';
    }

    const selectedTeacherIds = this.getSelectedTeacherIds();
    if (selectedTeacherIds.includes(course.teacherId)) {
      return `Ya seleccionaste una materia con el docente ${course.teacherName}. (Regla: Profesores distintos)`;
    }

    if (this.selectedCourseIds.size >= 3) {
      return 'Ya has seleccionado el límite máximo de 3 materias.';
    }

    return '';
  }

  /**
   * Maneja el cambio de selección de una materia
   */
  toggleCourse(course: Course): void {
    this.errorMessage = '';

    if (this.selectedCourseIds.has(course.id)) {
      this.selectedCourseIds.delete(course.id);
    } else {
      if (this.selectedCourseIds.size >= 3) {
        this.errorMessage = 'Solo puedes seleccionar exactamente 3 materias (9 créditos).';
        return;
      }

      // Validar profesor repetido
      const selectedTeacherIds = this.getSelectedTeacherIds();
      if (selectedTeacherIds.includes(course.teacherId)) {
        this.errorMessage = `No puedes seleccionar "${course.name}" porque ya tienes otra materia con el profesor ${course.teacherName}.`;
        return;
      }

      this.selectedCourseIds.add(course.id);
    }
    this.cdr.markForCheck();
  }

  // Validación de campos
  isDocumentTouched: boolean = false;
  isEmailTouched: boolean = false;
  isPhoneTouched: boolean = false;

  /**
   * Bloquea físicamente en el evento keydown cualquier caracter que no sea dígito numérico (0-9).
   */
  onlyNumbers(event: KeyboardEvent): boolean {
    const allowedKeys = [
      'Backspace', 'Delete', 'Tab', 'Escape', 'Enter',
      'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown',
      'Home', 'End'
    ];

    // Permitir teclas de control / atajos de edición (Ctrl+A, Ctrl+C, Ctrl+V, etc.)
    if (allowedKeys.includes(event.key) || event.ctrlKey || event.metaKey) {
      return true;
    }

    // Bloquear cualquier tecla que no sea dígito 0-9
    if (!/^\d$/.test(event.key)) {
      event.preventDefault();
      return false;
    }

    return true;
  }

  /**
   * Filtra entradas en tiempo real actualizando tanto el modelo como el valor visual del DOM
   * para impedir que se peguen o muestren caracteres no numéricos.
   */
  filterNumericInput(event: Event, field: 'documentNumber' | 'phone'): void {
    const input = event.target as HTMLInputElement;
    if (!input) return;

    const cleanValue = input.value.replace(/\D/g, '');
    if (input.value !== cleanValue) {
      input.value = cleanValue;
    }
    this[field] = cleanValue;
    this.cdr.markForCheck();
  }

  /**
   * Valida formato de correo electrónico
   */
  isValidEmail(email: string): boolean {
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return emailRegex.test(email.trim());
  }

  /**
   * Valida que el documento contenga entre 5 y 20 dígitos numéricos
   */
  isValidDocument(doc: string): boolean {
    return /^\d{5,20}$/.test(doc.trim());
  }

  /**
   * Valida el teléfono (si tiene contenido debe contener únicamente números y entre 7 y 15 dígitos)
   */
  isValidPhone(phone: string): boolean {
    if (!phone || phone.trim() === '') {
      return true;
    }
    return /^\d{7,15}$/.test(phone.trim());
  }

  get isPhoneValid(): boolean {
    return this.isValidPhone(this.phone);
  }

  get isDocumentValid(): boolean {
    return this.isEditMode || this.isValidDocument(this.documentNumber);
  }

  get isEmailValid(): boolean {
    return this.isValidEmail(this.email);
  }

  get isFullNameValid(): boolean {
    return this.fullName.trim().length >= 3;
  }

  get totalCredits(): number {
    return this.selectedCourseIds.size * 3;
  }

  get canSubmit(): boolean {
    return (
      this.isFullNameValid &&
      this.isEmailValid &&
      this.isDocumentValid &&
      this.isValidPhone(this.phone) &&
      this.selectedCourseIds.size === 3 &&
      !this.submitting
    );
  }

  onSubmit(): void {
    this.errorMessage = '';

    if (!this.isFullNameValid) {
      this.errorMessage = 'El nombre completo debe tener al menos 3 caracteres.';
      return;
    }

    if (!this.isDocumentValid) {
      this.errorMessage = 'El documento de identificación debe contener únicamente números (entre 5 y 20 dígitos).';
      return;
    }

    if (!this.isEmailValid) {
      this.errorMessage = 'El correo electrónico no tiene un formato válido (ejemplo: usuario@correo.com).';
      return;
    }

    if (!this.isValidPhone(this.phone)) {
      this.errorMessage = 'El teléfono debe contener únicamente números (entre 7 y 15 dígitos).';
      return;
    }

    if (this.selectedCourseIds.size !== 3) {
      this.errorMessage = 'Debes seleccionar exactamente 3 materias para completar la matrícula (9 créditos).';
      return;
    }

    this.submitting = true;
    this.cdr.markForCheck();

    if (this.isEditMode && this.studentId) {
      const updateReq: UpdateStudentRequest = {
        id: this.studentId,
        fullName: this.fullName,
        email: this.email,
        phone: this.phone,
        courseIds: Array.from(this.selectedCourseIds)
      };

      this.studentService.updateStudent(this.studentId, updateReq).subscribe({
        next: () => {
          this.submitting = false;
          this.cdr.markForCheck();
          this.router.navigate(['/estudiantes']);
        },
        error: (err) => {
          this.submitting = false;
          this.errorMessage = err.error?.descripcion || err.error?.message || 'Ocurrió un error al actualizar el estudiante.';
          this.cdr.markForCheck();
        }
      });
    } else {
      const createReq: CreateStudentRequest = {
        documentNumber: this.documentNumber,
        fullName: this.fullName,
        email: this.email,
        phone: this.phone,
        courseIds: Array.from(this.selectedCourseIds)
      };

      this.studentService.createStudent(createReq).subscribe({
        next: () => {
          this.submitting = false;
          this.cdr.markForCheck();
          this.router.navigate(['/estudiantes']);
        },
        error: (err) => {
          this.submitting = false;
          this.errorMessage = err.error?.descripcion || err.error?.message || 'Ocurrió un error al registrar el estudiante.';
          this.cdr.markForCheck();
        }
      });
    }
  }
}
