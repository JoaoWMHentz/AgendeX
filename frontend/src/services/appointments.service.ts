import { api } from './api'
import type { Appointment, AppointmentFilters, CreateAppointmentRequest } from '@/features/appointments/types'

export const appointmentsService = {
  getAll: (filters: AppointmentFilters) =>
    api.get<Appointment[]>('/api/appointments', { params: filters }).then((r) => r.data),

  getById: (id: string) => api.get<Appointment>(`/api/appointments/${id}`).then((r) => r.data),

  create: (data: CreateAppointmentRequest) =>
    api.post<Appointment>('/api/appointments', data).then((r) => r.data),

  confirm: (id: string) =>
    api.put<Appointment>(`/api/appointments/${id}/confirm`).then((r) => r.data),

  reject: (id: string, rejectionReason: string) =>
    api
      .put<Appointment>(`/api/appointments/${id}/reject`, JSON.stringify(rejectionReason))
      .then((r) => r.data),

  cancel: (id: string) =>
    api.put<Appointment>(`/api/appointments/${id}/cancel`).then((r) => r.data),

  complete: (id: string, serviceSummary?: string) =>
    api
      .put<Appointment>(`/api/appointments/${id}/complete`, JSON.stringify(serviceSummary ?? null))
      .then((r) => r.data),

  reassign: (id: string, newAgentId: string) =>
    api
      .put<Appointment>(`/api/appointments/${id}/reassign`, JSON.stringify(newAgentId))
      .then((r) => r.data),
}
