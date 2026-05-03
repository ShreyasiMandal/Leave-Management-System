export interface TimesheetEntryDto {
  id: number;
  userId: number;
  projectId: number;
  projectName: string;
  date: string;
  weekStart: string;
  hours: number;
  category: string;
  description?: string;
  status: string;
  approverId?: number;
  approverComment?: string;
}

export interface CreateTimesheetEntry {
  date: string;
  projectId: number;
  hours: number;
  category: string;
  description?: string;
}

export interface WeekViewDto {
  weekStart: string;
  weekEnd: string;
  totalHours: number;
  weekStatus: string;
  entries: TimesheetEntryDto[];
}

export interface ProjectDto {
  id: number;
  name: string;
  code: string;
  clientName?: string; 
  isActive: boolean;
}