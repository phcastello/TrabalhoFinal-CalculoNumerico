# Numerical Methods

Plataforma de Métodos Numéricos composta por backend ASP.NET Core 8, domínio compartilhado e frontend Vue 3 + Vite + TypeScript. Implementa métodos completos para sistemas lineares e raízes de funções, mantendo um contrato de API estável.

## Visão geral

- API .NET 8 em `backend/src/NumericalMethods.Api`, com Swagger em `http://localhost:5000/swagger`.
- Interpretador de expressões aceita constantes `pi`/`e`, operadores `+ - * / ^`, funções como `sin`, `cos`, `exp`, `ln`, `log10`, `sqrt` e multiplicação implícita (`2x`, `(x+1)(x-1)` etc.).
- Solução de raízes: Bisection, Fixed Point, Newton (derivada analítica ou numérica), Regula Falsi e Secant, com tratamento de validação, divergência e limite de iterações.
- Sistemas lineares: eliminação de Gauss (simples/parcial/completa), fatorações LU e Cholesky, Jacobi e Gauss–Seidel, com validação de simetria/positividade quando necessário.
- Frontend Vue 3 + Vite (`frontend/numerical-methods-frontend`) com telas para sistemas e raízes consumindo os mesmos DTOs da API.

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

- Backend: http://localhost:5000 (Swagger em `/swagger`).
- Frontend: http://localhost:4173 (SPA já construída; axios aponta para `http://backend:5000` dentro do compose).

## Como executar localmente (sem Docker)

### Backend (.NET 8)

```bash
cd backend
dotnet restore
dotnet run --project src/NumericalMethods.Api
```

API disponível em http://localhost:5000.

### Frontend (Vue 3 + Vite)

```bash
cd frontend/numerical-methods-frontend
npm install
npm run dev
```

O Vite expõe por padrão na porta 5173; use `--host --port 4173` se quiser refletir o ambiente do Docker. A URL da API pode ser sobrescrita com `VITE_API_BASE_URL`.

## Endpoints principais

- `POST /api/roots/solve` — resolve raízes a partir de `functionExpression`, intervalo ou chutes iniciais, tolerância e método informado.
- `POST /api/linear-systems/solve` — resolve sistemas Ax = b com o método informado; `iterativeParams` é usado em Jacobi/Gauss–Seidel (`tolerance`, `maxIterations`, `stopCondition`).

## Manual de expressões para funções

- Variável: apenas `x`.
- Números: aceitam ponto decimal e notação científica (`1.2`, `-0.5`, `3e-2`).
- Constantes: `pi`, `e`.
- Operadores: `+ - * / ^`, com precedência padrão; `-` e `+` também funcionam como unários.
- Parênteses: obrigatórios para agrupar (`(x+1)/(x-1)`).
- Multiplicação implícita: permitida entre número/variável/parênteses/função (ex.: `2x`, `(x+1)(x-1)`, `3sin(x)`).
- Funções (case-insensitive): `sin`, `cos`, `tan`, `exp`, `ln`, `log`, `log10`, `sqrt`, `abs`.
- Erros de domínio (ex.: `sqrt` de número negativo, divisão por zero) retornam erro de validação.
- Exemplos válidos: `exp(-x) - x`, `ln(x) + x^2/3`, `sin(pi/4) - cos(x)`, `(x+1)^(1/3) - x`.

## Amostras de entrada para a API (versão legível)

## Raízes de funções (`/api/roots/solve`)

Métodos: `0=Bisection`, `1=FixedPoint`, `2=Newton`, `3=RegulaFalsi`, `4=Secant`. Campos principais: `functionExpression`, `method`, `a`/`b` (quando há intervalo), `initialGuess`/`secondGuess` (quando precisa), `phiExpression` (ponto fixo), `derivativeExpression` (Newton opcional), `tolerance`, `maxIterations`, `returnSteps`.

1) **Bisseção** — f(x) = x^3 - 2*x - 5  
   Intervalo [2, 3]; tol 1e-6; máx 100; raiz ≈ 2.09455148.  

2) **Bisseção** — f(x) = x^2 - 5  
   Intervalo [2, 3]; tol 1e-6; máx 100; raiz ≈ 2.23606798.  

3) **Bisseção (usa sin e pi)** — f(x) = sin(x) - sin(pi/6)  
   Intervalo [0, 1]; tol 1e-6; máx 100; raiz ≈ 0.52359878 (= pi/6).  

4) **Regula Falsi (usa exp e constante e)** — f(x) = exp(x) - e  
   Intervalo [0, 2]; tol 1e-6; máx 100; raiz ≈ 1.00000000.  

5) **Regula Falsi** — f(x) = exp(-x) - x  
   Intervalo [0, 1]; tol 1e-6; máx 100; raiz ≈ 0.56714329.  

6) **Regula Falsi** — f(x) = x^3 - 7  
   Intervalo [1, 2]; tol 1e-6; máx 100; raiz ≈ 1.91293118.  

7) **Regula Falsi** — f(x) = x^3 + x - 1  
   Intervalo [0, 1]; tol 1e-6; máx 100; raiz ≈ 0.68232780.  

