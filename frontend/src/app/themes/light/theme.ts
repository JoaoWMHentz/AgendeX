import { theme as antdTheme, type ThemeConfig } from 'antd'

export const lightTheme: ThemeConfig = {
  algorithm: antdTheme.defaultAlgorithm,
  token: {
    colorPrimary: '#1677ff',
    colorSuccess: '#52c41a',
    colorWarning: '#faad14',
    colorError: '#ff4d4f',
    colorInfo: '#1677ff',
    colorBgLayout: '#f5f7fb',
    colorBgContainer: '#ffffff',
    colorTextBase: '#10233d',
    colorBorder: '#d9e2f2',
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
      '0 1px 2px 0 rgba(15,23,42,0.04), 0 1px 6px -1px rgba(15,23,42,0.08), 0 2px 4px 0 rgba(15,23,42,0.06)',
    boxShadowSecondary:
      '0 12px 24px 0 rgba(15,23,42,0.08), 0 4px 10px -4px rgba(15,23,42,0.12), 0 16px 32px 8px rgba(15,23,42,0.04)',
  },
}