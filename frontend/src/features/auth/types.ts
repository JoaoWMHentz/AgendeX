export interface AuthResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface AuthUser {
  id: string
  name: string
  email: string
  role: string
}
