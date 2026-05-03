export interface NotificationDto {
  id: number;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  entityId?: number;
  entityType?: string;
  createdAt: string;
}