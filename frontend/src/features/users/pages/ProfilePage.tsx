import { Card, Descriptions, Typography, Button, Form, Input, Tag, Spin, message, Space, Switch, DatePicker, theme as antdTheme } from 'antd'
import { UserOutlined } from '@ant-design/icons'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import dayjs from 'dayjs'
import { useCurrentUserProfile, useUpdateUser, useSetClientDetail } from '../hooks/useUsers'
import { userRoleLabel, UserRole } from '../models/types'
import { useAuthStore } from '@/features/auth/authStore'
import { extractApiError } from '@/shared/utils/apiError'
import { maskCpf, maskPhone } from '@/shared/utils/masks'
import { resolveTheme, useThemeStore } from '@/app/theme'

const { Title } = Typography

const profileSchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório').max(120),
  isActive: z.boolean().optional(),
})
type ProfileForm = z.infer<typeof profileSchema>

const clientDetailSchema = z.object({
  cpf: z.string().min(11, 'CPF inválido').max(14),
  birthDate: z.string().min(1, 'Data obrigatória'),
  phone: z.string().min(1, 'Telefone obrigatório'),
  notes: z.string().optional(),
})
type ClientDetailForm = z.infer<typeof clientDetailSchema>

export function ProfilePage() {
  const me = useAuthStore((s) => s.user)
  const isClient = me?.role === 'Client'
  const { data: profile, isLoading } = useCurrentUserProfile()
  const updateUser = useUpdateUser()
  const setClientDetail = useSetClientDetail()
  const { token } = antdTheme.useToken()
  const resolvedTheme = useThemeStore((state) => resolveTheme(state.preference, state.systemTheme))
  const toggleTheme = useThemeStore((state) => state.toggleTheme)

  const profileForm = useForm<ProfileForm>({
    resolver: zodResolver(profileSchema),
    values: { name: profile?.name ?? '', isActive: profile?.isActive ?? true },
  })

  const clientDetailForm = useForm<ClientDetailForm>({
    resolver: zodResolver(clientDetailSchema),
    values: {
      cpf: profile?.clientDetail?.cpf ?? '',
      birthDate: profile?.clientDetail?.birthDate ?? '',
      phone: profile?.clientDetail?.phone ?? '',
      notes: profile?.clientDetail?.notes ?? '',
    },
  })

  const onSaveProfile = async (values: ProfileForm) => {
    if (!me?.id) return
    try {
      await updateUser.mutateAsync({ id: me.id, data: { name: values.name, isActive: values.isActive } })
      message.success('Perfil atualizado com sucesso')
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  const onSaveClientDetail = async (values: ClientDetailForm) => {
    if (!me?.id) return
    try {
      await setClientDetail.mutateAsync({ id: me.id, data: values })
      message.success('Dados do cliente atualizados')
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  if (isLoading) {
    return <div style={{ textAlign: 'center', padding: 64 }}><Spin size="large" /></div>
  }
  if (!profile) return null

  return (
    <div style={{ maxWidth: 640 }}>
      <Space align="center" style={{ marginBottom: 24, width: '100%', justifyContent: 'space-between', flexWrap: 'wrap', gap: 12 }}>
        <Space align="center">
          <div style={{
            width: 48, height: 48, borderRadius: '50%', background: token.colorPrimary,
            display: 'flex', alignItems: 'center', justifyContent: 'center',
          }}>
            <UserOutlined style={{ color: token.colorTextLightSolid, fontSize: 22 }} />
          </div>
          <Title level={4} style={{ margin: 0 }}>Meu Perfil</Title>
        </Space>
        <Button type="default" onClick={toggleTheme}>
          {resolvedTheme === 'dark' ? 'Usar tema claro' : 'Usar tema escuro'}
        </Button>
      </Space>

      <Card title="Informações pessoais" style={{ marginBottom: 16 }}>
        <Form layout="vertical" onFinish={profileForm.handleSubmit(onSaveProfile)}>
          <Form.Item
            required
            label="Nome"
            validateStatus={profileForm.formState.errors.name ? 'error' : ''}
            help={profileForm.formState.errors.name?.message}
          >
            <Controller
              name="name"
              control={profileForm.control}
              render={({ field }) => <Input {...field} size="large" />}
            />
          </Form.Item>

          {isClient && (
            <Form.Item label="Conta ativa">
              <Controller
                name="isActive"
                control={profileForm.control}
                render={({ field }) => (
                  <Switch checked={field.value} onChange={field.onChange} />
                )}
              />
            </Form.Item>
          )}

          <Descriptions column={1} size="small" style={{ marginBottom: 16 }}>
            <Descriptions.Item label="E-mail">{profile.email}</Descriptions.Item>
            <Descriptions.Item label="Perfil">
              <Tag>{userRoleLabel[profile.role]}</Tag>
            </Descriptions.Item>
            {!isClient && (
              <Descriptions.Item label="Status">
                <Tag color={profile.isActive ? 'green' : 'default'}>
                  {profile.isActive ? 'Ativo' : 'Inativo'}
                </Tag>
              </Descriptions.Item>
            )}
            <Descriptions.Item label="Membro desde">
              {dayjs(profile.createdAt).format('DD/MM/YYYY')}
            </Descriptions.Item>
          </Descriptions>

          <Button
            type="primary"
            htmlType="submit"
            loading={updateUser.isPending}
            disabled={!profileForm.formState.isDirty}
          >
            Salvar
          </Button>
        </Form>
      </Card>

      {profile.role === UserRole.Client && (
        <Card title="Dados do cliente">
          <Form layout="vertical" onFinish={clientDetailForm.handleSubmit(onSaveClientDetail)}>
            <Form.Item
              required
              label="CPF"
              validateStatus={clientDetailForm.formState.errors.cpf ? 'error' : ''}
              help={clientDetailForm.formState.errors.cpf?.message}
            >
              <Controller
                name="cpf"
                control={clientDetailForm.control}
                render={({ field }) => (
                  <Input
                    {...field}
                    placeholder="000.000.000-00"
                    onChange={(e) => field.onChange(maskCpf(e.target.value))}
                  />
                )}
              />
            </Form.Item>
            <Form.Item
              required
              label="Data de nascimento"
              validateStatus={clientDetailForm.formState.errors.birthDate ? 'error' : ''}
              help={clientDetailForm.formState.errors.birthDate?.message}
            >
              <Controller
                name="birthDate"
                control={clientDetailForm.control}
                render={({ field }) => (
                  <DatePicker
                    style={{ width: '100%' }}
                    format="DD/MM/YYYY"
                    value={field.value ? dayjs(field.value, 'YYYY-MM-DD') : null}
                    onChange={(date) => field.onChange(date ? date.format('YYYY-MM-DD') : '')}
                  />
                )}
              />
            </Form.Item>
            <Form.Item
              required
              label="Telefone"
              validateStatus={clientDetailForm.formState.errors.phone ? 'error' : ''}
              help={clientDetailForm.formState.errors.phone?.message}
            >
              <Controller
                name="phone"
                control={clientDetailForm.control}
                render={({ field }) => (
                  <Input
                    {...field}
                    placeholder="(00) 00000-0000"
                    onChange={(e) => field.onChange(maskPhone(e.target.value))}
                  />
                )}
              />
            </Form.Item>
            <Form.Item
              label="Observações"
              validateStatus={clientDetailForm.formState.errors.notes ? 'error' : ''}
              help={clientDetailForm.formState.errors.notes?.message}
            >
              <Controller
                name="notes"
                control={clientDetailForm.control}
                render={({ field }) => <Input {...field} />}
              />
            </Form.Item>
            <Button
              type="primary"
              htmlType="submit"
              loading={setClientDetail.isPending}
              disabled={!clientDetailForm.formState.isDirty}
            >
              Salvar dados do cliente
            </Button>
          </Form>
        </Card>
      )}
    </div>
  )
}
