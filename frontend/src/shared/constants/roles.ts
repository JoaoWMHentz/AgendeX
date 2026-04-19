export const Roles = {
  Administrator: 'Administrator',
  Agent: 'Agent',
  Client: 'Client',
} as const

export type Role = (typeof Roles)[keyof typeof Roles]
