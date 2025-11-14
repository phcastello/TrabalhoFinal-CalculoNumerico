# Numerical Methods – Etapa 3

Plataforma de Métodos Numéricos composta por backend ASP.NET Core 8, domínio compartilhado e frontend Vue 3 + Vite + TypeScript. A etapa atual entrega implementações reais para todos os métodos de sistemas lineares **e** de raízes de funções, mantendo o mesmo contrato de API usado desde a primeira fase.

## Estrutura

```
.
├─ backend/
│  ├─ NumericalMethods.sln
│  └─ src/
│     ├─ NumericalMethods.Core/
│     └─ NumericalMethods.Api/
├─ frontend/
│  └─ numerical-methods-frontend/
└─ docker-compose.yml
```

## Como executar com Docker

```bash
docker compose up --build
```

- Backend API: http://localhost:5000
- Frontend: http://localhost:4173

O frontend já envia requests para `http://backend:5000` dentro da rede do compose. Em desenvolvimento local (sem Docker) o axios usa `http://localhost:5000` por padrão (`frontend/numerical-methods-frontend/.env`).

## Recursos entregues
    
### Backend

- `LinearSystemSolverService` implementa Gauss (sem, com pivoteamento parcial e completo), fatorações LU e Cholesky, além dos métodos iterativos Jacobi e Gauss–Seidel. Todos respeitam `IterativeParams`/`SolverStatus`, calculam tempo com `Stopwatch` e retornam a solução pronta para o frontend.
- `RootFindingService` interpreta a expressão `functionExpression` via `ExpressionEvaluator` (suporta números, variável `x`, operadores `+ - * / ^`, parênteses e funções como `sin`, `cos`, `exp`, `ln`, `log10`, `sqrt`) e executa Bisection, FixedPoint, Newton (com derivada numérica), Regula Falsi e Secant.
- Todos os métodos tratam validações de entrada, divergência numérica e limite de iterações, retornando `SolverStatus` adequado (`Success`, `InvalidInput`, `Divergence`, `MaxIterationsReached`, etc.).

### Frontend

- `LinearSystemsView` e `RootFindingView` continuam enviando os mesmos DTOs; agora recebem resultados reais e exibem o JSON retornado pela API.
- Configuração `.env` aponta para `http://localhost:5000` e pode ser sobrescrita por variáveis em tempo de build Docker (`VITE_API_BASE_URL`).

## Desenvolvimento local

```bash
# Backend
cd backend
dotnet build NumericalMethods.sln

# Frontend
cd ../frontend/numerical-methods-frontend
npm install
npm run dev
```

## Limitações e próximos passos

- O parâmetro `returnSteps` ainda é ignorado (sem registro da sequência completa de iterações).
- Não há parsing de arquivos ou validações avançadas; os dados devem ser enviados em JSON direto.
- Testes automatizados básicos são recomendados como evolução (atualmente apenas builds/execuções manuais foram verificados).
