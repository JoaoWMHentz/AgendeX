import { api } from './api'
import type { User, CreateUserRequest, SetClientDetailRequest, UserRoleValue } from '@/features/users/types'

export const usersService = {
  getAll: (role?: UserRoleValue) =>
    api.get<User[]>('/api/users', { params: role !== undefined ? { role } : {} }).then((r) => r.data),

  getById: (id: string) => api.get<User>(`/api/users/${id}`).then((r) => r.data),

  create: (data: CreateUserRequest) => api.post<User>('/api/users', data).then((r) => r.data),

  updateName: (id: string, name: string) =>
    api.put<User>(`/api/users/${id}`, JSON.stringify(name)).then((r) => r.data),

  setClientDetail: (id: string, data: SetClientDetailRequest) =>
    api.put<User>(`/api/users/${id}/client-detail`, data).then((r) => r.data),

  delete: (id: string) => api.delete(`/api/users/${id}`),
}
