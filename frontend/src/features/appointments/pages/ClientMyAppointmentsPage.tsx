import { AppointmentsList } from '../components/AppointmentsList'
import { AppointmentDetailModal } from '../components/AppointmentDetailModal'
import { useClientMyAppointmentsController } from '../hooks/useClientMyAppointmentsController'

export function ClientMyAppointmentsPage() {
  const {
    appointments,
    isLoading,
    filters,
    setFilters,
    selectedAppointment,
    openDetail,
    closeDetail,
    statusOptions,
    serviceTypeOptions,
    refetch,
    handleCancel,
  } = useClientMyAppointmentsController()

  return (
    <>
      <AppointmentsList
        title="Seus Agendamentos"
        appointments={appointments}
        loading={isLoading}
        filters={filters}
        statusOptions={statusOptions}
        serviceTypeOptions={serviceTypeOptions}
        showClientColumn={false}
        showAgentColumn={false}
        onFiltersChange={setFilters}
        onRowClick={openDetail}
        onRefresh={refetch}
      />

      <AppointmentDetailModal
        appointment={selectedAppointment}
        onClose={closeDetail}
        onCancel={handleCancel}
        cancelRequiresFuture
      />
    </>
  )
}
