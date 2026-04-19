import { useEffect, useState } from 'react'
import { message } from 'antd'
import { useAgents } from '@/features/users/hooks/useUsers'
import { useAuthStore } from '@/features/auth/authStore'
import { Roles } from '@/shared/constants/roles'
import { extractApiError } from '@/shared/utils/apiError'
import {
  useAvailabilityByAgent,
  useCreateAvailability,
  useDeleteAvailability,
  useUpdateAvailability,
} from '../useAvailability'
import type { Availability } from '../types'
import type { CreateAvailabilityFormValues } from '../components/CreateAvailabilityModal'
import type { EditAvailabilityFormValues } from '../components/EditAvailabilityModal'

export function useAvailabilityPageController() {
  const { user: me } = useAuthStore()
  const isAdmin = me?.role === Roles.Administrator
  const isAgent = me?.role === Roles.Agent

  const [selectedAgent, setSelectedAgent] = useState<string | undefined>()
  const [selectedWeekDay, setSelectedWeekDay] = useState<number | undefined>()
  const [createOpen, setCreateOpen] = useState(false)
  const [editTarget, setEditTarget] = useState<Availability | null>(null)

  useEffect(() => {
    if (isAgent && me?.id) {
      setSelectedAgent(me.id)
    }
  }, [isAgent, me?.id])

  const { data: agents = [] } = useAgents({ enabled: isAdmin })
  const { data: availabilities = [], isLoading } = useAvailabilityByAgent(selectedAgent, selectedWeekDay)

  const createAvailability = useCreateAvailability()
  const updateAvailability = useUpdateAvailability(selectedAgent ?? '')
  const deleteAvailability = useDeleteAvailability(selectedAgent ?? '')

  const openCreateModal = () => {
    setCreateOpen(true)
  }

  const closeCreateModal = () => {
    setCreateOpen(false)
  }

  const openEditModal = (availability: Availability) => {
    setEditTarget(availability)
  }

  const closeEditModal = () => {
    setEditTarget(null)
  }

  const handleCreate = async (values: CreateAvailabilityFormValues) => {
    try {
      const created = await createAvailability.mutateAsync(values)
      const count = created.length
      message.success(
        count === 1 ? '1 disponibilidade criada' : `${count} disponibilidades criadas`,
      )
      closeCreateModal()
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  const handleUpdate = async (values: EditAvailabilityFormValues) => {
    if (!editTarget) return

    try {
      await updateAvailability.mutateAsync({
        id: editTarget.id,
        data: values,
      })
      message.success('Disponibilidade atualizada')
      closeEditModal()
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  const handleDelete = async (id: string) => {
    try {
      await deleteAvailability.mutateAsync(id)
      message.success('Disponibilidade desativada')
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  return {
    isAdmin,
    isAgent,
    selectedAgent,
    setSelectedAgent,
    agentOptions: agents.map((agent) => ({ value: agent.id, label: agent.name })),
    availabilities,
    isLoading,
    selectedWeekDay,
    setSelectedWeekDay,
    createOpen,
    openCreateModal,
    closeCreateModal,
    createLoading: createAvailability.isPending,
    editTarget,
    openEditModal,
    closeEditModal,
    updateLoading: updateAvailability.isPending,
    handleCreate,
    handleUpdate,
    handleDelete,
  }
}
