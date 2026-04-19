import { Layout, Menu, Typography } from 'antd'
import {
  CalendarOutlined,
  ClockCircleOutlined,
  TeamOutlined,
  AppstoreOutlined,
} from '@ant-design/icons'
import { Outlet, useNavigate, useLocation } from 'react-router-dom'

const { Header, Sider, Content } = Layout
const { Title } = Typography

const menuItems = [
  { key: '/appointments', icon: <CalendarOutlined />, label: 'Agendamentos' },
  { key: '/availability', icon: <ClockCircleOutlined />, label: 'Disponibilidade' },
  { key: '/users', icon: <TeamOutlined />, label: 'Usuários' },
  { key: '/service-types', icon: <AppstoreOutlined />, label: 'Tipos de Serviço' },
]

export function AppLayout() {
  const navigate = useNavigate()
  const location = useLocation()

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
        <Header style={{ background: '#fff', padding: '0 24px', borderBottom: '1px solid #f0f0f0' }} />
        <Content style={{ margin: 16, padding: 24, background: '#fff', borderRadius: 8, minHeight: 280 }}>
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  )
}
