export const queryKeys = {
  serviceTypes: {
    all: ['serviceTypes'] as const,
    byId: (id: number) => ['serviceTypes', id] as const,
  },
  users: {
    all: (role?: number) => ['users', { role }] as const,
    byId: (id: string) => ['users', id] as const,
    agents: ['users', 'agents'] as const,
  },
  availability: {
    byAgent: (agentId: string, weekDay?: number) => ['availability', agentId, { weekDay }] as const,
    slots: (agentId: string, date: string) => ['availability', 'slots', agentId, date] as const,
  },
  appointments: {
    all: (filters: object) => ['appointments', filters] as const,
    byId: (id: string) => ['appointments', id] as const,
  },
  reports: {
    all: (filters: object) => ['reports', filters] as const,
  },
}
