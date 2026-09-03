import { Course } from './course.model';

export interface Student {
  id: number;
  documentNumber: string;
  fullName: string;
  email: string;
  phone: string;
  registrationDate: string;
  totalCredits: number;
  courses: Course[];
}

export interface CreateStudentRequest {
  documentNumber: string;
  fullName: string;
  email: string;
  phone: string;
  courseIds: number[];
}

export interface UpdateStudentRequest {
  id: number;
  fullName: string;
  email: string;
  phone: string;
  courseIds: number[];
}

export interface CourseClassmates {
  courseId: number;
  courseName: string;
  teacherName: string;
  classmates: string[];
}

export interface StudentClassmates {
  studentId: number;
  studentName: string;
  courses: CourseClassmates[];
}
