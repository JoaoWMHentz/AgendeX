import { useState } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Form, Input, Button, Card, Typography, Alert, Space } from 'antd'
import { MailOutlined, LockOutlined } from '@ant-design/icons'
import { useNavigate, useLocation } from 'react-router-dom'
import { authService } from '@/services/auth.service'
import { useAuthStore } from './authStore'
import type { AxiosError } from 'axios'

const { Title, Text } = Typography

const schema = z.object({
  email: z.string().email('Informe um e-mail válido'),
  password: z.string().min(1, 'Senha é obrigatória'),
})

type LoginForm = z.infer<typeof schema>

export function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const setSession = useAuthStore((s) => s.setSession)
  const [apiError, setApiError] = useState<string | null>(null)

  const from = (location.state as { from?: string } | null)?.from ?? '/'

  const {
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginForm>({
    resolver: zodResolver(schema),
    defaultValues: { email: '', password: '' },
  })

  const onSubmit = async (values: LoginForm) => {
    try {
      setApiError(null)
      const response = await authService.login(values)
      setSession(response)
      navigate(from, { replace: true })
    } catch (err) {
      const axiosErr = err as AxiosError<{ message?: string }>
      const message =
        axiosErr.response?.status === 401
          ? 'E-mail ou senha inválidos.'
          : axiosErr.response?.status === 429
            ? 'Muitas tentativas. Aguarde um momento.'
            : (axiosErr.response?.data?.message ?? 'Erro ao fazer login. Tente novamente.')
      setApiError(message)
    }
  }

  return (
    <div
      style={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: '#ebebeb',
      }}
    >
      <Card style={{ width: 400, boxShadow: '0 4px 24px rgba(0,0,0,0.08)' }}>
        <Space direction="vertical" size={8} style={{ width: '100%', marginBottom: 28, textAlign: 'center' }}>
          <Title level={3} style={{ margin: 0 }}>
            AgendeX
          </Title>
          <Text type="secondary">Faça login para continuar</Text>
        </Space>

        {apiError && (
          <Alert
            message={apiError}
            type="error"
            showIcon
            style={{ marginBottom: 20 }}
            closable
            onClose={() => setApiError(null)}
          />
        )}

        <Form layout="vertical" onFinish={handleSubmit(onSubmit)}>
          <Form.Item
            required
            label="E-mail"
            validateStatus={errors.email ? 'error' : ''}
            help={errors.email?.message}
          >
            <Controller
              name="email"
              control={control}
              render={({ field }) => (
                <Input
                  {...field}
                  prefix={<MailOutlined />}
                  placeholder="seu@email.com"
                  size="large"
                  autoComplete="email"
                />
              )}
            />
          </Form.Item>

          <Form.Item
            required
            label="Senha"
            validateStatus={errors.password ? 'error' : ''}
            help={errors.password?.message}
          >
            <Controller
              name="password"
              control={control}
              render={({ field }) => (
                <Input.Password
                  {...field}
                  prefix={<LockOutlined />}
                  placeholder="••••••••"
                  size="large"
                  autoComplete="current-password"
                />
              )}
            />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0, marginTop: 8 }}>
            <Button
              type="primary"
              htmlType="submit"
              size="large"
              block
              loading={isSubmitting}
            >
              Entrar
            </Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  )
}
