import { useState } from 'react'
import { message } from 'antd'
import { useUsers, useCreateUser, useUpdateUser, useDeleteUser, useSetClientDetail } from './useUsers'
import { UserRole, type User, type UserRoleValue } from '../models/types'
import { useAuthStore } from '@/features/auth/authStore'
import { Roles } from '@/shared/constants/roles'
import { notifyApiError } from '@/shared/utils/notifyApiError'
import type { CreateUserFormValues } from '../components/CreateUserModal'
import type { EditUserFormValues } from '../components/EditUserModal'

export function useUsersPageController() {
  const { user: me } = useAuthStore()
  const isAdmin = me?.role === Roles.Administrator

  const [roleFilter, setRoleFilter] = useState<UserRoleValue | undefined>()
  const [createOpen, setCreateOpen] = useState(false)
  const [editTarget, setEditTarget] = useState<User | null>(null)

  const { data = [], isLoading } = useUsers(roleFilter)
  const createUser = useCreateUser()
  const updateUser = useUpdateUser()
  const deleteUser = useDeleteUser()
  const setClientDetail = useSetClientDetail()

  const openCreateModal = () => {
    setCreateOpen(true)
  }

  const closeCreateModal = () => {
    setCreateOpen(false)
  }

  const closeEditModal = () => {
    setEditTarget(null)
  }

  const handleCreate = async (values: CreateUserFormValues) => {
    try {
      const { confirmPassword: _, cpf, birthDate, phone, notes, ...payload } = values
      const created = await createUser.mutateAsync({ ...payload, role: payload.role as UserRoleValue })

      if (payload.role === UserRole.Client && cpf) {
        await setClientDetail.mutateAsync({
          id: created.id,
          data: { cpf, birthDate: birthDate ?? '', phone: phone ?? '', notes },
        })
      }

      message.success('Usuário criado com sucesso')
      closeCreateModal()
    } catch (err) {
      notifyApiError(err)
    }
  }

  const handleUpdate = async (values: EditUserFormValues) => {
    if (!editTarget) return

    try {
      await updateUser.mutateAsync({
        id: editTarget.id,
        data: {
          name: values.name,
          role: isAdmin ? (values.role as UserRoleValue) : undefined,
          isActive: isAdmin ? values.isActive : undefined,
        },
      })

      const effectiveRole = isAdmin ? values.role : editTarget.role
      if (effectiveRole === UserRole.Client && values.cpf) {
        await setClientDetail.mutateAsync({
          id: editTarget.id,
          data: {
            cpf: values.cpf,
            birthDate: values.birthDate ?? '',
            phone: values.phone ?? '',
            notes: values.notes,
          },
        })
      }

      message.success('Usuário atualizado')
      closeEditModal()
    } catch (err) {
      notifyApiError(err)
    }
  }

  const handleDelete = async (id: string) => {
    try {
      await deleteUser.mutateAsync(id)
      message.success('Usuário desativado')
    } catch (err) {
      notifyApiError(err)
    }
  }

  const handleOpenEdit = (user: User) => {
    setEditTarget(user)
  }

  return {
    isAdmin,
    users: data,
    isLoading,
    setRoleFilter,
    createOpen,
    openCreateModal,
    closeCreateModal,
    createLoading: createUser.isPending || setClientDetail.isPending,
    editTarget,
    closeEditModal,
    updateLoading: updateUser.isPending || setClientDetail.isPending,
    handleCreate,
    handleUpdate,
    handleDelete,
    handleOpenEdit,
  }
}
