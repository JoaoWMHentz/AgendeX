import { Layout, Menu, Typography, Button, Space, Avatar, Dropdown, theme as antdTheme } from 'antd'
import {
  CalendarOutlined,
  ClockCircleOutlined,
  TeamOutlined,
  AppstoreOutlined,
  UserOutlined,
  LogoutOutlined,
  ProfileOutlined,
  PlusCircleOutlined,
  UnorderedListOutlined,
} from '@ant-design/icons'
import { Outlet, useNavigate, useLocation } from 'react-router-dom'
import { useAuthStore } from '@/features/auth/authStore'
import { authService } from '@/services/auth.service'
import { tokenStorage } from '@/services/tokenStorage'
import { Roles, type Role } from '@/shared/constants/roles'
import { resolveTheme, useThemeStore } from '@/app/theme'

const { Header, Sider, Content } = Layout
const { Title, Text } = Typography

interface MenuItem {
  key: string
  icon: React.ReactNode
  label: string
  allowedRoles: Role[]
}

const menuConfig: MenuItem[] = [
  {
    key: '/appointments',
    icon: <CalendarOutlined />,
    label: 'Agendamentos',
    allowedRoles: [Roles.Administrator],
  },
  {
    key: '/availability',
    icon: <ClockCircleOutlined />,
    label: 'Disponibilidade',
    allowedRoles: [Roles.Administrator, Roles.Agent],
  },
  {
    key: '/users',
    icon: <TeamOutlined />,
    label: 'Usuários',
    allowedRoles: [Roles.Administrator],
  },
  {
    key: '/service-types',
    icon: <AppstoreOutlined />,
    label: 'Tipos de Serviço',
    allowedRoles: [Roles.Administrator],
  },
  {
    key: '/agent/my-appointments',
    icon: <UnorderedListOutlined />,
    label: 'Seus Agendamentos',
    allowedRoles: [Roles.Agent],
  },
  {
    key: '/client/new-appointment',
    icon: <PlusCircleOutlined />,
    label: 'Novo Agendamento',
    allowedRoles: [Roles.Client],
  },
  {
    key: '/client/my-appointments',
    icon: <UnorderedListOutlined />,
    label: 'Seus Agendamentos',
    allowedRoles: [Roles.Client],
  },
]

export function AppLayout() {
  const navigate = useNavigate()
  const location = useLocation()
  const { user, clearSession } = useAuthStore()
  const { token } = antdTheme.useToken()
  const resolvedTheme = useThemeStore((state) => resolveTheme(state.preference, state.systemTheme))

  const visibleMenuItems = menuConfig
    .filter((item) => user && item.allowedRoles.includes(user.role as Role))
    .map(({ key, icon, label }) => ({ key, icon, label }))

  const handleLogout = async () => {
    const refreshToken = tokenStorage.getRefresh()
    if (refreshToken) {
      try {
        await authService.logout(refreshToken)
      } catch {
        // proceed with local logout even if request fails
      }
    }
    clearSession()
    navigate('/login', { replace: true })
  }

  const userMenuItems = [
    {
      key: 'profile',
      icon: <ProfileOutlined />,
      label: 'Meu perfil',
      onClick: () => navigate('/profile'),
    },
    { type: 'divider' as const },
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: 'Sair',
      danger: true,
      onClick: handleLogout,
    },
  ]

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider className="app-liquid-sider" theme={resolvedTheme} breakpoint="lg" collapsedWidth={0}>
        <Title
          level={4}
          style={{ color: resolvedTheme === 'dark' ? token.colorTextLightSolid : token.colorText, textAlign: 'center', padding: '20px 0 16px', margin: 0 }}
        >
          AgendeX
        </Title>
        <Menu
          theme={resolvedTheme}
          mode="inline"
          selectedKeys={[location.pathname]}
          items={visibleMenuItems}
          onClick={({ key }) => navigate(key)}
        />
      </Sider>

      <Layout>
        <Header
          style={{
            background: token.colorBgContainer,
            padding: '0 24px',
            borderBottom: `1px solid ${token.colorBorderSecondary}`,
            boxShadow: token.boxShadowSecondary,
            position: 'relative',
            zIndex: 1,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'flex-end',
          }}
        >
          <Dropdown menu={{ items: userMenuItems }} placement="bottomRight" arrow>
            <Button type="text" style={{ height: 'auto', padding: '4px 8px' }}>
              <Space>
                <Avatar size="small" icon={<UserOutlined />} />
                <Text strong>{user?.name}</Text>
                <Text type="secondary" style={{ fontSize: 12 }}>
                  {user?.role}
                </Text>
              </Space>
            </Button>
          </Dropdown>
        </Header>

        <Content
          style={{
            margin: 16,
            padding: 24,
            background: token.colorBgContainer,
            borderRadius: token.borderRadiusLG,
            minHeight: 280,
            border: `1px solid ${token.colorBorderSecondary}`,
            boxShadow: token.boxShadow,
          }}
        >
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  )
}
