import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { NavbarComponent } from '../../../shared/components/navbar/navbar';
import { SidebarComponent } from '../../../shared/components/sidebar/sidebar';
import { EmployeeService } from '../../../core/services/employee';
import { AuthService } from '../../../core/services/auth';
import { EmployeeDto, DepartmentDto } from '../../../core/models/employee';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, NavbarComponent, SidebarComponent],
  templateUrl: './employee-list.html',
  styleUrl: './employee-list.scss'
})
export class EmployeeListComponent implements OnInit {
  employees: EmployeeDto[] = [];
  filtered: EmployeeDto[] = [];
  departments: DepartmentDto[] = [];
  managers: EmployeeDto[] = [];
  searchText = '';
  loading = true;
  isHR = false;
  message = '';
  messageType: 'success' | 'error' = 'success';
  showForm = false;
  activeTab: 'employees' | 'departments' = 'employees';

  // ── CREATE EMPLOYEE FORM ──────────────────────────────────────────────────
  newEmployee = {
    fullName: '',
    email: '',
    designation: '',
    role: 'Employee',
    gender: '',                                          // ✅ gender tracked
    dateOfJoining: new Date().toISOString().split('T')[0]
  };
  formLoading = false;
  formError = '';

  // ── EDIT EMPLOYEE MODAL ───────────────────────────────────────────────────
  showEditModal = false;
  editLoading = false;
  editError = '';
  editForm = {
    id: 0,
    fullName: '',
    designation: '',
    employmentType: 'Full-time',
    departmentId: 0,
    managerId: null as number | null,
    dateOfJoining: '',
    gender: ''                                           // ✅ gender tracked
  };

  // ── DEPARTMENTS ───────────────────────────────────────────────────────────
  deptLoading = false;
  showDeptForm = false;
  deptFormLoading = false;
  deptFormError = '';
  newDept = { name: '', description: '', headId: null as number | null };

  showEditDeptModal = false;
  editDeptLoading = false;
  editDeptError = '';
  editDeptForm = { id: 0, name: '', description: '', headId: null as number | null };

  constructor(
    private employeeService: EmployeeService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.isHR = this.authService.isHR();
    this.loadEmployees();
    this.loadDepartments();
  }

  // ── LOAD ──────────────────────────────────────────────────────────────────
  loadEmployees() {
    this.loading = true;
    this.employeeService.getEmployees().subscribe({
      next: (res: any) => {
        this.employees = res.data ?? res;
        this.managers  = this.employees.filter(e => e.isActive);
        this.filtered  = this.employees;
        this.loading   = false;
      },
      error: () => { this.loading = false; }
    });
  }

  loadDepartments() {
    this.deptLoading = true;
    this.employeeService.getDepartments().subscribe({
      next: (res: any) => {
        this.departments = res.data ?? res;
        this.deptLoading = false;
      },
      error: () => { this.deptLoading = false; }
    });
  }

  onSearch() {
    const q = this.searchText.toLowerCase();
    this.filtered = this.employees.filter(e =>
      e.fullName.toLowerCase().includes(q) ||
      e.email.toLowerCase().includes(q) ||
      (e.employeeCode || '').toLowerCase().includes(q) ||
      (e.departmentName || '').toLowerCase().includes(q)
    );
  }

  showMsg(msg: string, type: 'success' | 'error' = 'success') {
    this.message = msg;
    this.messageType = type;
    setTimeout(() => this.message = '', 4000);
  }

  // ── CREATE EMPLOYEE ───────────────────────────────────────────────────────
  createEmployee() {
    this.formError = '';
    if (!this.newEmployee.fullName.trim()) {
      this.formError = 'Full name is required.'; return;
    }
    if (!this.newEmployee.email.trim()) {
      this.formError = 'Email is required.'; return;
    }

    this.formLoading = true;

    // authService.createUser → POST /api/auth/create-user
    // This triggers the UserCreatedEvent which EmployeeService and
    // LeaveService both consume → gender flows through the entire pipeline
    this.authService.createUser({
      fullName: this.newEmployee.fullName,
      email:    this.newEmployee.email,
      password: 'TempPass@123',
      role:     this.newEmployee.role,
      gender:   this.newEmployee.gender   // ✅ gender sent to Auth → event → Employee → Leave
    }).subscribe({
      next: () => {
        this.formLoading = false;
        this.showForm = false;
        this.newEmployee = {
          fullName: '', email: '', designation: '', role: 'Employee',
          gender: '', dateOfJoining: new Date().toISOString().split('T')[0]
        };
        this.showMsg('Employee created. Welcome email sent.');
        setTimeout(() => this.loadEmployees(), 1500);
      },
      error: err => {
        this.formLoading = false;
        this.formError = err.error?.message || 'Failed to create employee.';
      }
    });
  }

  // ── EDIT EMPLOYEE ─────────────────────────────────────────────────────────
  openEdit(emp: EmployeeDto) {
    this.editError = '';
    this.editForm = {
      id:             emp.id,
      fullName:       emp.fullName,
      designation:    emp.designation || '',
      employmentType: 'Full-time',
      departmentId:   emp.departmentId || 0,
      managerId:      emp.managerId || null,
      dateOfJoining:  emp.dateOfJoining
                        ? emp.dateOfJoining.split('T')[0]
                        : new Date().toISOString().split('T')[0],
      gender:         (emp as any).gender || ''         // ✅ pre-fills existing gender
    };
    this.showEditModal = true;
  }

