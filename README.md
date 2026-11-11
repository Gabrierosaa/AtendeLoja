# AtendeLoja

[![CI](https://github.com/Gabrierosaa/AtendeLoja/actions/workflows/ci.yml/badge.svg)](https://github.com/Gabrierosaa/AtendeLoja/actions/workflows/ci.yml)

Assistente de atendimento para comércios locais. Responde clientes sobre horários, preços e políticas da loja usando **somente os dados que o dono cadastrou** e **recusa** quando não tem base.

Projeto de aprendizado e portfólio, com foco em um RAG que se avalia e se observa.

> **Status:** 🚧 em construção (Fase 0). A solução .NET, a estrutura de pastas e o ambiente Docker estão prontos; a API, o worker e o Angular ainda vão ser implementados fase a fase (ver [Roadmap](#roadmap)).
> Detalhes completos em [`docs/AtendeLoja_Documento_Tecnico.pdf`](docs/AtendeLoja_Documento_Tecnico.pdf).

## O que ele faz (exemplo)

| Cliente pergunta | Como é respondido |
|------------------|-------------------|
| "Quanto sai a coxinha?" | Preço lido do banco, em frase fixa. Sem IA nos valores. |
| "Vocês trocam produto?" | Busca nos documentos da loja (RAG), com citação do documento. |
| "Tem o produto X?" (não cadastrado) | "Não encontrei" e encaminhamento para uma pessoa. |

## Por que o projeto existe

É um projeto de aprendizado. Quero praticar, num sistema pequeno mas completo: arquitetura em camadas, RAG com avaliação mensurável, isolamento entre lojas, observabilidade e testes de integração.

## Problema

Comércios pequenos recebem as mesmas perguntas várias vezes ao dia (horário, preço, prazo de encomenda, política de troca). Um assistente baseado em LLM resolve a escala, mas cria um risco novo: inventar preço, promoção ou regra que a loja não tem. Numa loja, isso é prejuízo.

## Princípio central

> O modelo escreve texto, mas **não é fonte de fatos**.

- Preço, disponibilidade e horário vêm do **banco** (resposta por template).
- Regras da loja vêm de **trechos recuperados** dos documentos do dono.
- O que não tem base é **recusado** e registrado.
- Nenhum valor monetário ou horário sai da API sem o validador rastrear sua origem.

## Escopo da v1

- API REST e painel Angular para o dono cadastrar produtos, preços, horários e documentos.
- Chat público por loja (widget), sem login do cliente e sem histórico.
- Roteador: dado estruturado / política / fora de escopo.
- Ingestão de documentos (PDF com texto, TXT, MD) em segundo plano.
- Busca por similaridade, geração com citação, validação, recusa com encaminhamento.
- Conjunto de avaliação versionado, traces OpenTelemetry e métricas de custo estimado.
- Dois ambientes: dev (local, modelo local opcional) e prod (provedor real).

**Fora de escopo:** WhatsApp, histórico de conversa, pedidos/pagamento, OCR, voz, multi-idioma, reranking, banco vetorial dedicado, agentes com ferramentas, planos e cobrança.

## Arquitetura

Estilo **API + roteador + RAG**, em Clean Architecture enxuta com DDD tático (`Horario`, `Documento`, `Dinheiro`).

```
Pergunta ─▶ Roteador ─┬─▶ Dado estruturado ─▶ banco + template
                      ├─▶ Política ─▶ RAG (busca + geração) ─▶ validador
                      └─▶ Fora de escopo / sem base ─▶ recusa + encaminha + registra
```

### Decisões principais

| ID | Decisão |
|----|---------|
| D01 | Roteador antes do RAG: fatos estruturados e texto livre seguem caminhos diferentes |
| D02 | Dado estruturado responde por template, sem texto gerado para valores |
| D03 | Classificação por modelo (JSON) com fallback por palavras-chave |
| D04 | Multi-loja com coluna `LojaId` e filtros globais do EF Core |
| D05 | Embeddings no SQL Server, similaridade em memória (atrás de `IBuscadorDeTrechos`) |
| D06 | Provedor de IA via `Microsoft.Extensions.AI` (`IChatClient`, `IEmbeddingGenerator`) |
| D07 | Chat sem histórico na v1 |
| D08 | Clean Architecture enxuta + DDD tático |
| D10 | Validador determinístico (regex) após a geração |
| D11 | Dinheiro como `decimal(18,2)`, horário como `time` no fuso da loja |
| D12 | Casos de uso como classes simples, sem MediatR |
| D13 | JWT no painel; chat público sem login, protegido por rate limit |

### Guardas do RAG

Limiar de similaridade · instrução de recusa · validador de valores · citação obrigatória · limite de tamanho · temperatura baixa.

## Estrutura do repositório

```
AtendeLoja/
├── AtendeLoja.sln
├── src/
│   ├── Atende.Domain/           entidades, value objects, regras (horário, dinheiro)
│   ├── Atende.Application/      casos de uso, roteador, validador, divisor de texto, portas
│   ├── Atende.Infrastructure/   EF Core, IA (Extensions.AI), PdfPig, busca, telemetria
│   ├── Atende.Api/              endpoints, JWT, rate limit, ProblemDetails, health
│   └── Atende.Worker/           ingestão de documentos em segundo plano
├── web/                         Angular: painel do dono e widget de chat
├── tests/
│   ├── Atende.Domain.Tests/
│   ├── Atende.Application.Tests/
│   ├── Atende.IntegrationTests/ (Testcontainers + WebApplicationFactory)
│   └── Atende.Evaluation/       runner da avaliação
├── eval/                        casos de avaliação (JSON) e loja fictícia
├── docker/                      Dockerfiles e docker-compose
└── docs/                        documento técnico e diagramas
```

### Dependências entre projetos

As dependências apontam para dentro; o `Domain` não depende de nada.

| Projeto | Depende de |
|---------|-----------|
| Domain | — |
| Application | Domain |
| Infrastructure | Application, Domain |
| Api | Application, Infrastructure |
| Worker | Application, Infrastructure |
| web (Angular) | Api (contrato HTTP) |

## Modelo de dados

Oito tabelas: `Loja`, `Usuario`, `Produto`, `Horario`, `ExcecaoHorario`, `Documento`, `Trecho`, `PerguntaSemResposta`. Todas, exceto `Loja`, carregam `LojaId`.

Estados do `Documento`: `Enviado → Processando → Pronto | Falhou` (falha temporária volta a `Enviado`; substituição leva `Pronto → Processando`).

## Endpoints (`/api/v1`)

| Método | Rota | Acesso |
|--------|------|--------|
| POST | `/auth/registro` | Público (rate limit: 5/min por IP) |
| POST | `/auth/login` | Público |
| GET/POST | `/produtos` | JWT |
| PUT/DELETE | `/produtos/{id}` | JWT |
| GET/PUT | `/horarios` | JWT |
| POST/DELETE | `/horarios/excecoes` | JWT |
| POST/GET | `/documentos` | JWT |
| GET/DELETE | `/documentos/{id}` | JWT |
| GET | `/perguntas-sem-resposta` | JWT |
| PATCH | `/perguntas-sem-resposta/{id}` | JWT |
| GET | `/metricas/resumo` | JWT |
| POST | `/public/lojas/{slug}/chat` | Público |
| GET | `/health/live`, `/health/ready` | Público (raiz) |

Erros seguem `ProblemDetails` (RFC 9457). Recusa do assistente **não é erro**: volta `200` com `tipo: "Recusa"`.

## Segurança e privacidade

- JWT de vida curta; `LojaId` vem **só** do claim.
- Isolamento por loja em 3 camadas: filtro global, `LojaId` obrigatório na busca, teste de integração com duas lojas.
- Rate limit por IP e por loja, pergunta ≤ 500 caracteres, CORS restrito.
- Prompt injection: dados delimitados, modelo sem ferramentas de escrita, validador na saída.
- Segredos fora do Git. Perguntas mascaradas (e-mail, telefone, CPF), retenção sugerida de 30 dias, sem texto de pergunta/resposta em logs.

## Observabilidade

Serilog (JSON), traces OpenTelemetry por etapa (`chat.request`, `chat.route`, `rag.search`, `rag.generate`, `chat.validate`, `ingest.document`), métricas (`perguntas_total`, `recusas_total`, `tokens_total`, `custo_estimado`, ...) e health checks.

## Testes e avaliação

- Unitários (xUnit, NSubstitute): horário, divisor, validador, mascaramento, estados.
- Integração (Testcontainers): SQL Server real, isolamento entre lojas, upload → ingestão.
- Avaliação (`eval/`): ~40 perguntas (10 dado estruturado, 15 política, 15 armadilhas).
- CI usa provedor falso e nunca chama a API paga; avaliação completa roda sob demanda.

| Métrica | Meta inicial |
|---------|--------------|
| Hit rate@k | a calibrar |
| Recusa correta nas armadilhas | 100% |
| Valores sem origem | 0 |
| Resposta de política correta | a calibrar (~80%) |

## Como rodar

Pré-requisitos: [Docker Desktop](https://www.docker.com/products/docker-desktop/) e, para desenvolver, o SDK do .NET 10.

1. Crie o `.env` e preencha `SQLSERVER_SA_PASSWORD` e `JWT_SECRET` (o arquivo não vai para o Git):

```bash
cp .env.example .env
```

2. Suba o ambiente. Por enquanto só o SQL Server está ativo:

```bash
docker compose -f docker/docker-compose.yml --env-file .env up -d
```

3. Quando a API, o worker e o Angular existirem, suba tudo com o perfil `app`:

```bash
docker compose -f docker/docker-compose.yml --env-file .env --profile app up --build
```

Serviços: `sqlserver` (1433), `api` (8080), `worker`, `web` (4200). Opcionais: `--profile ollama` (modelo local) e `--profile observabilidade` (Jaeger, interface em 16686).

### Rodando a API localmente e testando o cadastro

Com o SQL Server do passo 2 no ar, aplique a migração e suba a API (a partir da raiz, em Git Bash/Linux/macOS):

```bash
set -a && . ./.env && set +a
export ConnectionStrings__Padrao="Server=localhost,1433;Database=AtendeLoja;User Id=sa;Password=${SQLSERVER_SA_PASSWORD};TrustServerCertificate=True"
dotnet ef database update --project src/Atende.Infrastructure
ASPNETCORE_URLS=http://localhost:5080 dotnet run --project src/Atende.Api --no-launch-profile
```

> **Visual Studio / F5:** a IDE não carrega o `.env`. Guarde a connection string no user-secrets (fica fora do Git) e rode com o perfil padrão:
>
> ```bash
> dotnet user-secrets set "ConnectionStrings:Padrao" "Server=localhost,1433;Database=AtendeLoja;User Id=sa;Password=<SQLSERVER_SA_PASSWORD>;TrustServerCertificate=True" --project src/Atende.Api
> ```
>
> Alternativa sem user-secrets: crie `src/Atende.Api/appsettings.Local.json` (ignorado pelo Git) com `{ "ConnectionStrings": { "Padrao": "..." } }`.
>
> Nesse caso a API sobe em `http://localhost:47123` (veja `launchSettings.json`); troque `5080` por `47123` nos `curl`.

Em outro terminal, teste `POST /api/v1/auth/registro`. A resposta nunca traz a senha nem o hash.

**201 – cadastro válido** (slug gerado: `padaria-do-ze`):

```bash
curl -i -X POST http://localhost:5080/api/v1/auth/registro \
  -H "Content-Type: application/json" \
  --data-binary '{"nome":"Padaria do Ze","email":"ze@exemplo.com","senha":"senha-segura-123"}'
```

**409 – slug repetido** (rode o comando acima de novo com outro e-mail):

```bash
curl -i -X POST http://localhost:5080/api/v1/auth/registro \
  -H "Content-Type: application/json" \
  --data-binary '{"nome":"Padaria do Ze","email":"outro@exemplo.com","senha":"senha-segura-123"}'
```

**409 – e-mail repetido** (mesmo e-mail do primeiro, nome diferente; maiúsculas não importam):

```bash
curl -i -X POST http://localhost:5080/api/v1/auth/registro \
  -H "Content-Type: application/json" \
  --data-binary '{"nome":"Confeitaria da Ana","email":"ZE@exemplo.com","senha":"senha-segura-123"}'
```

**400 – senha fraca** (menos de 8 caracteres; a resposta lista os erros por campo):

```bash
curl -i -X POST http://localhost:5080/api/v1/auth/registro \
  -H "Content-Type: application/json" \
  --data-binary '{"nome":"Loja Nova","email":"nova@exemplo.com","senha":"123"}'
```

**400 – fuso inexistente:**

```bash
curl -i -X POST http://localhost:5080/api/v1/auth/registro \
  -H "Content-Type: application/json" \
  --data-binary '{"nome":"Loja Fuso","email":"fuso@exemplo.com","senha":"senha-segura-123","fusoHorario":"Marte/Olympus"}'
```

**429 – rate limit** (a 6ª chamada no mesmo minuto é recusada):

```bash
for i in 1 2 3 4 5 6 7; do curl -s -o /dev/null -w "%{http_code} " -X POST http://localhost:5080/api/v1/auth/registro -H "Content-Type: application/json" --data-binary '{}'; done
```

> **Windows/Git Bash e acentos:** o `curl` do Git Bash envia mal caracteres não ASCII passados direto na linha de comando (o JSON chega inválido e a API responde 400). Use nomes sem acento, como acima, ou mande o corpo pela entrada padrão:
>
> ```bash
> printf '%s' '{"nome":"Padaria do Zé","email":"ze@exemplo.com","senha":"senha-segura-123"}' \
>   | curl -i -X POST http://localhost:5080/api/v1/auth/registro -H "Content-Type: application/json" --data-binary @-
> ```
>
> No PowerShell, use `curl.exe` (não o alias `curl`, que é o `Invoke-WebRequest`).

Para refazer os testes do zero, apague as linhas de teste (ou recrie o banco com `docker compose ... down -v`).

## Roadmap

| Fase | Semana | Objetivo | Pronto quando | Status |
|------|--------|----------|---------------|--------|
| 0 | 1 | Fundação (solução, compose, CI, login JWT) | Dono faz login e pipeline fica verde | 🚧 solução e Docker prontos; faltam CI e login |
| 1 | 2 | Dados estruturados (produtos, horários, exceções) | Responde preço e horário sem IA | ⏳ |
| 2 | 3 | Ingestão (upload, extração, trechos, embeddings) | Um comando popula a loja fictícia | ⏳ |
| 3 | 4 | Busca e resposta (roteador, RAG, validador, recusa) | Produto inexistente → "não encontrei" | ⏳ |
| 4 | 5 | Avaliação e observabilidade | Nota antes/depois e armadilhas passam | ⏳ |
| 5 | 6 | Telas em Angular | Fluxo completo sem Swagger | ⏳ |
| 6 | 7 | Acabamento e demo | Link da demo funcional | ⏳ |

Se o tempo apertar, pare na fase 4.

## Decisões em aberto

A02 modelo de embeddings · A03 tamanho de trecho/k/limiar · A04 Controllers vs Minimal APIs · A05 hospedagem da demo · A06 retenção das perguntas.

Resolvidas: A01 (Ollama em dev), registrada em [`docs/decisoes.md`](docs/decisoes.md).

## Stack

.NET 10 (C#, ASP.NET Core) · EF Core · SQL Server · Angular + TypeScript · Microsoft.Extensions.AI · PdfPig · FluentValidation · Serilog · OpenTelemetry · xUnit/NSubstitute/Testcontainers · k6 · Docker Compose · GitHub Actions.

## Autor

Gabriel Rosa