8) **Ponto Fixo (usa cos)** — f(x) = cos(x) - x; φ(x) = cos(x)  
   Chute 0.5; tol 1e-6; máx 100; raiz ≈ 0.73908513.  

9) **Ponto Fixo (usa exp)** — f(x) = x - exp(-x); φ(x) = exp(-x)  
   Chute 0.5; tol 1e-6; máx 100; raiz ≈ 0.56714329.  

10) **Ponto Fixo** — f(x) = x^3 - x - 1; φ(x) = (x + 1)^(1/3)  
    Chute 1.0; tol 1e-6; máx 100; raiz ≈ 1.32471796.  

11) **Newton (usa ln/log)** — f(x) = ln(x) + x - 3; f’(x) = 1/x + 1  
    Chute 2.0; tol 1e-8; máx 100; raiz ≈ 2.20794003.  

12) **Newton (usa log)** — f(x) = log(x) - 1; f’(x) = 1/x  
    Chute 2.0; tol 1e-8; máx 100; raiz ≈ 2.71828183 (= e).  

13) **Newton** — f(x) = x^2 - 2; f’(x) = 2*x  
    Chute 1.0; tol 1e-10; máx 50; raiz ≈ 1.41421356.  

14) **Newton** — f(x) = x^3 - 2*x - 5; f’(x) = 3*x^2 - 2  
    Chute 2.0; tol 1e-8; máx 100; raiz ≈ 2.09455148.  

15) **Newton (usa sqrt)** — f(x) = sqrt(x) - 3  
    Chute 4.0; tol 1e-8; máx 100; raiz ≈ 9.00000000.  

16) **Secante (usa log10)** — f(x) = log10(x) - 0.3  
    Chutes 1.0 e 3.0; tol 1e-6; máx 100; raiz ≈ 1.99526231.  

17) **Secante** — f(x) = x^3 - x - 1  
    Chutes 1.0 e 1.5; tol 1e-6; máx 100; raiz ≈ 1.32471796.  

18) **Secante** — f(x) = x^4 - 18  
    Chutes 1.5 e 2.5; tol 1e-6; máx 100; raiz ≈ 2.05976714.  

19) **Secante** — f(x) = sin(x) - x/2  
    Chutes 1.0 e 2.0; tol 1e-6; máx 100; raiz ≈ 1.89549427.  

20) **Bisseção (usa tan)** — f(x) = tan(x) - x  
    Intervalo [-1, 1]; tol 1e-6; máx 100; raiz ≈ 0.00000000.  

21) **Bisseção (usa abs)** — f(x) = abs(x - 2) - 0.5  
    Intervalo [1, 2]; tol 1e-6; máx 100; raiz ≈ 1.50000000.  

## Sistemas Lineares

**Endpoint:** `/api/linear-systems/solve`

### Métodos Disponíveis

|Código|Método|
|---|---|
|**0**|Gauss|
|**1**|Gauss (Pivoteamento Parcial)|
|**2**|Gauss (Pivoteamento Completo)|
|**3**|Decomposição LU|
|**4**|Cholesky|
|**5**|Jacobi|
|**6**|Gauss–Seidel|

### Campos do Payload

- **a** — matriz de coeficientes
    
- **b** — vetor de constantes
    
- **method** — código do método
    
- **iterativeParams** (para métodos iterativos):
    
    - `tolerance`
        
    - `maxIterations`
        
    - `stopCondition`

A resposta retorna somente solução, iterações, tempo e mensagens; não há registro de passos para sistemas lineares.

---

## Casos de Teste

### **Método 0 — Gauss**

1. **Gauss**  
    A:  
    $$  
    A=\begin{bmatrix}  
    4 & 2 \\
    1 & 3  
    \end{bmatrix}  
    $$  
    b:  
    $$  
    b=\begin{bmatrix}  
    20 \\ 11  
    \end{bmatrix}  
    $$  
    **Solução:** ≈ `[3.8, 2.4]`
    
2. **Gauss**  
    $$  
    A=\begin{bmatrix}  
    3 & 2 & -4\\  
    2 & 3 & 3\ \ 
    5 & -3 & 1  
    \end{bmatrix},  
    \quad  
    b=\begin{bmatrix}  
    -1 \\ 9 \\ 20  
    \end{bmatrix}  
    $$  
    **Solução:** ≈ `[3, -1, 2]`
    
3. **Gauss**  
    $$  
    A=\begin{bmatrix}  
    10 & 2 & 3 & 4\\  
    1 & 12 & -1 & 2\\
    3 & -2 & 15 & 1\\  
    2 & 3 & 4 & 13  
    \end{bmatrix},  
    \quad  
    b=\begin{bmatrix}  
    39\\30\\48\\72  
    \end{bmatrix}  
    $$  
    **Solução:** ≈ `[1, 2, 3, 4]`
    

---

### **Método 1 — Gauss com Pivoteamento Parcial**

