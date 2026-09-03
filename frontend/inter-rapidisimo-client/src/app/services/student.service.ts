import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Student, CreateStudentRequest, UpdateStudentRequest, StudentClassmates } from '../models/student.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class StudentService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/students`;

  getStudents(): Observable<Student[]> {
    return this.http.get<Student[]>(this.apiUrl);
  }

  getStudentById(id: number): Observable<Student> {
    return this.http.get<Student>(`${this.apiUrl}/${id}`);
  }

  getStudentClassmates(id: number): Observable<StudentClassmates> {
    return this.http.get<StudentClassmates>(`${this.apiUrl}/${id}/classmates`);
  }

  createStudent(request: CreateStudentRequest): Observable<any> {
    return this.http.post<any>(this.apiUrl, request);
  }

  updateStudent(id: number, request: UpdateStudentRequest): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, request);
  }

  deleteStudent(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }
}
