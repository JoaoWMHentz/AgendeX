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
  useReassignAppointment,
  useRejectAppointment,
} from '../useAppointments'
import { appointmentStatusLabel, type Appointment, type AppointmentFilters } from '../types'

type ModalType = 'reject' | 'reassign' | 'complete' | null

export function useAppointmentsPageController() {
  const { user: me } = useAuthStore()
  const isAdmin = me?.role === Roles.Administrator
  const isAgent = me?.role === Roles.Agent

  const [filters, setFilters] = useState<AppointmentFilters>({})
  const [modal, setModal] = useState<{ type: ModalType; appointment?: Appointment }>({ type: null })
  const [rejectReason, setRejectReason] = useState('')
  const [completeSummary, setCompleteSummary] = useState('')
  const [reassignAgentId, setReassignAgentId] = useState<string | undefined>()

  const { data: appointments = [], isLoading } = useAppointments(filters)
  const { data: serviceTypes = [] } = useServiceTypes()
  const { data: agents = [] } = useAgents({ enabled: isAdmin })

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

  const openRejectModal = (appointment: Appointment) => setModal({ type: 'reject', appointment })
  const openCompleteModal = (appointment: Appointment) => setModal({ type: 'complete', appointment })
  const openReassignModal = (appointment: Appointment) => setModal({ type: 'reassign', appointment })

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
    isClient: false,
    appointments,
    isLoading,
    filters,
    setFilters,
    statusOptions: Object.entries(appointmentStatusLabel).map(([value, label]) => ({
      value: Number(value),
      label,
    })),
    serviceTypeOptions: serviceTypes.map((st) => ({ value: st.id, label: st.description })),
    agentOptions: agents.map((agent) => ({ value: agent.id, label: agent.name })),
    modal,
    openRejectModal,
    openCompleteModal,
    openReassignModal,
    closeModal,
    rejectLoading: rejectAppointment.isPending,
    completeLoading: completeAppointment.isPending,
    reassignLoading: reassignAppointment.isPending,
    rejectReason,
    setRejectReason,
    completeSummary,
    setCompleteSummary,
    reassignAgentId,
    setReassignAgentId,
    handleConfirm,
    handleCancel,
    handleReject,
    handleComplete,
    handleReassign,
  }
}
