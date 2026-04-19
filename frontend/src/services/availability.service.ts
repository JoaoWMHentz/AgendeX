import { api } from './api'
import type {
  Availability,
  AvailableSlot,
  CreateAvailabilityRequest,
  UpdateAvailabilityRequest,
} from '@/features/availability/types'

export const availabilityService = {
  getByAgent: (agentId: string) =>
    api.get<Availability[]>(`/api/availability/agent/${agentId}`).then((r) => r.data),

  getSlots: (agentId: string, date: string) =>
    api
      .get<AvailableSlot[]>('/api/availability/slots', { params: { agentId, date } })
      .then((r) => r.data),

  create: (data: CreateAvailabilityRequest) =>
    api.post<Availability>('/api/availability', data).then((r) => r.data),

  update: (id: string, data: UpdateAvailabilityRequest) =>
    api.put<Availability>(`/api/availability/${id}`, data).then((r) => r.data),

  delete: (id: string) => api.delete(`/api/availability/${id}`),
}
