import { create } from 'zustand'
import { tokenStorage } from '@/services/tokenStorage'
import { decodeJwt } from './jwtUtils'
import type { AuthUser, AuthResponse } from './types'

interface AuthState {
  user: AuthUser | null
  isAuthenticated: boolean
  setSession: (response: AuthResponse) => void
  clearSession: () => void
}

function loadUser(): AuthUser | null {
  const token = tokenStorage.getAccess()
  return token ? decodeJwt(token) : null
}

const initialUser = loadUser()

export const useAuthStore = create<AuthState>((set) => ({
  user: initialUser,
  isAuthenticated: initialUser !== null,

  setSession: (response) => {
    tokenStorage.setAccess(response.accessToken)
    tokenStorage.setRefresh(response.refreshToken)
    const user = decodeJwt(response.accessToken)
    set({ user, isAuthenticated: user !== null })
  },

  clearSession: () => {
    tokenStorage.clear()
    set({ user: null, isAuthenticated: false })
  },
}))

// Allows the axios interceptor to clear auth without a circular import
window.addEventListener('auth:unauthorized', () => {
  useAuthStore.getState().clearSession()
})
