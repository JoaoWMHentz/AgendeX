import { AppointmentDetailModal } from '../components/AppointmentDetailModal'
import { AppointmentsList } from '../components/AppointmentsList'
import { CompleteAppointmentModal } from '../components/CompleteAppointmentModal'
import { RejectAppointmentModal } from '../components/RejectAppointmentModal'
import { useAgentMyAppointmentsController } from '../hooks/useAgentMyAppointmentsController'

export function AgentMyAppointmentsPage() {
  const {
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
    rejectLoading,
    completeLoading,
    statusOptions,
    serviceTypeOptions,
    refetch,
    handleConfirm,
    handleReject,
    handleComplete,
  } = useAgentMyAppointmentsController()

  return (
    <>
      <AppointmentsList
        title="Seus Agendamentos"
        appointments={appointments}
        loading={isLoading}
        filters={filters}
        statusOptions={statusOptions}
        serviceTypeOptions={serviceTypeOptions}
        showAgentColumn={false}
        onFiltersChange={setFilters}
        onRowClick={openDetail}
        onRefresh={refetch}
      />

      <AppointmentDetailModal
        appointment={selectedAppointment}
        onClose={closeDetail}
        onConfirm={handleConfirm}
        onOpenReject={openRejectModal}
        onOpenComplete={openCompleteModal}
      />

      <RejectAppointmentModal
        open={subModal === 'reject'}
        loading={rejectLoading}
        reason={rejectReason}
        onReasonChange={setRejectReason}
        onClose={closeSubModal}
        onSubmit={handleReject}
      />

      <CompleteAppointmentModal
        open={subModal === 'complete'}
        loading={completeLoading}
        summary={completeSummary}
        onSummaryChange={setCompleteSummary}
        onClose={closeSubModal}
        onSubmit={handleComplete}
      />
    </>
  )
}
