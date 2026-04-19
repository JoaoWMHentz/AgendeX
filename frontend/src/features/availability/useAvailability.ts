import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { availabilityService } from '@/services/availability.service'
import { queryKeys } from '@/shared/queryKeys'

export function useAvailabilityByAgent(agentId: string | undefined, weekDay?: number) {
  return useQuery({
    queryKey: queryKeys.availability.byAgent(agentId ?? '', weekDay),
    queryFn: () => availabilityService.getByAgent(agentId!, weekDay),
    enabled: !!agentId,
  })
}

export function useAvailableSlots(agentId: string | undefined, date: string | undefined) {
  return useQuery({
    queryKey: queryKeys.availability.slots(agentId ?? '', date ?? ''),
    queryFn: () => availabilityService.getSlots(agentId!, date!),
    enabled: !!agentId && !!date,
  })
}

export function useCreateAvailability() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: availabilityService.create,
    onSuccess: (_data, variables) =>
      qc.invalidateQueries({ queryKey: queryKeys.availability.byAgent(variables.agentId) }),
  })
}

export function useUpdateAvailability(agentId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Parameters<typeof availabilityService.update>[1] }) =>
      availabilityService.update(id, data),
    onSuccess: () =>
      qc.invalidateQueries({ queryKey: queryKeys.availability.byAgent(agentId) }),
  })
}

export function useDeleteAvailability(agentId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: availabilityService.delete,
    onSuccess: () =>
      qc.invalidateQueries({ queryKey: queryKeys.availability.byAgent(agentId) }),
  })
}
