export interface LeaveRequestDto {
  id: number;
  userId: number;
    correlationId?: string;
  leaveTypeName: string;
  leaveTypeCode: string;
  startDate: string;
  endDate: string;
  days: number;
  halfDaySession: string;
  status: string;
  reason: string;
  attachmentUrl?: string;
  managerComment?: string;
  hrComment?: string;
  needsHrApproval: boolean;
  createdAt: string;

}

export interface CreateLeaveRequest {
  leaveTypeId: number;
  startDate: string;
  endDate: string;
  halfDaySession: string;
  reason: string;
  attachmentUrl?: string;
}

export interface LeaveBalanceDto {
  leaveTypeName: string;
  leaveTypeCode: string;
  year: number;
  entitled: number;
  used: number;
  pending: number;
  carried: number;
  available: number;
  isPaid: boolean;
}

export interface LeaveTypeDto {
  id: number;
  name: string;
  code: string;
  maxDaysPerYear: number;
  isPaid: boolean;
  isActive: boolean;
}

export interface RejectLeaveRequest {
  comment: string;
}