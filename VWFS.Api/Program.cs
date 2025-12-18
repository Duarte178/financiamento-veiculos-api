using Microsoft.OpenApi.Models;
using VWFS.Api.Dtos;
using VWFS.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "VWFS API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// “Banco” em memória
var contratos = new List<Contrato>();

// =========================
// 1) CRUD de Contratos
// =========================

app.MapGet("/api/contratos", () => Results.Ok(contratos));

app.MapGet("/api/contratos/{id:guid}", (Guid id) =>
{
    var contrato = contratos.FirstOrDefault(c => c.Id == id);
    return contrato is null ? Results.NotFound() : Results.Ok(contrato);
});

app.MapPost("/api/contratos", (CriarContratoRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.ClienteCpfCnpj))
        return Results.BadRequest("clienteCpfCnpj é obrigatório.");

    if (request.PrazoMeses <= 0)
        return Results.BadRequest("prazoMeses deve ser maior que zero.");

    var contrato = new Contrato
    {
        Id = Guid.NewGuid(),
        ClienteCpfCnpj = request.ClienteCpfCnpj.Trim(),
        ValorTotal = request.ValorTotal,
        PrazoMeses = request.PrazoMeses,
        TaxaMensal = request.TaxaMensal,
        DataVencimentoPrimeiraParcela = request.DataVencimentoPrimeiraParcela.Date,
        TipoVeiculo = request.TipoVeiculo,
        CondicaoVeiculo = request.CondicaoVeiculo,
        Pagamentos = new List<Pagamento>()
    };

    contratos.Add(contrato);

    return Results.Created($"/api/contratos/{contrato.Id}", contrato);
});

app.MapPut("/api/contratos/{id:guid}", (Guid id, AtualizarContratoRequest request) =>
{
    var contrato = contratos.FirstOrDefault(c => c.Id == id);
    if (contrato is null) return Results.NotFound();

    if (string.IsNullOrWhiteSpace(request.ClienteCpfCnpj))
        return Results.BadRequest("clienteCpfCnpj é obrigatório.");

    if (request.PrazoMeses <= 0)
        return Results.BadRequest("prazoMeses deve ser maior que zero.");

    contrato.ClienteCpfCnpj = request.ClienteCpfCnpj.Trim();
    contrato.ValorTotal = request.ValorTotal;
    contrato.PrazoMeses = request.PrazoMeses;
    contrato.TaxaMensal = request.TaxaMensal;
    contrato.DataVencimentoPrimeiraParcela = request.DataVencimentoPrimeiraParcela.Date;
    contrato.TipoVeiculo = request.TipoVeiculo;
    contrato.CondicaoVeiculo = request.CondicaoVeiculo;

    // Se reduzir prazo, remove pagamentos que ficaram fora
    contrato.Pagamentos = contrato.Pagamentos
        .Where(p => p.NumeroParcela >= 1 && p.NumeroParcela <= contrato.PrazoMeses)
        .ToList();

    return Results.NoContent();
});

app.MapDelete("/api/contratos/{id:guid}", (Guid id) =>
{
    var contrato = contratos.FirstOrDefault(c => c.Id == id);
    if (contrato is null) return Results.NotFound();

    contratos.Remove(contrato);
    return Results.NoContent();
});

// =========================
// 2) Pagamentos
// =========================

app.MapPost("/api/contratos/{id:guid}/pagamentos", (Guid id, RegistrarPagamentoRequest request) =>
{
    var contrato = contratos.FirstOrDefault(c => c.Id == id);
    if (contrato is null) return Results.NotFound("Contrato não encontrado.");

    if (request.NumeroParcela <= 0 || request.NumeroParcela > contrato.PrazoMeses)
        return Results.BadRequest($"numeroParcela deve estar entre 1 e {contrato.PrazoMeses}.");

    if (contrato.Pagamentos.Any(p => p.NumeroParcela == request.NumeroParcela))
        return Results.Conflict("Essa parcela já foi paga.");

    var pagamento = new Pagamento
    {
        Id = Guid.NewGuid(),
        NumeroParcela = request.NumeroParcela,
        DataPagamento = request.DataPagamento.Date
    };

    contrato.Pagamentos.Add(pagamento);

    return Results.Created($"/api/contratos/{id}/pagamentos", pagamento);
});

app.MapGet("/api/contratos/{id:guid}/pagamentos", (Guid id) =>
{
    var contrato = contratos.FirstOrDefault(c => c.Id == id);
    if (contrato is null) return Results.NotFound("Contrato não encontrado.");

    var lista = contrato.Pagamentos
        .OrderBy(p => p.NumeroParcela)
        .ToList();

    return Results.Ok(lista);
});

// =========================
// 3) Resumo por Cliente
// =========================

app.MapGet("/api/clientes/{cpfCnpj}/resumo", (string cpfCnpj) =>
{
    cpfCnpj = (cpfCnpj ?? "").Trim();

    var contratosCliente = contratos
        .Where(c => c.ClienteCpfCnpj == cpfCnpj)
        .ToList();

    if (!contratosCliente.Any())
        return Results.NotFound("Cliente sem contratos.");

    var hoje = DateTime.UtcNow.Date;

    var totalParcelas = contratosCliente.Sum(c => c.PrazoMeses);
    var parcelasPagas = contratosCliente.Sum(c => c.Pagamentos.Count);

    int parcelasEmAtraso = 0;
    int parcelasAVencer = 0;
    int pagasEmDia = 0;

    foreach (var contrato in contratosCliente)
    {
        for (int n = 1; n <= contrato.PrazoMeses; n++)
        {
            var vencimento = contrato.DataVencimentoPrimeiraParcela.AddMonths(n - 1).Date;
            var pagamento = contrato.Pagamentos.FirstOrDefault(p => p.NumeroParcela == n);

            if (pagamento is not null)
            {
                if (pagamento.DataPagamento.Date <= vencimento)
                    pagasEmDia++;
            }
            else
            {
                if (vencimento < hoje) parcelasEmAtraso++;
                else parcelasAVencer++;
            }
        }
    }

    double percentualPagasEmDia = totalParcelas == 0
        ? 0
        : Math.Round((double)pagasEmDia / totalParcelas * 100.0, 1);

    var contratosAtivos = contratosCliente.Count(c => c.Pagamentos.Count < c.PrazoMeses);

    var resumo = new ResumoClienteResponse
    {
        CpfCnpj = cpfCnpj,
        QuantidadeContratos = contratosCliente.Count,
        QuantidadeContratosAtivos = contratosAtivos,
        TotalParcelas = totalParcelas,
        ParcelasPagas = parcelasPagas,
        ParcelasEmAtraso = parcelasEmAtraso,
        ParcelasAVencer = parcelasAVencer,
        PercentualPagasEmDia = percentualPagasEmDia
    };

    return Results.Ok(resumo);
});

app.Run();
