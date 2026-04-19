import { useQuery } from '@tanstack/react-query'
import { serviceTypesService } from '@/services/service-types.service'
import { queryKeys } from '@/shared/queryKeys'

export function useServiceTypes() {
  return useQuery({
    queryKey: queryKeys.serviceTypes.all,
    queryFn: serviceTypesService.getAll,
    staleTime: Infinity,
  })
}
