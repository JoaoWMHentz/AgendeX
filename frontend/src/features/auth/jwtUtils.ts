import type { AuthUser } from './types'

// ClaimTypes.Role URI emitted by the .NET backend
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

export function decodeJwt(token: string): AuthUser | null {
  try {
    const [, payload] = token.split('.')
    const padded = payload.replace(/-/g, '+').replace(/_/g, '/').padEnd(
      payload.length + ((4 - (payload.length % 4)) % 4),
      '=',
    )
    const decoded: Record<string, string> = JSON.parse(atob(padded))
    return {
      id: decoded['sub'],
      name: decoded['name'],
      email: decoded['email'],
      role: decoded[ROLE_CLAIM],
    }
  } catch {
    return null
  }
}
