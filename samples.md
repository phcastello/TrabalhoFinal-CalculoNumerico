# Catálogo de Casos de Entrada – Métodos de Raízes

Guia em formato de catálogo: primeiro uma visão geral resumida, depois cartões detalhados (objetivo, parâmetros, métodos e bloco pronto para request). 

## Visão Geral Rápida
| Caso | Função principal | Raiz esperada | Métodos | Observações |
| --- | --- | --- | --- | --- |
| 1 | `x^3 - x - 2` | ≈ 1.5213797068 | Bisection, Regula Falsi, Newton, Secant, Fixed Point | Todos os métodos habilitados.
| 2 | `sin(x) - 0.25` | ≈ 0.2526802551 | Bisection, Regula Falsi, Secant | Tolerância relativa ativa.
| 3 | `ln(x) - 1` | ≈ 2.7182818285 | Newton, Secant | Foco em erro relativo baixo.
| 4 | `cos(x) - x` | ≈ 0.7390851332 | Fixed Point, Newton | Função de iteração explícita.
| 5 | `e^(-x) - x` | ≈ 0.5671432904 | Newton, Fixed Point, Secant | Limite de iterações reduzido.
| 6 | `x^2 + 1` | — | Bisection, Regula Falsi | Deve falhar (sem troca de sinal).
| 7 | `sqrt(x) - 1` | 1.0 | Bisection, Secant | Intervalo curto.
| 8 | `x^5 - x` | ±1.0 | Newton, Secant, Bisection | Tolerância relativa dominante.
| 9 | `tan(x) - 1` | ≈ 0.7853981634 | Bisection, Regula Falsi, Newton, Secant | Inclinação alta (π/4).
| 10 | `log(x) + sin(x) - 0.5` | ≈ 0.8029781357 | Newton, Secant, Bisection | Tolerâncias apertadas.
| 11 | `sqrt(x) - e^(-x)` | ≈ 0.4263027510 | Bisection, Newton, Secant | Combina radiciação e exponencial.
| 12 | `abs(x - 1) - 0.2` | 0.8 / 1.2 | Bisection, Regula Falsi, Secant | Duas raízes reais distintas.

---

### Caso 1 · Polinômio cúbico completo
- **Objetivo:** testar todos os métodos convergindo para ~1.5213797068.
- **Condições:** `tol_abs = 1e-6`, `tol_res = 1e-8`, `max_iter = 100` (erro relativo desligado).
- **Definições:** `f = x^3 - x - 2`, `df = 3*x^2 - 1`, `phi = x - (f/df)`.
- **Métodos:**
  - Bisection `a=1, b=2`
  - Regula Falsi `a=1, b=2`
  - Newton `x0=1.5`
  - Secant `x0=1, x1=2`
  - Fixed Point `x0=1.2`
- **Payload pronto:**
```ini
f: x^3 - x - 2
df: 3*x^2 - 1
phi: x - (x^3 - x - 2)/(3*x^2 - 1)
tol_abs: 1e-6
tol_rel: 0
tol_res: 1e-8
max_iter: 100

[methods]
bisection: a=1, b=2
regula_falsi: a=1, b=2
newton: x0=1.5
secant: x0=1, x1=2
fixed_point: x0=1.2
```

### Caso 2 · Função trigonométrica com erro relativo
- **Objetivo:** medir convergência com `tol_rel = 1e-6` para raiz ~0.25268.
- **Condições:** `tol_abs = 1e-8`, `tol_res = 1e-10`, `max_iter = 80`.
- **Definições:** `f = sin(x) - 0.25` (derivada analítica não necessária).
- **Métodos:**
  - Bisection `a=0, b=2`
  - Regula Falsi `a=0, b=2`
  - Secant `x0=0.2, x1=1.0`
- **Payload pronto:**
```ini
f: sin(x) - 0.25
tol_abs: 1e-8
tol_rel: 1e-6
tol_res: 1e-10
max_iter: 80

[methods]
# Métodos baseados em intervalo
bisection: a=0, b=2
regula_falsi: a=0, b=2
# Método de duas aproximações iniciais
secant: x0=0.2, x1=1.0
```

