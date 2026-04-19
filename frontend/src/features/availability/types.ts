// Matches C# WeekDay enum: Sunday=0 ... Saturday=6
export const WeekDay = {
  Sunday: 0,
  Monday: 1,
  Tuesday: 2,
  Wednesday: 3,
  Thursday: 4,
  Friday: 5,
  Saturday: 6,
} as const
export type WeekDayValue = (typeof WeekDay)[keyof typeof WeekDay]

export const weekDayLabel: Record<WeekDayValue, string> = {
  0: 'Domingo',
  1: 'Segunda',
  2: 'Terça',
  3: 'Quarta',
  4: 'Quinta',
  5: 'Sexta',
  6: 'Sábado',
}

export interface Availability {
  id: string
  agentId: string
  weekDay: WeekDayValue
  startTime: string
  endTime: string
  isActive: boolean
}

export interface AvailableSlot {
  startTime: string
  endTime: string
}

export interface CreateAvailabilityRequest {
  agentId: string
  weekDays: WeekDayValue[]
  startTime: string
  endTime: string
}

export interface UpdateAvailabilityRequest {
  startTime: string
  endTime: string
}
