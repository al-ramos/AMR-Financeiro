feat(amr-financeiro): Multi-banco + Parcelamento + Aging Report — Sprint 23 Card 23.6

### Backend (.NET 8 + EF Core + MediatR)

**Domain**
- `ContaBancaria` — entidade com TipoContaBancaria enum (ContaCorrente/Poupanca/Investimento); saldo nunca persistido
- `Parcelamento` + `Parcela` — geração automática de N parcelas mensais; última parcela absorve centavos
- Enums: `TipoContaBancaria`, `StatusParcela`, `TipoVinculoParcelamento`
- `LancamentoFinanceiro.ContaBancariaId` — campo para vínculo multi-banco

**Interfaces**
- `IContaBancariaRepository` — CRUD + ObterSaldosAsync (calculado via query) + ObterExtratoAsync
- `IParcelamentoRepository` — CRUD + ObterParcelasEmAbertoAsync
- `ILancamentoFinanceiroRepository.ObterFuturosAsync` — para projeção de fluxo de caixa

**Infrastructure**
- `ContaBancariaRepository` — saldo calculado em SQL (SaldoInicial + créditos − débitos desde DataSaldoInicial)
- `ParcelamentoRepository` — CRUD + parcelas em aberto para aging/fluxo
- `FinanceiroDbContext` — DbSets + OnModelCreating para as 3 novas tabelas
- Migration `AddMultiBancoParcelamentoAging` — cria `contas_bancarias`, `parcelamentos`, `parcelas`

**Application (CQRS)**
- `ContasBancarias`: GetContasBancariasQuery / GetContaBancariaByIdQuery / GetExtratoQuery + handlers
- `ContasBancarias`: CriarContaBancariaCommand / AtualizarContaBancariaCommand / DesativarContaBancariaCommand + handlers
- `Parcelamentos`: GetParcelamentosQuery / GetParcelamentoByIdQuery + handlers
- `Parcelamentos`: CriarParcelamentoCommand / PagarParcelaCommand + handlers
- `Financeiro`: GetAgingQuery (aging por 5 faixas) / GetFluxoCaixaQuery (projeção 30/60/90d) + handlers

**API Controllers**
- `ContasBancariasController` — GET/POST /api/contas-bancarias, PUT/DELETE /{id}, GET /{id}/extrato
- `ParcelamentosController` — GET/POST /api/parcelamentos, GET /{id}, PATCH /{id}/parcelas/{id}/pagar
- `FinanceiroController` — GET /api/financeiro/aging, GET /api/financeiro/fluxo-caixa

### Frontend (React 18 + TypeScript)

**API Clients** (`frontend/src/api/`)
- `contasBancariasApi.ts` — CRUD + extrato + tipos TS
- `parcelamentosApi.ts` — CRUD + pagar parcela
- `financeiroApi.ts` — aging + fluxo de caixa

**4 novas páginas**
- `/financeiro/contas-bancarias` — cards com saldo calculado em tempo real, badge TipoConta colorido, modal criar/editar
- `/financeiro/aging` — barra de progresso proporcional + cards por faixa (verde→roxo), tabela com %, total em aberto
- `/financeiro/parcelamentos` — accordion collapsible, barra de progresso X/N pagas, modal pagar parcela
- `/financeiro/fluxo-caixa` — 3 KPI cards, gráfico SVG de saldo acumulado, toggle 30/60/90d, tabela por dia

**App.tsx** — rotas + seção "Financeiro" na sidebar com 3 novos itens

**index.css** — classes `amr-card`, `amr-btn`, `amr-modal`, `amr-table`, `amr-input`, `amr-form-group`, `amr-alert`, `amr-spin`

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
