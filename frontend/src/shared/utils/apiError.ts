import type { AxiosError } from 'axios'
import { apiErrorMap } from '@/shared/constants/apiErrorMap'

interface ApiErrorBody {
  title?: string
  message?: string
  errors?: Record<string, string[]> | string[]
}

type ResolvedApiError = {
  userMessage: string
  serverMessage?: string
  status?: number
  mapped: boolean
}

function normalizeText(value: string) {
  return value.trim().toLowerCase()
}

function statusToPtBr(status?: number): string | undefined {
  switch (status) {
    case 400: return 'Dados inválidos. Verifique as informações enviadas.'
    case 401: return 'Sessão expirada ou inválida. Faça login novamente.'
    case 403: return 'Sem permissão para realizar esta ação.'
    case 404: return 'Recurso não encontrado.'
    case 409: return 'Conflito de dados. Este registro pode já existir.'
    case 422: return 'Dados inválidos. Revise os campos informados.'
    case 429: return 'Muitas tentativas. Aguarde um momento e tente novamente.'
    case 500: return 'Erro interno do servidor. Tente novamente em instantes.'
    case 503: return 'Serviço temporariamente indisponível. Tente novamente mais tarde.'
    default: return undefined
  }
}

function firstServerMessage(data?: ApiErrorBody): string | undefined {
  if (!data) return undefined

  if (Array.isArray(data.errors) && data.errors.length > 0) {
    return data.errors[0]
  }

  if (data.errors && !Array.isArray(data.errors)) {
    const firstValidationMessage = Object.values(data.errors).flat()[0]
    if (firstValidationMessage) return firstValidationMessage
  }

  return data.message ?? data.title
}

function mapServerMessageToPtBr(serverMessage?: string): string | undefined {
  if (!serverMessage) return undefined

  const normalized = normalizeText(serverMessage)
  const entry = apiErrorMap.find((item) => normalized.includes(item.key))
  return entry?.ptBr
}

export function resolveApiError(
  err: unknown,
  fallback = 'Ocorreu um erro inesperado.',
): ResolvedApiError {
  const axiosErr = err as AxiosError<ApiErrorBody>

  if (!axiosErr?.response) {
    return {
      userMessage: 'Sem conexão com o servidor.',
      mapped: true,
    }
  }

  const { status, data } = axiosErr.response
  const serverMessage = firstServerMessage(data)

  const mappedByText = mapServerMessageToPtBr(serverMessage)
  if (mappedByText) {
    return {
      userMessage: mappedByText,
      serverMessage,
      status,
      mapped: true,
    }
  }

  const mappedByStatus = statusToPtBr(status)
  if (mappedByStatus) {
    return {
      userMessage: mappedByStatus,
      serverMessage,
      status,
      mapped: true,
    }
  }

  return {
    userMessage: fallback,
    serverMessage,
    status,
    mapped: false,
  }
}

export function extractApiError(err: unknown, fallback = 'Ocorreu um erro inesperado.'): string {
  const resolved = resolveApiError(err, fallback)

  if (resolved.mapped) {
    return resolved.userMessage
  }

  if (resolved.serverMessage) {
    return `${resolved.userMessage} Server: ${resolved.serverMessage}`
  }

  return resolved.userMessage
}
