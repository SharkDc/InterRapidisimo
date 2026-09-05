import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Student, CreateStudentRequest, UpdateStudentRequest, StudentClassmates } from '../models/student.model';
import { ApiResponse } from '../models/api-response.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class StudentService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/students`;

  getStudents(): Observable<Student[]> {
    return this.http.get<ApiResponse<Student[]>>(this.apiUrl).pipe(
      map(response => response.data)
    );
  }

  getStudentById(id: number): Observable<Student> {
    return this.http.get<ApiResponse<Student>>(`${this.apiUrl}/${id}`).pipe(
      map(response => response.data)
    );
  }

  getStudentClassmates(id: number): Observable<StudentClassmates> {
    return this.http.get<ApiResponse<StudentClassmates>>(`${this.apiUrl}/${id}/classmates`).pipe(
      map(response => response.data)
    );
  }

  createStudent(request: CreateStudentRequest): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(this.apiUrl, request);
  }

  updateStudent(id: number, request: UpdateStudentRequest): Observable<ApiResponse<any>> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/${id}`, request);
  }

  deleteStudent(id: number): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`);
  }
}
