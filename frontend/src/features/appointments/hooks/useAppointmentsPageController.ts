import { useState } from 'react'
import { message, Modal } from 'antd'
import { useServiceTypes } from '@/features/service-types/useServiceTypes'
import { useAgents } from '@/features/users/hooks/useUsers'
import { extractApiError } from '@/shared/utils/apiError'
import { useAppointments, useCancelAppointment, useReassignAppointment } from '../useAppointments'
import { appointmentStatusLabel, type Appointment, type AppointmentFilters } from '../types'

export function useAppointmentsPageController() {
  const [filters, setFilters] = useState<AppointmentFilters>({})
  const [selectedAppointment, setSelectedAppointment] = useState<Appointment | null>(null)
  const [reassignAgentId, setReassignAgentId] = useState<string | undefined>()
  const [reassignModalOpen, setReassignModalOpen] = useState(false)

  const { data: appointments = [], isLoading, refetch } = useAppointments(filters)
  const { data: serviceTypes = [] } = useServiceTypes()
  const { data: agents = [] } = useAgents({ enabled: true })

  const cancelAppointment = useCancelAppointment()
  const reassignAppointment = useReassignAppointment()

  const openDetail = (appointment: Appointment) => setSelectedAppointment(appointment)
  const closeDetail = () => setSelectedAppointment(null)

  const openReassignModal = (appointment: Appointment) => {
    setSelectedAppointment(appointment)
    setReassignModalOpen(true)
  }
  const closeReassignModal = () => {
    setReassignModalOpen(false)
    setReassignAgentId(undefined)
  }

  const handleCancel = (id: string) => {
    Modal.confirm({
      title: 'Cancelar agendamento?',
      okButtonProps: { danger: true },
      onOk: async () => {
        try {
          await cancelAppointment.mutateAsync(id)
          message.success('Agendamento cancelado')
          closeDetail()
        } catch (err) {
          message.error(extractApiError(err))
        }
      },
    })
  }

  const handleReassign = async () => {
    if (!selectedAppointment || !reassignAgentId) return
    try {
      await reassignAppointment.mutateAsync({ id: selectedAppointment.id, agentId: reassignAgentId })
      message.success('Agendamento reatribuído')
      closeReassignModal()
      closeDetail()
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  return {
    appointments,
    isLoading,
    filters,
    setFilters,
    selectedAppointment,
    openDetail,
    closeDetail,
    reassignModalOpen,
    reassignAgentId,
    setReassignAgentId,
    openReassignModal,
    closeReassignModal,
    reassignLoading: reassignAppointment.isPending,
    statusOptions: Object.entries(appointmentStatusLabel).map(([value, label]) => ({
      value: Number(value),
      label,
    })),
    serviceTypeOptions: serviceTypes.map((st) => ({ value: st.id, label: st.description })),
    agentOptions: agents.map((a) => ({ value: a.id, label: a.name })),
    refetch,
    handleCancel,
    handleReassign,
  }
}
