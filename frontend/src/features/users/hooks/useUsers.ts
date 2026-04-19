import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { usersService } from '@/services/users.service'
import { queryKeys } from '@/shared/queryKeys'
import { useAuthStore } from '@/features/auth/authStore'
import type { UserRoleValue } from '../models/types'

type QueryOptions = {
  enabled?: boolean
}

export function useUsers(role?: UserRoleValue, options?: QueryOptions) {
  return useQuery({
    queryKey: queryKeys.users.all(role),
    queryFn: () => usersService.getAll(role),
    enabled: options?.enabled ?? true,
  })
}

export function useAgents(options?: QueryOptions) {
  return useQuery({
    queryKey: queryKeys.users.agents,
    queryFn: () => usersService.getAll(1), // Agent = 1
    staleTime: 1000 * 60 * 5,
    enabled: options?.enabled ?? true,
  })
}

export function useCreateUser() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: usersService.create,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['users'] }),
  })
}

export function useUpdateUser() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Parameters<typeof usersService.updateUser>[1] }) =>
      usersService.updateUser(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['users'] }),
  })
}

export function useSetClientDetail() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Parameters<typeof usersService.setClientDetail>[1] }) =>
      usersService.setClientDetail(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['users'] }),
  })
}

export function useDeleteUser() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: usersService.delete,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['users'] }),
  })
}

export function useCurrentUserProfile() {
  const userId = useAuthStore((s) => s.user?.id)
  return useQuery({
    queryKey: queryKeys.users.byId(userId ?? ''),
    queryFn: () => usersService.getById(userId!),
    enabled: !!userId,
  })
}
