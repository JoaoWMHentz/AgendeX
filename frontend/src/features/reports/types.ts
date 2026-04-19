import type { AppointmentStatusValue } from '@/features/appointments/types'

export const ReportType = {
  TotalAppointmentsByAgent: 0,
  TotalAppointmentsByClient: 1,
  AppointmentsByStatus: 2,
  CompletedVsCanceledRate: 3,
  AppointmentsByServiceType: 4,
} as const

export type ReportTypeValue = (typeof ReportType)[keyof typeof ReportType]

export const reportTypeLabel: Record<ReportTypeValue, string> = {
  0: 'Total de agendamentos por agente',
  1: 'Total de agendamentos por cliente',
  2: 'Agendamentos por status',
  3: 'Taxa de concluídos vs cancelados',
  4: 'Distribuição por tipo de serviço',
}

export const ReportSortDirection = {
  Asc: 'Asc',
  Desc: 'Desc',
} as const

export type ReportSortDirectionValue = (typeof ReportSortDirection)[keyof typeof ReportSortDirection]

export type ReportSortBy =
  | 'clientName'
  | 'agentName'
  | 'appointmentDate'
  | 'appointmentTime'
  | 'serviceType'
  | 'status'
  | 'createdAt'
  | 'confirmedAt'
  | 'canceledAt'
  | 'reason'

export interface ReportFilters {
  clientIds?: string[]
  agentIds?: string[]
  from?: string
  to?: string
  serviceTypeIds?: number[]
  statuses?: AppointmentStatusValue[]
  reportType: ReportTypeValue
  sortBy?: ReportSortBy
  sortDirection?: ReportSortDirectionValue
}

export interface ReportAggregate {
  label: string
  value: number
}

export interface ReportRow {
  appointmentId: string
  clientName: string
  agentName: string
  appointmentDate: string
  appointmentTime: string
  serviceType: string
  status: AppointmentStatusValue
  createdAt: string
  confirmedAt?: string
  canceledAt?: string
  rejectionOrCancellationReason?: string
}

export interface ReportResult {
  reportType: ReportTypeValue
  aggregates: ReportAggregate[]
  rows: ReportRow[]
}

export interface ReportFileResponse {
  blob: Blob
  fileName: string
}
