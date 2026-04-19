import { useState } from 'react'
import { message, Modal } from 'antd'
import { useServiceTypes } from '@/features/service-types/useServiceTypes'
import { useAgents } from '@/features/users/hooks/useUsers'
import { useAuthStore } from '@/features/auth/authStore'
import { Roles } from '@/shared/constants/roles'
import { extractApiError } from '@/shared/utils/apiError'
import {
  useAppointments,
  useCancelAppointment,
  useCompleteAppointment,
  useConfirmAppointment,
  useCreateAppointment,
  useReassignAppointment,
  useRejectAppointment,
} from '../useAppointments'
import { appointmentStatusLabel, type Appointment, type AppointmentFilters } from '../types'
import type { CreateAppointmentFormValues } from '../components/CreateAppointmentModal'

type ModalType = 'create' | 'reject' | 'reassign' | 'complete' | null

export function useAppointmentsPageController() {
  const { user: me } = useAuthStore()
  const isAdmin = me?.role === Roles.Administrator
  const isAgent = me?.role === Roles.Agent
  const isClient = me?.role === Roles.Client

  const [filters, setFilters] = useState<AppointmentFilters>({})
  const [modal, setModal] = useState<{ type: ModalType; appointment?: Appointment }>({ type: null })
  const [rejectReason, setRejectReason] = useState('')
  const [completeSummary, setCompleteSummary] = useState('')
  const [reassignAgentId, setReassignAgentId] = useState<string | undefined>()

  const shouldLoadServiceTypes = isAdmin || isClient
  const shouldLoadAgents = isAdmin || (isClient && modal.type === 'create')

  const { data: appointments = [], isLoading } = useAppointments(filters)
  const { data: serviceTypes = [] } = useServiceTypes({ enabled: shouldLoadServiceTypes })
  const { data: agents = [] } = useAgents({ enabled: shouldLoadAgents })

  const createAppointment = useCreateAppointment()
  const confirmAppointment = useConfirmAppointment()
  const rejectAppointment = useRejectAppointment()
  const cancelAppointment = useCancelAppointment()
  const completeAppointment = useCompleteAppointment()
  const reassignAppointment = useReassignAppointment()

  const closeModal = () => {
    setModal({ type: null })
    setRejectReason('')
    setCompleteSummary('')
    setReassignAgentId(undefined)
  }

  const openCreateModal = () => {
    setModal({ type: 'create' })
  }

  const openRejectModal = (appointment: Appointment) => {
    setModal({ type: 'reject', appointment })
  }

  const openCompleteModal = (appointment: Appointment) => {
    setModal({ type: 'complete', appointment })
  }

  const openReassignModal = (appointment: Appointment) => {
    setModal({ type: 'reassign', appointment })
  }

  const handleCreate = async (values: CreateAppointmentFormValues) => {
    try {
      await createAppointment.mutateAsync(values)
      message.success('Agendamento criado com sucesso')
      closeModal()
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  const handleConfirm = (id: string) => {
    Modal.confirm({
      title: 'Confirmar agendamento?',
      onOk: async () => {
        try {
          await confirmAppointment.mutateAsync(id)
          message.success('Agendamento confirmado')
        } catch (err) {
          message.error(extractApiError(err))
        }
      },
    })
  }

  const handleCancel = (id: string) => {
    Modal.confirm({
      title: 'Cancelar agendamento?',
      okButtonProps: { danger: true },
      onOk: async () => {
        try {
          await cancelAppointment.mutateAsync(id)
          message.success('Agendamento cancelado')
        } catch (err) {
          message.error(extractApiError(err))
        }
      },
    })
  }

  const handleReject = async () => {
    if (!modal.appointment || !rejectReason.trim()) return

    try {
      await rejectAppointment.mutateAsync({ id: modal.appointment.id, reason: rejectReason })
      message.success('Agendamento rejeitado')
      closeModal()
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  const handleComplete = async () => {
    if (!modal.appointment) return

    try {
      await completeAppointment.mutateAsync({
        id: modal.appointment.id,
        summary: completeSummary || undefined,
      })
      message.success('Agendamento concluído')
      closeModal()
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  const handleReassign = async () => {
    if (!modal.appointment || !reassignAgentId) return

    try {
      await reassignAppointment.mutateAsync({ id: modal.appointment.id, agentId: reassignAgentId })
      message.success('Agendamento reatribuído')
      closeModal()
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  return {
    isAdmin,
    isAgent,
    isClient,
    appointments,
    isLoading,
    filters,
    setFilters,
    statusOptions: Object.entries(appointmentStatusLabel).map(([value, label]) => ({
      value: Number(value),
      label,
    })),
    serviceTypeOptions: serviceTypes.map((serviceType) => ({
      value: serviceType.id,
      label: serviceType.description,
    })),
    agentOptions: agents.map((agent) => ({ value: agent.id, label: agent.name })),
    modal,
    openCreateModal,
    openRejectModal,
    openCompleteModal,
    openReassignModal,
    closeModal,
    createLoading: createAppointment.isPending,
    rejectLoading: rejectAppointment.isPending,
    completeLoading: completeAppointment.isPending,
    reassignLoading: reassignAppointment.isPending,
    rejectReason,
    setRejectReason,
    completeSummary,
    setCompleteSummary,
    reassignAgentId,
    setReassignAgentId,
    handleCreate,
    handleConfirm,
    handleCancel,
    handleReject,
    handleComplete,
    handleReassign,
  }
}
