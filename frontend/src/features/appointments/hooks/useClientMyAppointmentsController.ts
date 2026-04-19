import { useState } from 'react'
import { message, Modal } from 'antd'
import { useServiceTypes } from '@/features/service-types/useServiceTypes'
import { notifyApiError } from '@/shared/utils/notifyApiError'
import { getDefaultPeriodRange } from '@/shared/utils/date'
import { useAppointments, useCancelAppointment } from '../useAppointments'
import {
  appointmentStatusLabel,
  type Appointment,
  type AppointmentFilters,
} from '../types'

export function useClientMyAppointmentsController() {
  const [filters, setFilters] = useState<AppointmentFilters>(() => getDefaultPeriodRange())
  const [selectedAppointment, setSelectedAppointment] = useState<Appointment | null>(null)

  const { data: appointments = [], isLoading, refetch } = useAppointments(filters)
  const { data: serviceTypes = [] } = useServiceTypes()
  const cancelAppointment = useCancelAppointment()

  const openDetail = (appointment: Appointment) => setSelectedAppointment(appointment)
  const closeDetail = () => setSelectedAppointment(null)

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
          notifyApiError(err)
        }
      },
    })
  }

  return {
    appointments,
    isLoading,
    filters,
    setFilters,
    selectedAppointment,
    openDetail,
    closeDetail,
    statusOptions: Object.entries(appointmentStatusLabel).map(([value, label]) => ({
      value: Number(value),
      label,
    })),
    serviceTypeOptions: serviceTypes.map((st) => ({ value: st.id, label: st.description })),
    refetch,
    handleCancel,
  }
}
