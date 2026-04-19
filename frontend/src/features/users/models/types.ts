import { RoleLabels } from '@/shared/constants/roles'

// Matches C# UserRole enum: Administrator=0, Agent=1, Client=2
export const UserRole = {
  Administrator: 0,
  Agent: 1,
  Client: 2,
} as const
export type UserRoleValue = (typeof UserRole)[keyof typeof UserRole]

export const userRoleLabel: Record<UserRoleValue, string> = {
  0: RoleLabels.Administrator,
  1: RoleLabels.Agent,
  2: RoleLabels.Client,
}

export const userRoleOptions = [
  { value: UserRole.Administrator, label: userRoleLabel[UserRole.Administrator] },
  { value: UserRole.Agent, label: userRoleLabel[UserRole.Agent] },
  { value: UserRole.Client, label: userRoleLabel[UserRole.Client] },
]

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
