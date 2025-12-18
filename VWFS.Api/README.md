# VWFS – API de Financiamento de Veículos (Desafio Técnico)

API básica em **C# / ASP.NET Core (Minimal API)** com **Swagger** e persistência **em memória**, feita para discussão de abordagem.

## Como rodar

```bash
dotnet restore
dotnet run --project ./VWFS.Api
```

Swagger:
- http://localhost:5000/swagger

## Endpoints

### Contratos
- GET `/api/contratos`
- GET `/api/contratos/{id}`
- POST `/api/contratos`
- PUT `/api/contratos/{id}`
- DELETE `/api/contratos/{id}`

### Pagamentos
- POST `/api/contratos/{id}/pagamentos`
- GET `/api/contratos/{id}/pagamentos`

### Resumo do cliente
- GET `/api/clientes/{cpfCnpj}/resumo`
