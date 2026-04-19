import { theme as antdTheme, type ThemeConfig } from 'antd'

export const darkTheme: ThemeConfig = {
  algorithm: antdTheme.darkAlgorithm,
  token: {
    colorPrimary: '#4c8dff',
    colorSuccess: '#5ad45a',
    colorWarning: '#f4b942',
    colorError: '#ff6b6b',
    colorInfo: '#4c8dff',
    colorBgLayout: '#0b1120',
    colorBgContainer: '#111827',
    colorTextBase: '#e5eefc',
    colorBorder: '#273449',
    fontFamily:
      "-apple-system, BlinkMacSystemFont, 'Segoe UI', Inter, 'Helvetica Neue', Arial, sans-serif",
    fontSize: 14,
    fontSizeLG: 16,
    fontSizeSM: 12,
    lineHeight: 1.5714,
    padding: 16,
    paddingLG: 24,
    paddingSM: 12,
    paddingXS: 8,
    margin: 16,
    marginLG: 24,
    marginSM: 12,
    marginXS: 8,
    borderRadius: 8,
    borderRadiusLG: 12,
    borderRadiusSM: 6,
    lineWidth: 1,
    lineType: 'solid',
    motionDurationFast: '0.1s',
    motionDurationMid: '0.2s',
    motionDurationSlow: '0.3s',
    boxShadow:
      '0 1px 2px 0 rgba(2,6,23,0.28), 0 8px 24px -8px rgba(2,6,23,0.36)',
    boxShadowSecondary:
      '0 16px 32px 0 rgba(2,6,23,0.32), 0 8px 16px -8px rgba(2,6,23,0.38), 0 24px 48px 12px rgba(2,6,23,0.18)',
  },
}