### Caso 3 · Logaritmo natural (erro relativo dominante)
- **Objetivo:** validar `tol_rel = 1e-8` com `f = ln(x) - 1`.
- **Condições:** `tol_abs = 0`, `tol_res = 1e-10`, `max_iter = 60`.
- **Definições:** `df = 1/x`.
- **Métodos:** Newton `x0=2`; Secant `x0=1.5, x1=3.0`.
- **Payload pronto:**
```ini
f: ln(x) - 1
df: 1/x
tol_abs: 0
tol_rel: 1e-8
tol_res: 1e-10
max_iter: 60

[methods]
newton: x0=2
secant: x0=1.5, x1=3.0
```

### Caso 4 · Ponto fixo com cosseno
- **Objetivo:** comparar convergência de Fixed Point vs Newton (~0.7390851332).
- **Condições:** `tol_abs = 1e-7`, `tol_res = 1e-8`, `max_iter = 200`.
- **Definições:** `f = cos(x) - x`, `df = -sin(x) - 1`, `phi = cos(x)`.
- **Métodos:** Fixed Point `x0=0.5`; Newton `x0=0.8`.
- **Payload pronto:**
```ini
f: cos(x) - x
df: -sin(x) - 1
phi: cos(x)
tol_abs: 1e-7
tol_rel: 0
tol_res: 1e-8
max_iter: 200

[methods]
fixed_point: x0=0.5
newton: x0=0.8
```

### Caso 5 · Função exponencial com max_iter baixo
- **Objetivo:** raiz ~0.56714 com `max_iter = 20` (pode estourar limite se muito rígido).
- **Condições:** `tol_abs = 1e-12`, `tol_res = 1e-12`.
- **Definições:** `f = e^(-x) - x`, `df = -e^(-x) - 1`, `phi = e^(-x)`.
- **Métodos:** Newton `x0=0.0`; Fixed Point `x0=0.5`; Secant `x0=-1, x1=1`.
- **Payload pronto:**
```ini
f: e^(-x) - x
df: -e^(-x) - 1
phi: e^(-x)
tol_abs: 1e-12
tol_rel: 0
tol_res: 1e-12
max_iter: 20

[methods]
newton: x0=0.0
fixed_point: x0=0.5
secant: x0=-1, x1=1
```

### Caso 6 · Falha sem troca de sinal
- **Objetivo:** garantir validação `error: no sign change`.
- **Condições:** `tol_abs = 1e-6`, `tol_res = 1e-6`, `max_iter = 50`.
- **Definições:** `f = x^2 + 1` (sempre positiva).
- **Métodos:** Bisection `a=-2, b=2`; Regula Falsi `a=-2, b=2`.
- **Payload pronto:**
```ini
f: x^2 + 1
tol_abs: 1e-6
tol_rel: 0
tol_res: 1e-6
max_iter: 50

[methods]
bisection: a=-2, b=2
regula_falsi: a=-2, b=2
```

### Caso 7 · Raiz quadrada simples
- **Objetivo:** raiz exata 1.0 com intervalos estreitos.
- **Condições:** `tol_abs = 1e-6`, `tol_rel = 1e-8`, `tol_res = 1e-9`, `max_iter = 40`.
- **Definições:** `f = sqrt(x) - 1`.
- **Métodos:** Bisection `a=0, b=2`; Secant `x0=0.25, x1=2.0`.
- **Payload pronto:**
```ini
f: sqrt(x) - 1
tol_abs: 1e-6
tol_rel: 1e-8
tol_res: 1e-9
max_iter: 40

[methods]
bisection: a=0, b=2
secant: x0=0.25, x1=2.0
```

### Caso 8 · Polinômio ímpar (raiz positiva vs negativa)
- **Objetivo:** mostrar convergência para ±1 com `tol_rel = 1e-9`.
- **Condições:** `tol_abs = 0`, `tol_res = 1e-8`, `max_iter = 120`.
- **Definições:** `f = x^5 - x`, `df = 5*x^4 - 1`.
- **Métodos:**
  - Newton `x0=0.8` → raiz positiva
  - Secant `x0=-1.5, x1=-0.5` → raiz negativa
  - Bisection `a=-2, b=-0.5`
