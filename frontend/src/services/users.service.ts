import { api } from './api'
import type {
  AgentLookup,
  User,
  CreateUserRequest,
  UpdateUserRequest,
  SetClientDetailRequest,
  UserRoleValue,
} from '@/features/users/models/types'

export const usersService = {
  getAll: (role?: UserRoleValue) =>
    api.get<User[]>('/api/users', { params: role !== undefined ? { role } : {} }).then((r) => r.data),

  getAgents: () => api.get<AgentLookup[]>('/api/users/agents').then((r) => r.data),

  getById: (id: string) => api.get<User>(`/api/users/${id}`).then((r) => r.data),

  create: (data: CreateUserRequest) => api.post<User>('/api/users', data).then((r) => r.data),

  updateUser: (id: string, data: UpdateUserRequest) =>
    api.put<User>(`/api/users/${id}`, data).then((r) => r.data),

  setClientDetail: (id: string, data: SetClientDetailRequest) =>
    api.put<User>(`/api/users/${id}/client-detail`, data).then((r) => r.data),

  delete: (id: string) => api.delete(`/api/users/${id}`),
}
