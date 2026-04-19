export const Roles = {
  Administrator: 'Administrator',
  Agent: 'Agent',
  Client: 'Client',
} as const

export type Role = (typeof Roles)[keyof typeof Roles]

export const RoleLabels: Record<Role, string> = {
  Administrator: 'Administrador',
  Agent: 'Agente',
  Client: 'Cliente',
}

export function getRoleLabel(role?: Role | string | null): string {
  if (!role) return ''
  if (role in RoleLabels) {
    return RoleLabels[role as Role]
  }

  return String(role)
}

export const RoleOptions = Object.entries(RoleLabels).map(([value, label]) => ({
  value: value as Role,
  label,
}))
