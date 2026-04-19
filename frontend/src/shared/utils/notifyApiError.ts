import { message } from 'antd'
import { extractApiError } from './apiError'

export function notifyApiError(err: unknown, fallback = 'Ocorreu um erro inesperado.') {
  message.error(extractApiError(err, fallback))
}
