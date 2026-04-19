import { Layout, Menu, Typography, Button, Space, Avatar, Dropdown } from 'antd'
import {
  CalendarOutlined,
  ClockCircleOutlined,
  TeamOutlined,
  AppstoreOutlined,
  UserOutlined,
  LogoutOutlined,
} from '@ant-design/icons'
import { Outlet, useNavigate, useLocation } from 'react-router-dom'
import { useAuthStore } from '@/features/auth/authStore'
import { authService } from '@/services/auth.service'
import { tokenStorage } from '@/services/tokenStorage'

const { Header, Sider, Content } = Layout
const { Title, Text } = Typography

const menuItems = [
  { key: '/appointments', icon: <CalendarOutlined />, label: 'Agendamentos' },
  { key: '/availability', icon: <ClockCircleOutlined />, label: 'Disponibilidade' },
  { key: '/users', icon: <TeamOutlined />, label: 'Usuários' },
  { key: '/service-types', icon: <AppstoreOutlined />, label: 'Tipos de Serviço' },
]

export function AppLayout() {
  const navigate = useNavigate()
  const location = useLocation()
  const { user, clearSession } = useAuthStore()

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
      key: 'logout',
      icon: <LogoutOutlined />,
      label: 'Sair',
      danger: true,
      onClick: handleLogout,
    },
  ]

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider theme="dark" breakpoint="lg" collapsedWidth={0}>
        <Title level={4} style={{ color: '#fff', textAlign: 'center', padding: '20px 0 16px', margin: 0 }}>
          AgendeX
        </Title>
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[location.pathname]}
          items={menuItems}
          onClick={({ key }) => navigate(key)}
        />
      </Sider>

      <Layout>
        <Header
          style={{
            background: '#fff',
            padding: '0 24px',
            borderBottom: '1px solid #f0f0f0',
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

        <Content style={{ margin: 16, padding: 24, background: '#fff', borderRadius: 8, minHeight: 280 }}>
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  )
}
