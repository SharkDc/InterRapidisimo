import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Course } from '../models/course.model';
import { ApiResponse } from '../models/api-response.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CourseService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/courses`;

  getCourses(): Observable<Course[]> {
    return this.http.get<ApiResponse<Course[]>>(this.apiUrl).pipe(
      map(response => response.data)
    );
  }
}
