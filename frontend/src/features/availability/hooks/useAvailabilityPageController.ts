import { useState } from 'react'
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

  const [selectedAgent, setSelectedAgent] = useState<string | undefined>()
  const [createOpen, setCreateOpen] = useState(false)
  const [editTarget, setEditTarget] = useState<Availability | null>(null)

  const { data: agents = [] } = useAgents()
  const { data: availabilities = [], isLoading } = useAvailabilityByAgent(selectedAgent)

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
      await createAvailability.mutateAsync(values)
      message.success('Disponibilidade criada')
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
    selectedAgent,
    setSelectedAgent,
    agentOptions: agents.map((agent) => ({ value: agent.id, label: agent.name })),
    availabilities,
    isLoading,
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
