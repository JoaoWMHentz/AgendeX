import { api } from './api'
import type { ServiceType } from '@/features/service-types/types'

export const serviceTypesService = {
  getAll: () => api.get<ServiceType[]>('/api/servicetypes').then((r) => r.data),
  getById: (id: number) => api.get<ServiceType>(`/api/servicetypes/${id}`).then((r) => r.data),
}
