import { api } from './api'
import type { AuthResponse, LoginRequest } from '@/features/auth/types'

export const authService = {
  login: (data: LoginRequest) =>
    api.post<AuthResponse>('/api/auth/login', data).then((r) => r.data),

  logout: (refreshToken: string) =>
    api.post('/api/auth/logout', { refreshToken }),

  refresh: (refreshToken: string) =>
    api.post<AuthResponse>('/api/auth/refresh', { refreshToken }).then((r) => r.data),
}
