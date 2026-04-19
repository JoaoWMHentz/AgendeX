import { useState } from 'react'
import { message, Modal } from 'antd'
import { useServiceTypes } from '@/features/service-types/useServiceTypes'
import { notifyApiError } from '@/shared/utils/notifyApiError'
import { getDefaultPeriodRange } from '@/shared/utils/date'
import {
  useAppointments,
  useCompleteAppointment,
  useConfirmAppointment,
  useRejectAppointment,
} from '../useAppointments'
import { appointmentStatusLabel, type Appointment, type AppointmentFilters } from '../types'

type SubModal = 'reject' | 'complete' | null

export function useAgentMyAppointmentsController() {
  const [filters, setFilters] = useState<AppointmentFilters>(() => getDefaultPeriodRange())
  const [selectedAppointment, setSelectedAppointment] = useState<Appointment | null>(null)
  const [subModal, setSubModal] = useState<SubModal>(null)
  const [rejectReason, setRejectReason] = useState('')
  const [completeSummary, setCompleteSummary] = useState('')

  const { data: appointments = [], isLoading, refetch } = useAppointments(filters)
  const { data: serviceTypes = [] } = useServiceTypes()

  const confirmAppointment = useConfirmAppointment()
  const rejectAppointment = useRejectAppointment()
  const completeAppointment = useCompleteAppointment()

  const openDetail = (appointment: Appointment) => setSelectedAppointment(appointment)
  const closeDetail = () => {
    setSelectedAppointment(null)
    setSubModal(null)
    setRejectReason('')
    setCompleteSummary('')
  }

  const openRejectModal = (appointment: Appointment) => {
    setSelectedAppointment(appointment)
    setSubModal('reject')
  }
  const openCompleteModal = (appointment: Appointment) => {
    setSelectedAppointment(appointment)
    setSubModal('complete')
  }
  const closeSubModal = () => {
    setSubModal(null)
    setRejectReason('')
    setCompleteSummary('')
  }

  const handleConfirm = (id: string) => {
    Modal.confirm({
      title: 'Confirmar agendamento?',
      onOk: async () => {
        try {
          await confirmAppointment.mutateAsync(id)
          message.success('Agendamento confirmado')
          closeDetail()
        } catch (err) {
          notifyApiError(err)
        }
      },
    })
  }

  const handleReject = async () => {
    if (!selectedAppointment || !rejectReason.trim()) return
    try {
      await rejectAppointment.mutateAsync({ id: selectedAppointment.id, reason: rejectReason })
      message.success('Agendamento rejeitado')
      closeDetail()
    } catch (err) {
      notifyApiError(err)
    }
  }

  const handleComplete = async () => {
    if (!selectedAppointment) return
    try {
      await completeAppointment.mutateAsync({
        id: selectedAppointment.id,
        summary: completeSummary || undefined,
      })
      message.success('Agendamento concluído')
      closeDetail()
    } catch (err) {
      notifyApiError(err)
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
    subModal,
    openRejectModal,
    openCompleteModal,
    closeSubModal,
    rejectReason,
    setRejectReason,
    completeSummary,
    setCompleteSummary,
    rejectLoading: rejectAppointment.isPending,
    completeLoading: completeAppointment.isPending,
    statusOptions: Object.entries(appointmentStatusLabel).map(([value, label]) => ({
      value: Number(value),
      label,
    })),
    serviceTypeOptions: serviceTypes.map((st) => ({ value: st.id, label: st.description })),
    refetch,
    handleConfirm,
    handleReject,
    handleComplete,
  }
}
