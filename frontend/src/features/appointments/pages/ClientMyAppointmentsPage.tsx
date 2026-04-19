import { AppointmentsList } from '../components/AppointmentsList'
import { useClientMyAppointmentsController } from '../hooks/useClientMyAppointmentsController'

export function ClientMyAppointmentsPage() {
  const {
    appointments,
    isLoading,
    filters,
    setFilters,
    statusOptions,
    serviceTypeOptions,
    handleCancel,
  } = useClientMyAppointmentsController()

  return (
    <AppointmentsList
      isAdmin={false}
      isAgent={false}
      isClient={true}
      appointments={appointments}
      loading={isLoading}
      filters={filters}
      statusOptions={statusOptions}
      serviceTypeOptions={serviceTypeOptions}
      agentOptions={[]}
      onFiltersChange={setFilters}
      onConfirm={() => {}}
      onOpenReject={() => {}}
      onCancel={handleCancel}
      onOpenComplete={() => {}}
      onOpenReassign={() => {}}
    />
  )
}