3. $$  
    A=\begin{bmatrix}  
    0.0001 & 1 & 1\\  
    2 & -1 & 3\\  
    4 & 1 & 1  
    \end{bmatrix},  
    \quad  
    b=\begin{bmatrix}  
    5.0001 \\ 9 \\ 9  
    \end{bmatrix}  
    $$  
    **Solução:** ≈ `[1, 2, 3]`
    
4. $$  
    A=\begin{bmatrix}  
    0.1 & 1 & 1\\  
    1 & 0.1 & 2\\  
    1 & 2 & 0.1  
    \end{bmatrix},  
    \quad  
    b=\begin{bmatrix}  
    0.1 \\ 2.9 \\ -0.9  
    \end{bmatrix}  
    $$  
    **Solução:** ≈ `[1, -1, 1]`
    

---

### **Método 2 — Gauss com Pivoteamento Completo**

5. $$  
    A=\begin{bmatrix}  
    1 & 2 & 3\\  
    4 & 0 & 6\\  
    7 & 8 & 9  
    \end{bmatrix},  
    \quad  
    b=\begin{bmatrix}  
    5\\16\\17  
    \end{bmatrix}  
    $$  
    **Solução:** ≈ `[1, -1, 2]`
    
6. $$  
    A=\begin{bmatrix}  
    2 & 1 & 1\\  
    1 & 3 & 2\\  
    1 & 0 & 0  
    \end{bmatrix},  
    \quad  
    b=\begin{bmatrix}  
    19\\31\\4  
    \end{bmatrix}  
    $$  
    **Solução:** ≈ `[4, 5, 6]`
    

---

### **Método 3 — Decomposição LU**

7. $$  
    A=\begin{bmatrix}  
    2 & 3 & 1\\  
    4 & 7 & 7\\  
    6 & 18 & 22  
    \end{bmatrix},  
    \quad  
    b=\begin{bmatrix}  
    3\\11\\28  
    \end{bmatrix}  
    $$  
    **Solução:** ≈ `[1, 0, 1]`
    
8. $$  
    A=\begin{bmatrix}  
    4 & 1 & 0 & 0\\  
    1 & 4 & 1 & 0\\  
    0 & 1 & 4 & 1\\  
    0 & 0 & 1 & 3  
    \end{bmatrix},  
    \quad  
    b=\begin{bmatrix}  
    6\\12\\18\\15  
    \end{bmatrix}  
    $$  
    **Solução:** ≈ `[1, 2, 3, 4]`
    

---

### **Método 4 — Cholesky**

9. $$  
    A=\begin{bmatrix}  
    6 & 2 & 1\\  
    2 & 5 & 2\\  
    1 & 2 & 4  
    \end{bmatrix},  
    \quad  
    b=\begin{bmatrix}  
    8\\6\\9  
    \end{bmatrix}  
    $$  
    **Solução:** ≈ `[1, 0, 2]`
    

$$  
A=\begin{bmatrix}  
4 & 1 & 0 & 0\\  
1 & 4 & 1 & 0\\  
0 & 1 & 4 & 1\\  
0 & 0 & 1 & 3  
\end{bmatrix},  
\quad  
b=\begin{bmatrix}  
9\\6\\0\\-3  
\end{bmatrix}  
$$  
**Solução:** ≈ `[2, 1, 0, -1]`

---

### **Método 5 — Jacobi**

$$  
A=\begin{bmatrix}  
10 & -1 & 2 & 0\\  
-1 & 11 & -1 & 3\\  
2 & -1 & 10 & -1\\ 
0 & 3 & -1 & 8  
\end{bmatrix},  
\quad  
b=\begin{bmatrix}  
11\\12\\10\\10  
\end{bmatrix}  
$$  
Tolerância: `1e-8`, Máx: `100`, stopCondition: `0`  
**Solução:** ≈ `[1, 1, 1, 1]`

$$  
A=\begin{bmatrix}  
5 & 2 & 1\\
1 & 8 & 1\\  
2 & 1 & 6  
\end{bmatrix},  
\quad  
b=\begin{bmatrix}  
15\\13\\23  
\end{bmatrix}  
$$  
Tolerância: `1e-8`, Máx: `100`, stopCondition: `1`  
**Solução:** ≈ `[2, 1, 3]`

---

### **Método 6 — Gauss–Seidel**

$$  
A=\begin{bmatrix}  
4 & 1 & 1\\  
2 & 7 & 1\\  
1 & -3 & 12  
\end{bmatrix},  
\quad  
b=\begin{bmatrix}  
6\\10\\10  
\end{bmatrix}  
$$  
Tolerância: `1e-8`, Máx: `100`, stopCondition: `0`  
**Solução:** ≈ `[1, 1, 1]`

$$  
A=\begin{bmatrix}  
9 & 1 & 1 & 0\\  
2 & 10 & 1 & 2\\  
1 & 0 & 8 & 1\\  
0 & 2 & 1 & 7  
\end{bmatrix},  
\quad  
b=\begin{bmatrix}  
14 \\ 33 \\ 29 \\ 35  
\end{bmatrix}  
$$  
Tolerância: `1e-8`, Máx: `100`, stopCondition: `0`  
**Solução:** ≈ `[1, 2, 3, 4]`
