import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { appointmentsService } from '@/services/appointments.service'
import { queryKeys } from '@/shared/queryKeys'
import type { AppointmentFilters } from './types'

export function useAppointments(filters: AppointmentFilters) {
  return useQuery({
    queryKey: queryKeys.appointments.all(filters),
    queryFn: () => appointmentsService.getAll(filters),
  })
}

function useAppointmentMutation<TVariables>(
  mutationFn: (vars: TVariables) => Promise<unknown>,
) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['appointments'] }),
  })
}

export function useCreateAppointment() {
  return useAppointmentMutation(appointmentsService.create)
}

export function useConfirmAppointment() {
  return useAppointmentMutation(appointmentsService.confirm)
}

export function useRejectAppointment() {
  return useAppointmentMutation(({ id, reason }: { id: string; reason: string }) =>
    appointmentsService.reject(id, reason),
  )
}

export function useCancelAppointment() {
  return useAppointmentMutation(appointmentsService.cancel)
}

export function useCompleteAppointment() {
  return useAppointmentMutation(({ id, summary }: { id: string; summary?: string }) =>
    appointmentsService.complete(id, summary),
  )
}

export function useReassignAppointment() {
  return useAppointmentMutation(({ id, agentId }: { id: string; agentId: string }) =>
    appointmentsService.reassign(id, agentId),
  )
}
