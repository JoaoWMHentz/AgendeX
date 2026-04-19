import type { ThemeConfig } from 'antd'

export const theme: ThemeConfig = {
  token: {
    // Brand
    colorPrimary: '#1677ff',
    colorSuccess: '#52c41a',
    colorWarning: '#faad14',
    colorError: '#ff4d4f',
    colorInfo: '#1677ff',

    // Typography
    fontFamily:
      "-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif",
    fontSize: 14,
    fontSizeLG: 16,
    fontSizeSM: 12,
    lineHeight: 1.5714,

    // Spacing
    padding: 16,
    paddingLG: 24,
    paddingSM: 12,
    paddingXS: 8,
    margin: 16,
    marginLG: 24,
    marginSM: 12,
    marginXS: 8,

    // Border
    borderRadius: 6,
    borderRadiusLG: 8,
    borderRadiusSM: 4,
    lineWidth: 1,
    lineType: 'solid',

    // Motion
    motionDurationFast: '0.1s',
    motionDurationMid: '0.2s',
    motionDurationSlow: '0.3s',

    // Shadow
    boxShadow:
      '0 1px 2px 0 rgba(0,0,0,0.03), 0 1px 6px -1px rgba(0,0,0,0.02), 0 2px 4px 0 rgba(0,0,0,0.02)',
    boxShadowSecondary:
      '0 6px 16px 0 rgba(0,0,0,0.08), 0 3px 6px -4px rgba(0,0,0,0.12), 0 9px 28px 8px rgba(0,0,0,0.05)',
  },
  components: {
    Layout: {
      siderBg: '#001529',
      headerBg: '#ffffff',
      headerHeight: 56,
      headerPadding: '0 24px',
      bodyBg: '#f5f5f5',
      footerBg: '#f5f5f5',
    },
    Menu: {
      darkItemBg: '#001529',
      darkItemSelectedBg: '#1677ff',
      darkItemHoverBg: 'rgba(255,255,255,0.08)',
      darkSubMenuItemBg: '#000c17',
    },
    Table: {
      headerBg: '#fafafa',
      rowHoverBg: '#f5f5f5',
      borderColor: '#f0f0f0',
    },
    Button: {
      primaryShadow: '0 2px 0 rgba(5,145,255,0.1)',
    },
    Card: {
      boxShadowTertiary:
        '0 1px 2px 0 rgba(0,0,0,0.03), 0 1px 6px -1px rgba(0,0,0,0.02), 0 2px 4px 0 rgba(0,0,0,0.02)',
    },
    Form: {
      itemMarginBottom: 20,
    },
  },
}
