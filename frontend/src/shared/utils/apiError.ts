import type { AxiosError } from 'axios'

interface ApiErrorBody {
  title?: string
  message?: string
  errors?: Record<string, string[]> | string[]
}

export function extractApiError(err: unknown, fallback = 'Ocorreu um erro inesperado.'): string {
  const axiosErr = err as AxiosError<ApiErrorBody>

  if (!axiosErr?.response) return 'Sem conexão com o servidor.'

  const { status, data } = axiosErr.response

  if (Array.isArray(data?.errors) && data.errors.length > 0) {
    return data.errors[0]
  }

  if (data?.errors && !Array.isArray(data.errors)) {
    const first = Object.values(data.errors).flat()[0]
    if (first) return first
  }

  if (data?.title) return data.title
  if (data?.message) return data.message

  switch (status) {
    case 400: return 'Dados inválidos. Verifique as informações.'
    case 401: return 'Sessão expirada. Faça login novamente.'
    case 403: return 'Sem permissão para realizar esta ação.'
    case 404: return 'Recurso não encontrado.'
    case 409: return 'Conflito: registro já existe.'
    case 429: return 'Muitas tentativas. Aguarde um momento.'
    case 500: return 'Erro interno do servidor. Tente novamente.'
    default:  return fallback
  }
}
