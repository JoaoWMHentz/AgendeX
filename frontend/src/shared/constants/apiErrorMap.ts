export type ApiErrorMapEntry = {
  key: string
  ptBr: string
}

export const apiErrorMap: ApiErrorMapEntry[] = [
  { key: 'invalid credentials', ptBr: 'Credenciais inválidas. Verifique e-mail e senha.' },
  { key: 'invalid refresh token', ptBr: 'Token de atualização inválido. Faça login novamente.' },
  { key: 'you are not the assigned agent for this appointment', ptBr: 'Você não é o agente responsável por este agendamento.' },
  { key: 'you can only cancel your own appointments', ptBr: 'Você só pode cancelar seus próprios agendamentos.' },
  { key: 'only pending appointments can be confirmed', ptBr: 'Somente agendamentos pendentes podem ser confirmados.' },
  { key: 'only pending appointments can be rejected', ptBr: 'Somente agendamentos pendentes podem ser rejeitados.' },
  { key: 'only confirmed appointments can be marked as completed', ptBr: 'Somente agendamentos confirmados podem ser concluídos.' },
  { key: 'cannot complete an appointment that has not occurred yet', ptBr: 'Não é possível concluir um agendamento que ainda não ocorreu.' },
  { key: 'appointment is already completed or canceled', ptBr: 'O agendamento já está concluído ou cancelado.' },
  { key: 'appointment cannot be canceled at its current status', ptBr: 'O agendamento não pode ser cancelado no status atual.' },
  { key: 'cannot cancel an appointment that has already occurred', ptBr: 'Não é possível cancelar um agendamento que já ocorreu.' },
  { key: 'cannot reassign a completed or canceled appointment', ptBr: 'Não é possível reatribuir um agendamento concluído ou cancelado.' },
  { key: 'the selected time is not within any active availability window for this agent', ptBr: 'O horário selecionado não está dentro de uma janela ativa de disponibilidade deste agente.' },
  { key: 'the agent already has an appointment at this time', ptBr: 'O agente já possui um agendamento neste horário.' },
  { key: 'client details can only be set for users with role', ptBr: 'Os dados de cliente só podem ser definidos para usuários com perfil Cliente.' },
  { key: 'is already in use', ptBr: 'Este e-mail já está em uso.' },
  { key: 'only administrator and agent can access reports', ptBr: 'Apenas Administrador e Agente podem acessar relatórios.' },
  { key: 'availability interval overlaps with an existing slot', ptBr: 'O intervalo de disponibilidade conflita com outro já cadastrado.' },
  { key: 'conflito de disponibilidade', ptBr: 'Há conflito de disponibilidade para o período informado.' },
  { key: 'at least one week day must be informed', ptBr: 'Informe pelo menos um dia da semana.' },
  { key: 'week days must not contain duplicates', ptBr: 'Os dias da semana não podem conter duplicidades.' },
  { key: 'only monday to friday is allowed', ptBr: 'Somente segunda a sexta-feira é permitido.' },
  { key: 'endtime must be after starttime', ptBr: 'O horário final deve ser maior que o horário inicial.' },
  { key: 'slotdurationminutes must be 30 or 60', ptBr: 'A duração do slot deve ser 30 ou 60 minutos.' },
  { key: 'the time range must be exactly divisible by the slot duration', ptBr: 'Não foi possível dividir o período com a duração de slot selecionada. Ajuste o horário de início/fim para fechar exatamente (ex.: slot 30 min: 08:00-09:30; slot 60 min: 08:00-10:00).' },
  { key: 'date cannot be in the past', ptBr: 'A data não pode estar no passado.' },
  { key: 'from date must be less than or equal to to date', ptBr: 'A data inicial deve ser menor ou igual à data final.' },
  { key: 'sortby is invalid', ptBr: 'O campo de ordenação informado é inválido.' },
]
