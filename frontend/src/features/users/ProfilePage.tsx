import { Card, Descriptions, Typography, Button, Form, Input, Tag, Spin, message, Space } from 'antd'
import { UserOutlined } from '@ant-design/icons'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useCurrentUserProfile, useUpdateUserName } from './useUsers'
import { userRoleLabel } from './types'
import { useAuthStore } from '@/features/auth/authStore'
import { extractApiError } from '@/shared/utils/apiError'
import dayjs from 'dayjs'

const { Title } = Typography

const schema = z.object({ name: z.string().min(1, 'Nome é obrigatório') })
type NameForm = z.infer<typeof schema>

export function ProfilePage() {
  const me = useAuthStore((s) => s.user)
  const { data: profile, isLoading } = useCurrentUserProfile()
  const updateName = useUpdateUserName()

  const { control, handleSubmit, formState: { errors, isSubmitting, isDirty } } = useForm<NameForm>({
    resolver: zodResolver(schema),
    values: { name: profile?.name ?? '' },
  })

  const onSubmit = async (values: NameForm) => {
    if (!me?.id) return
    try {
      await updateName.mutateAsync({ id: me.id, name: values.name })
      message.success('Nome atualizado com sucesso')
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: 64 }}>
        <Spin size="large" />
      </div>
    )
  }

  if (!profile) return null

  return (
    <div style={{ maxWidth: 640 }}>
      <Space align="center" style={{ marginBottom: 24 }}>
        <div
          style={{
            width: 48, height: 48, borderRadius: '50%',
            background: '#1677ff', display: 'flex',
            alignItems: 'center', justifyContent: 'center',
          }}
        >
          <UserOutlined style={{ color: '#fff', fontSize: 22 }} />
        </div>
        <Title level={4} style={{ margin: 0 }}>Meu Perfil</Title>
      </Space>

      {/* Edit name */}
      <Card title="Informações pessoais" style={{ marginBottom: 16 }}>
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)}>
          <Form.Item
            label="Nome"
            validateStatus={errors.name ? 'error' : ''}
            help={errors.name?.message}
          >
            <Controller
              name="name"
              control={control}
              render={({ field }) => <Input {...field} size="large" />}
            />
          </Form.Item>

          <Descriptions column={1} size="small">
            <Descriptions.Item label="E-mail">{profile.email}</Descriptions.Item>
            <Descriptions.Item label="Perfil">
              <Tag>{userRoleLabel[profile.role]}</Tag>
            </Descriptions.Item>
            <Descriptions.Item label="Status">
              <Tag color={profile.isActive ? 'green' : 'default'}>
                {profile.isActive ? 'Ativo' : 'Inativo'}
              </Tag>
            </Descriptions.Item>
            <Descriptions.Item label="Membro desde">
              {dayjs(profile.createdAt).format('DD/MM/YYYY')}
            </Descriptions.Item>
          </Descriptions>

          <Button
            type="primary"
            htmlType="submit"
            loading={isSubmitting}
            disabled={!isDirty}
            style={{ marginTop: 16 }}
          >
            Salvar nome
          </Button>
        </Form>
      </Card>

      {/* Client detail (read-only) */}
      {profile.clientDetail && (
        <Card title="Dados do cliente">
          <Descriptions column={1} size="small">
            <Descriptions.Item label="CPF">{profile.clientDetail.cpf}</Descriptions.Item>
            <Descriptions.Item label="Data de nascimento">
              {dayjs(profile.clientDetail.birthDate).format('DD/MM/YYYY')}
            </Descriptions.Item>
            <Descriptions.Item label="Telefone">{profile.clientDetail.phone}</Descriptions.Item>
            {profile.clientDetail.notes && (
              <Descriptions.Item label="Observações">{profile.clientDetail.notes}</Descriptions.Item>
            )}
          </Descriptions>
        </Card>
      )}
    </div>
  )
}
