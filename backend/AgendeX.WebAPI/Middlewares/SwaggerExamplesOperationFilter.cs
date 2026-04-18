using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace AgendeX.WebAPI.Middlewares;

public sealed class SwaggerExamplesOperationFilter : IOperationFilter
{
    private static readonly Dictionary<string, JsonNode> _examples = new()
    {
        ["AppointmentsController.Create"] = new JsonObject
        {
            ["title"] = "Consulta Inicial",
            ["description"] = "Primeira consulta do cliente",
            ["serviceTypeId"] = 1,
            ["agentId"] = "00000000-0000-0000-0000-000000000000",
            ["date"] = "2026-04-25",
            ["time"] = "09:00:00",
            ["notes"] = JsonValue.Create<string?>(null)
        },
        ["AppointmentsController.Reject"] = new JsonObject
        {
            ["rejectionReason"] = "Horário indisponível para o agente."
        },
        ["AppointmentsController.Complete"] = new JsonObject
        {
            ["serviceSummary"] = "Atendimento realizado com sucesso."
        },
        ["AppointmentsController.Reassign"] = new JsonObject
        {
            ["newAgentId"] = "00000000-0000-0000-0000-000000000000"
        },
        ["AvailabilityController.Create"] = new JsonObject
        {
            ["agentId"] = "00000000-0000-0000-0000-000000000000",
            ["weekDay"] = 1,
            ["startTime"] = "08:00:00",
            ["endTime"] = "17:00:00"
        },
        ["AvailabilityController.Update"] = new JsonObject
        {
            ["startTime"] = "09:00:00",
            ["endTime"] = "18:00:00"
        },
        ["UsersController.Create"] = new JsonObject
        {
            ["name"] = "João Silva",
            ["email"] = "joao@agendex.local",
            ["password"] = "senha@123",
            ["role"] = 2
        },
        ["UsersController.Update"] = new JsonObject
        {
            ["name"] = "João Silva Atualizado"
        },
        ["UsersController.SetClientDetail"] = new JsonObject
        {
            ["cpf"] = "000.000.000-00",
            ["birthDate"] = "1990-01-15",
            ["phone"] = "48999990000",
            ["notes"] = JsonValue.Create<string?>(null)
        }
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        string key = $"{context.MethodInfo.DeclaringType?.Name}.{context.MethodInfo.Name}";

        if (!_examples.TryGetValue(key, out JsonNode? example)) return;
        if (operation.RequestBody is null) return;

        foreach (OpenApiMediaType mediaType in operation.RequestBody.Content.Values)
        {
            mediaType.Example = example!.DeepClone()!;
        }
    }
}
