import type { AxiosError } from 'axios'

interface ApiErrorBody {
  message?: string
  errors?: Record<string, string[]>
}

export function extractApiError(err: unknown, fallback = 'Ocorreu um erro inesperado.'): string {
  const axiosErr = err as AxiosError<ApiErrorBody>

  if (!axiosErr?.response) return 'Sem conexão com o servidor.'

  const { status, data } = axiosErr.response

  if (data?.errors) {
    const first = Object.values(data.errors).flat()[0]
    if (first) return first
  }

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