- **Payload pronto:**
```ini
f: x^5 - x
df: 5*x^4 - 1
tol_abs: 0
tol_rel: 1e-9
tol_res: 1e-8
max_iter: 120

[methods]
newton: x0=0.8
secant: x0=-1.5, x1=-0.5
bisection: a=-2, b=-0.5
```

### Caso 9 · Tangente inclinada
- **Objetivo:** raiz π/4 com tolerância residual 1e-12.
- **Condições:** `tol_abs = 1e-10`, `tol_rel = 0`, `max_iter = 80`.
- **Definições:** `f = tan(x) - 1`, `df = 1 + tan(x)^2`.
- **Métodos:** Bisection `a=0.4, b=1.2`; Regula Falsi `a=0.4, b=1.2`; Newton `x0=0.9`; Secant `x0=0.6, x1=1.1`.
- **Payload pronto:**
```ini
f: tan(x) - 1
df: 1 + tan(x)^2
tol_abs: 1e-10
tol_rel: 0
tol_res: 1e-12
max_iter: 80

[methods]
bisection: a=0.4, b=1.2
regula_falsi: a=0.4, b=1.2
newton: x0=0.9
secant: x0=0.6, x1=1.1
```

### Caso 10 · Log + sen com tolerâncias apertadas
- **Objetivo:** raiz ~0.803 com tolerâncias 1e-12/1e-11.
- **Condições:** `tol_abs = 1e-12`, `tol_rel = 1e-9`, `tol_res = 1e-11`, `max_iter = 120`.
- **Definições:** `f = log(x) + sin(x) - 0.5`, `df = 1/x + cos(x)`.
- **Métodos:** Newton `x0=0.6`; Secant `x0=0.5, x1=1.1`; Bisection `a=0.4, b=1.2`.
- **Payload pronto:**
```ini
f: log(x) + sin(x) - 0.5
df: 1/x + cos(x)
tol_abs: 1e-12
tol_rel: 1e-9
tol_res: 1e-11
max_iter: 120

[methods]
newton: x0=0.6
secant: x0=0.5, x1=1.1
bisection: a=0.4, b=1.2
```

### Caso 11 · Raiz quadrada vs exponencial
- **Objetivo:** raiz ~0.4263 com residual 1e-12.
- **Condições:** `tol_abs = 1e-11`, `tol_rel = 0`, `tol_res = 1e-12`, `max_iter = 90`.
- **Definições:** `f = sqrt(x) - e^(-x)`, `df = 1/(2*sqrt(x)) + e^(-x)`.
- **Métodos:** Bisection `a=0, b=1`; Newton `x0=0.3`; Secant `x0=0.1, x1=0.8`.
- **Payload pronto:**
```ini
f: sqrt(x) - e^(-x)
df: 1/(2*sqrt(x)) + e^(-x)
tol_abs: 1e-11
tol_rel: 0
tol_res: 1e-12
max_iter: 90

[methods]
bisection: a=0, b=1
newton: x0=0.3
secant: x0=0.1, x1=0.8
```

### Caso 12 · Módulo com duas raízes
- **Objetivo:** capturar raízes 0.8 (lado esquerdo) e 1.2 (lado direito).
- **Condições:** `tol_abs = 1e-8`, `tol_rel = 0`, `tol_res = 1e-8`, `max_iter = 100`.
- **Definições:** `f = abs(x - 1) - 0.2`.
- **Métodos:**
  - Bisection `a=0.4, b=0.95`
  - Regula Falsi `a=1.05, b=1.5`
  - Secant `x0=0.6, x1=0.95`
- **Payload pronto:**
```ini
f: abs(x - 1) - 0.2
tol_abs: 1e-8
tol_rel: 0
tol_res: 1e-8
max_iter: 100

[methods]
bisection: a=0.4, b=0.95
regula_falsi: a=1.05, b=1.5
secant: x0=0.6, x1=0.95
```
