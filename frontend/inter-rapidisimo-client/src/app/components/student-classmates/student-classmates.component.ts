import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { StudentService } from '../../services/student.service';
import { StudentClassmates } from '../../models/student.model';

@Component({
  selector: 'app-student-classmates',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './student-classmates.component.html',
  styleUrl: './student-classmates.component.css'
})
export class StudentClassmatesComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private studentService = inject(StudentService);
  private cdr = inject(ChangeDetectorRef);

  studentId: number | null = null;
  classmatesData: StudentClassmates | null = null;
  loading: boolean = true;
  errorMessage: string = '';

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.studentId = Number(idParam);
      this.loadClassmates(this.studentId);
    } else {
      this.errorMessage = 'Identificador de estudiante no especificado.';
      this.loading = false;
      this.cdr.markForCheck();
    }
  }

  loadClassmates(id: number): void {
    this.loading = true;
    this.cdr.markForCheck();
    this.studentService.getStudentClassmates(id).subscribe({
      next: (data) => {
        this.classmatesData = data;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.errorMessage = 'No se pudo cargar la lista de compañeros de clase.';
        this.loading = false;
        this.cdr.markForCheck();
        console.error(err);
      }
    });
  }

  getInitials(name: string): string {
    if (!name) return '';
    const parts = name.trim().split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return parts[0].substring(0, 2).toUpperCase();
  }
}
