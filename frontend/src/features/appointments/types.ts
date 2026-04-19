// Matches C# AppointmentStatus enum
export const AppointmentStatus = {
  PendingConfirmation: 0,
  Confirmed: 1,
  Rejected: 2,
  Canceled: 3,
  Completed: 4,
} as const
export type AppointmentStatusValue = (typeof AppointmentStatus)[keyof typeof AppointmentStatus]

export const appointmentStatusLabel: Record<AppointmentStatusValue, string> = {
  0: 'Pendente',
  1: 'Confirmado',
  2: 'Rejeitado',
  3: 'Cancelado',
  4: 'Concluído',
}

export const appointmentStatusColor: Record<AppointmentStatusValue, string> = {
  0: 'orange',
  1: 'green',
  2: 'red',
  3: 'default',
  4: 'blue',
}

export interface Appointment {
  id: string
  title: string
  description?: string
  serviceTypeId: number
  serviceTypeDescription: string
  clientId: string
  clientName: string
  agentId: string
  agentName: string
  date: string
  time: string
  status: AppointmentStatusValue
  rejectionReason?: string
  serviceSummary?: string
  createdAt: string
  confirmedAt?: string
  canceledAt?: string
  notes?: string
}

export interface AppointmentFilters {
  clientId?: string
  agentId?: string
  serviceTypeId?: number
  status?: AppointmentStatusValue
  from?: string
  to?: string
}

export interface CreateAppointmentRequest {
  title: string
  description?: string
  serviceTypeId: number
  agentId: string
  date: string
  time: string
  notes?: string
}