  closeEdit() { this.showEditModal = false; this.editError = ''; }

  saveEdit() {
    this.editError = '';
    if (!this.editForm.fullName.trim())    { this.editError = 'Full name is required.'; return; }
    if (!this.editForm.departmentId)       { this.editError = 'Please select a department.'; return; }
    if (!this.editForm.designation.trim()) { this.editError = 'Designation is required.'; return; }

    this.editLoading = true;
    this.employeeService.updateEmployee(this.editForm.id, {
      fullName:       this.editForm.fullName,
      designation:    this.editForm.designation,
      employmentType: this.editForm.employmentType,
      departmentId:   this.editForm.departmentId,
      managerId:      this.editForm.managerId || undefined,
      dateOfJoining:  this.editForm.dateOfJoining + 'T00:00:00',
      gender:         this.editForm.gender || undefined  // ✅ gender saved on update
    }).subscribe({
      next: () => {
        this.editLoading = false;
        this.showEditModal = false;
        this.showMsg(`${this.editForm.fullName} updated successfully.`);
        this.loadEmployees();
      },
      error: err => {
        this.editLoading = false;
        this.editError = err.error?.message || 'Update failed.';
      }
    });
  }

  // ── DEACTIVATE / REACTIVATE ───────────────────────────────────────────────
  deactivate(emp: EmployeeDto) {
    if (!confirm(`Deactivate ${emp.fullName}?`)) return;
    this.employeeService.deactivateEmployee(emp.id).subscribe({
      next: () => { this.showMsg(`${emp.fullName} deactivated.`); this.loadEmployees(); },
      error: err => this.showMsg(err.error?.message || 'Failed.', 'error')
    });
  }

  reactivate(emp: EmployeeDto) {
    this.employeeService.reactivateEmployee(emp.id).subscribe({
      next: () => { this.showMsg(`${emp.fullName} reactivated.`); this.loadEmployees(); },
      error: err => this.showMsg(err.error?.message || 'Failed.', 'error')
    });
  }

  // ── DEPARTMENTS ───────────────────────────────────────────────────────────
  createDept() {
    this.deptFormError = '';
    if (!this.newDept.name.trim()) { this.deptFormError = 'Department name is required.'; return; }
    this.deptFormLoading = true;
    this.employeeService.createDepartment({
      name:        this.newDept.name,
      description: this.newDept.description,
      headId:      this.newDept.headId || undefined
    }).subscribe({
      next: () => {
        this.deptFormLoading = false;
        this.showDeptForm = false;
        this.newDept = { name: '', description: '', headId: null };
        this.showMsg('Department created successfully.');
        this.loadDepartments();
      },
      error: err => {
        this.deptFormLoading = false;
        this.deptFormError = err.error?.message || 'Failed.';
      }
    });
  }

  openEditDept(dept: DepartmentDto) {
    this.editDeptError = '';
    this.editDeptForm = {
      id:          dept.id,
      name:        dept.name,
      description: (dept as any).description || '',
      headId:      dept.managerId || null
    };
    this.showEditDeptModal = true;
  }

  closeEditDept() { this.showEditDeptModal = false; this.editDeptError = ''; }

  saveEditDept() {
    this.editDeptError = '';
    if (!this.editDeptForm.name.trim()) { this.editDeptError = 'Department name is required.'; return; }
    this.editDeptLoading = true;
    this.employeeService.updateDepartment(this.editDeptForm.id, {
      name:        this.editDeptForm.name,
      description: this.editDeptForm.description,
      headId:      this.editDeptForm.headId || undefined
    }).subscribe({
      next: () => {
        this.editDeptLoading = false;
        this.showEditDeptModal = false;
        this.showMsg('Department updated.');
        this.loadDepartments();
      },
      error: err => {
        this.editDeptLoading = false;
        this.editDeptError = err.error?.message || 'Failed.';
      }
    });
  }

  deactivateDept(dept: DepartmentDto) {
    if (!confirm(`Deactivate "${dept.name}"?`)) return;
    this.employeeService.deactivateDepartment(dept.id).subscribe({
      next: () => { this.showMsg(`${dept.name} deactivated.`); this.loadDepartments(); },
      error: err => this.showMsg(err.error?.message || 'Failed.', 'error')
    });
  }

  reactivateDept(dept: DepartmentDto) {
    this.employeeService.reactivateDepartment(dept.id).subscribe({
      next: () => { this.showMsg(`${dept.name} reactivated.`); this.loadDepartments(); },
      error: err => this.showMsg(err.error?.message || 'Failed.', 'error')
    });
  }

  getDeptName(id?: number): string {
    if (!id) return '—';
    return this.departments.find(d => d.id === id)?.name || '—';
  }

  getEmployeesInDept(deptId: number): number {
    return this.employees.filter(e => e.departmentId === deptId).length;
  }

  getInitials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }
}