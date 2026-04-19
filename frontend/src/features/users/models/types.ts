// Matches C# UserRole enum: Administrator=0, Agent=1, Client=2
export const UserRole = {
  Administrator: 0,
  Agent: 1,
  Client: 2,
} as const
export type UserRoleValue = (typeof UserRole)[keyof typeof UserRole]

export const userRoleLabel: Record<UserRoleValue, string> = {
  0: 'Administrador',
  1: 'Atendente',
  2: 'Cliente',
}

export interface ClientDetail {
  id: string
  cpf: string
  birthDate: string
  phone: string
  notes?: string
}

export interface User {
  id: string
  name: string
  email: string
  role: UserRoleValue
  isActive: boolean
  createdAt: string
  clientDetail?: ClientDetail
}

export interface AgentLookup {
  id: string
  name: string
}

export interface CreateUserRequest {
  name: string
  email: string
  password: string
  role: UserRoleValue
}

export interface UpdateUserRequest {
  name: string
  role?: UserRoleValue
  isActive?: boolean
}

export interface SetClientDetailRequest {
  cpf: string
  birthDate: string
  phone: string
  notes?: string
}
