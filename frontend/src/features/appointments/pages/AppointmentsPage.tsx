import { AppointmentDetailModal } from '../components/AppointmentDetailModal'
import { AppointmentsList } from '../components/AppointmentsList'
import { ReassignAppointmentModal } from '../components/ReassignAppointmentModal'
import { useAppointmentsPageController } from '../hooks/useAppointmentsPageController'

export function AppointmentsPage() {
  const {
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
    reassignLoading,
    statusOptions,
    serviceTypeOptions,
    agentOptions,
    refetch,
    handleCancel,
    handleReassign,
  } = useAppointmentsPageController()

  return (
    <>
      <AppointmentsList
        appointments={appointments}
        loading={isLoading}
        filters={filters}
        statusOptions={statusOptions}
        serviceTypeOptions={serviceTypeOptions}
        agentOptions={agentOptions}
        showAgentFilter
        onFiltersChange={setFilters}
        onRowClick={openDetail}
        onRefresh={refetch}
      />

      <AppointmentDetailModal
        appointment={selectedAppointment}
        onClose={closeDetail}
        onCancel={handleCancel}
        onOpenReassign={openReassignModal}
      />

      <ReassignAppointmentModal
        open={reassignModalOpen}
        loading={reassignLoading}
        value={reassignAgentId}
        options={agentOptions}
        onChange={setReassignAgentId}
        onClose={closeReassignModal}
        onSubmit={handleReassign}
      />
    </>
  )
}
