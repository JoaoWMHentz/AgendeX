import { useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useQueries } from '@tanstack/react-query'
import { message } from 'antd'
import { useNavigate } from 'react-router-dom'
import { useServiceTypes } from '@/features/service-types/useServiceTypes'
import { useAgents } from '@/features/users/hooks/useUsers'
import { availabilityService } from '@/services/availability.service'
import { queryKeys } from '@/shared/queryKeys'
import { extractApiError } from '@/shared/utils/apiError'
import { useCreateAppointment } from '../useAppointments'

const schema = z.object({
  title: z.string().min(1, 'Título obrigatório'),
  description: z.string().optional(),
  serviceTypeId: z.number({ message: 'Selecione o tipo de serviço' }),
  notes: z.string().optional(),
})

type FormValues = z.infer<typeof schema>

export interface AvailableSlotRow {
  key: string
  agentId: string
  agentName: string
  startTime: string
  endTime: string
}

export interface SelectedSlot {
  agentId: string
  agentName: string
  startTime: string
  date: string
}

export function useClientNewAppointmentController() {
  const navigate = useNavigate()
  const form = useForm<FormValues>({ resolver: zodResolver(schema) })
  const [selectedDate, setSelectedDate] = useState<string | undefined>()
  const [selectedSlot, setSelectedSlot] = useState<SelectedSlot | undefined>()

  const { data: serviceTypes = [] } = useServiceTypes()
  const { data: agents = [] } = useAgents()

  const agentSlotQueries = useQueries({
    queries: agents.map((agent) => ({
      queryKey: queryKeys.availability.slots(agent.id, selectedDate ?? ''),
      queryFn: () => availabilityService.getSlots(agent.id, selectedDate!),
      enabled: !!selectedDate,
    })),
  })

  const slotsLoading = agentSlotQueries.some((q) => q.isFetching)

  const availableSlots = useMemo<AvailableSlotRow[]>(() => {
    if (!selectedDate) return []
    return agentSlotQueries.flatMap((query, i) => {
      const agent = agents[i]
      return (query.data ?? []).map((slot) => ({
        key: `${agent.id}-${slot.startTime}`,
        agentId: agent.id,
        agentName: agent.name,
        startTime: slot.startTime.slice(0, 5),
        endTime: slot.endTime.slice(0, 5),
      }))
    })
  }, [agentSlotQueries, agents, selectedDate])

  const createAppointment = useCreateAppointment()

  const openConfirm = (slot: AvailableSlotRow) => {
    setSelectedSlot({
      agentId: slot.agentId,
      agentName: slot.agentName,
      startTime: slot.startTime,
      date: selectedDate!,
    })
  }

  const closeConfirm = () => setSelectedSlot(undefined)

  const handleConfirm = form.handleSubmit(async (values) => {
    if (!selectedSlot) return
    try {
      await createAppointment.mutateAsync({
        ...values,
        agentId: selectedSlot.agentId,
        date: selectedSlot.date,
        time: selectedSlot.startTime,
      })
      message.success('Agendamento criado com sucesso')
      navigate('/client/my-appointments')
    } catch (err) {
      message.error(extractApiError(err))
      closeConfirm()
    }
  })

  return {
    form,
    selectedDate,
    setSelectedDate,
    selectedSlot,
    openConfirm,
    closeConfirm,
    handleConfirm,
    confirmLoading: createAppointment.isPending,
    serviceTypeOptions: serviceTypes.map((st) => ({ value: st.id, label: st.description })),
    availableSlots,
    slotsLoading,
  }
}
