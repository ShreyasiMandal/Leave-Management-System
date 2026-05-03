import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NavbarComponent } from '../../../shared/components/navbar/navbar';
import { SidebarComponent } from '../../../shared/components/sidebar/sidebar';
import { TimesheetService } from '../../../core/services/timesheet';
import { ProjectDto } from '../../../core/models/timesheet';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [CommonModule, FormsModule, NavbarComponent, SidebarComponent],
  templateUrl: './projects.html',
  styleUrl: './projects.scss'
})
export class ProjectsComponent implements OnInit {
  projects: ProjectDto[] = [];
  loading = false;
  message = '';
  messageType: 'success' | 'error' = 'success';

  isHR = false;
  filterActive: 'all' | 'active' | 'inactive' = 'all';

  // Create form
  showCreateForm = false;
  createForm = {
    name: '',
    code: '',
    clientName: ''
  };
  createLoading = false;

  constructor(
    private tsService: TimesheetService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.isHR = this.authService.isHR();
    this.loadProjects();
  }

  loadProjects() {
    this.loading = true;
    this.tsService.getProjects().subscribe({
      next: res => {
        this.projects = res;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.showMessage('Failed to load projects.', 'error');
      }
    });
  }

  get filteredProjects(): ProjectDto[] {
    if (this.filterActive === 'active') return this.projects.filter(p => p.isActive);
    if (this.filterActive === 'inactive') return this.projects.filter(p => !p.isActive);
    return this.projects;
  }

  createProject() {
    if (!this.createForm.name || !this.createForm.code) {
      this.showMessage('Name and code are required.', 'error');
      return;
    }
    this.createLoading = true;
    this.tsService.createProject({
      name: this.createForm.name,
      code: this.createForm.code.toUpperCase(),
      clientName: this.createForm.clientName || null
    }).subscribe({
      next: () => {
        this.createLoading = false;
        this.showCreateForm = false;
        this.resetCreateForm();
        this.showMessage('Project created successfully.', 'success');
        this.loadProjects();
      },
      error: err => {
        this.createLoading = false;
        this.showMessage(err.error?.message || 'Failed to create project.', 'error');
      }
    });
  }

  deactivateProject(id: number, name: string) {
    if (!confirm(`Deactivate project "${name}"? Employees won't be able to log time against it.`)) return;
    this.tsService.deactivateProject(id).subscribe({
      next: () => {
        this.showMessage('Project deactivated.', 'success');
        this.loadProjects();
      },
      error: () => this.showMessage('Failed to deactivate project.', 'error')
    });
  }

  resetCreateForm() {
    this.createForm = { name: '', code: '', clientName: '' };
  }

  showMessage(msg: string, type: 'success' | 'error') {
    this.message = msg;
    this.messageType = type;
    setTimeout(() => this.message = '', 4000);
  }

  get activeCount() { return this.projects.filter(p => p.isActive).length; }
  get inactiveCount() { return this.projects.filter(p => !p.isActive).length; }
}