import { useState } from 'react'
import { message, Modal } from 'antd'
import { useServiceTypes } from '@/features/service-types/useServiceTypes'
import { extractApiError } from '@/shared/utils/apiError'
import { useAppointments, useCancelAppointment } from '../useAppointments'
import { appointmentStatusLabel, type AppointmentFilters, type AppointmentStatusValue } from '../types'

export function useClientMyAppointmentsController() {
  const [filters, setFilters] = useState<AppointmentFilters>({})

  const { data: appointments = [], isLoading } = useAppointments(filters)
  const { data: serviceTypes = [] } = useServiceTypes()
  const cancelAppointment = useCancelAppointment()

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

  return {
    appointments,
    isLoading,
    filters,
    setFilters,
    statusOptions: Object.entries(appointmentStatusLabel).map(([value, label]) => ({
      value: Number(value),
      label,
    })),
    serviceTypeOptions: serviceTypes.map((st) => ({ value: st.id, label: st.description })),
    handleCancel,
  }
}
