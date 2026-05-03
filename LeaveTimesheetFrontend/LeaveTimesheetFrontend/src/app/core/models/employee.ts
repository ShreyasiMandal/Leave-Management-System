export interface EmployeeDto {
  id: number;
  userId: number;
  employeeCode: string;
  fullName: string;
  email: string;
  phone?: string;
  departmentId?: number;
  departmentName?: string;
  managerId?: number;
  managerName?: string;
  designation?: string;
  dateOfJoining: string;
  isActive: boolean;
  gender?: string; 
}

export interface CreateEmployeeRequest {
  userId: number;         // links to auth user
  fullName: string;
  email: string;
  gender?: string;        // "Male" | "Female" | "Other"
  phone?: string;
  departmentId: number;
  managerId?: number | null;
  designation: string;
  employmentType?: string;
  dateOfJoining: string;
}

export interface DepartmentDto {
  id: number;
  name: string;
  code: string;
  managerId?: number;
    headId?: number; 
   headName?: string; 
  managerName?: string;
  isActive: boolean;
  employeeCount: number;
}