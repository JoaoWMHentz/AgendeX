import { UsersList } from '../components/UsersList'
import { CreateUserModal } from '../components/CreateUserModal'
import { EditUserModal } from '../components/EditUserModal'
import { useUsersPageController } from '../hooks/useUsersPageController'

export function UsersPage() {
  const {
    isAdmin,
    users,
    isLoading,
    setRoleFilter,
    createOpen,
    openCreateModal,
    closeCreateModal,
    createLoading,
    editTarget,
    closeEditModal,
    updateLoading,
    handleCreate,
    handleUpdate,
    handleDelete,
    handleOpenEdit,
  } = useUsersPageController()

  return (
    <>
      <UsersList
        users={users}
        loading={isLoading}
        isAdmin={isAdmin}
        onRoleFilterChange={setRoleFilter}
        onOpenCreate={openCreateModal}
        onEdit={handleOpenEdit}
        onDelete={handleDelete}
      />

      <CreateUserModal
        open={createOpen}
        loading={createLoading}
        onClose={closeCreateModal}
        onSubmit={handleCreate}
      />

      <EditUserModal
        open={!!editTarget}
        loading={updateLoading}
        isAdmin={isAdmin}
        targetUser={editTarget}
        onClose={closeEditModal}
        onSubmit={handleUpdate}
      />
    </>
  )
}
