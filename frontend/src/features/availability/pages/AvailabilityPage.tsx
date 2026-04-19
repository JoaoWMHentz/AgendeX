import { AvailabilityList } from '../components/AvailabilityList'
import { CreateAvailabilityModal } from '../components/CreateAvailabilityModal'
import { EditAvailabilityModal } from '../components/EditAvailabilityModal'
import { useAvailabilityPageController } from '../hooks/useAvailabilityPageController'

export function AvailabilityPage() {
  const {
    isAdmin,
    isAgent,
    selectedAgent,
    setSelectedAgent,
    selectedWeekDay,
    setSelectedWeekDay,
    agentOptions,
    availabilities,
    isLoading,
    createOpen,
    openCreateModal,
    closeCreateModal,
    createLoading,
    editTarget,
    openEditModal,
    closeEditModal,
    updateLoading,
    handleCreate,
    handleUpdate,
    handleDelete,
  } = useAvailabilityPageController()

  return (
    <>
      <AvailabilityList
        isAdmin={isAdmin}
        isAgent={isAgent}
        availabilities={availabilities}
        loading={isLoading}
        selectedAgent={selectedAgent}
        agentOptions={agentOptions}
        selectedWeekDay={selectedWeekDay}
        onAgentChange={setSelectedAgent}
        onWeekDayChange={setSelectedWeekDay}
        onOpenCreate={openCreateModal}
        onEdit={openEditModal}
        onDelete={handleDelete}
      />

      <CreateAvailabilityModal
        open={createOpen}
        loading={createLoading}
        agentOptions={agentOptions}
        onClose={closeCreateModal}
        onSubmit={handleCreate}
      />

      <EditAvailabilityModal
        open={!!editTarget}
        loading={updateLoading}
        targetAvailability={editTarget}
        onClose={closeEditModal}
        onSubmit={handleUpdate}
      />
    </>
  )
}
