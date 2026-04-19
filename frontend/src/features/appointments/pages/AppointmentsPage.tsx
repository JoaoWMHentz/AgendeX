import { AppointmentsList } from '../components/AppointmentsList'
import { CompleteAppointmentModal } from '../components/CompleteAppointmentModal'
import { ReassignAppointmentModal } from '../components/ReassignAppointmentModal'
import { RejectAppointmentModal } from '../components/RejectAppointmentModal'
import { useAppointmentsPageController } from '../hooks/useAppointmentsPageController'

export function AppointmentsPage() {
  const {
    isAdmin,
    isAgent,
    isClient,
    appointments,
    isLoading,
    filters,
    setFilters,
    statusOptions,
    serviceTypeOptions,
    agentOptions,
    modal,
    openRejectModal,
    openCompleteModal,
    openReassignModal,
    closeModal,
    rejectLoading,
    completeLoading,
    reassignLoading,
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
  } = useAppointmentsPageController()

  return (
    <>
      <AppointmentsList
        isAdmin={isAdmin}
        isAgent={isAgent}
        isClient={isClient}
        appointments={appointments}
        loading={isLoading}
        filters={filters}
        statusOptions={statusOptions}
        serviceTypeOptions={serviceTypeOptions}
        agentOptions={agentOptions}
        onFiltersChange={setFilters}
        onConfirm={handleConfirm}
        onOpenReject={openRejectModal}
        onCancel={handleCancel}
        onOpenComplete={openCompleteModal}
        onOpenReassign={openReassignModal}
      />

      <RejectAppointmentModal
        open={modal.type === 'reject'}
        loading={rejectLoading}
        reason={rejectReason}
        onReasonChange={setRejectReason}
        onClose={closeModal}
        onSubmit={handleReject}
      />

      <CompleteAppointmentModal
        open={modal.type === 'complete'}
        loading={completeLoading}
        summary={completeSummary}
        onSummaryChange={setCompleteSummary}
        onClose={closeModal}
        onSubmit={handleComplete}
      />

      <ReassignAppointmentModal
        open={modal.type === 'reassign'}
        loading={reassignLoading}
        value={reassignAgentId}
        options={agentOptions}
        onChange={setReassignAgentId}
        onClose={closeModal}
        onSubmit={handleReassign}
      />
    </>
  )
}
