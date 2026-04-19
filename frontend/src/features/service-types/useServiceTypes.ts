import { useQuery } from '@tanstack/react-query'
import { serviceTypesService } from '@/services/service-types.service'
import { queryKeys } from '@/shared/queryKeys'

type QueryOptions = {
  enabled?: boolean
}

export function useServiceTypes(options?: QueryOptions) {
  return useQuery({
    queryKey: queryKeys.serviceTypes.all,
    queryFn: serviceTypesService.getAll,
    staleTime: Infinity,
    enabled: options?.enabled ?? true,
  })
}
