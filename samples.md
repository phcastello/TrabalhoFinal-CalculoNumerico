# Amostras de entrada para a API (versão legível)

## Raízes de funções (`/api/roots/solve`)

Métodos: `0=Bisection`, `1=FixedPoint`, `2=Newton`, `3=RegulaFalsi`, `4=Secant`. Campos principais: `functionExpression`, `method`, `a`/`b` (quando há intervalo), `initialGuess`/`secondGuess` (quando precisa), `phiExpression` (ponto fixo), `derivativeExpression` (Newton opcional), `tolerance`, `maxIterations`, `returnSteps`.

1) **Bisseção** — f(x) = x^3 - 2*x - 5  
   Intervalo [2, 3]; tol 1e-6; máx 100; raiz ≈ 2.09455148.  
2) **Bisseção** — f(x) = x^2 - 5  
   Intervalo [2, 3]; tol 1e-6; máx 100; raiz ≈ 2.23606798.  
3) **Bisseção** — f(x) = sin(x) - 0.5  
   Intervalo [0, 2]; tol 1e-6; máx 100; raiz ≈ 0.52359878.  
4) **Regula Falsi** — f(x) = exp(-x) - x  
   Intervalo [0, 1]; tol 1e-6; máx 100; raiz ≈ 0.56714329.  
5) **Regula Falsi** — f(x) = x^3 - 7  
   Intervalo [1, 2]; tol 1e-6; máx 100; raiz ≈ 1.91293118.  
6) **Regula Falsi** — f(x) = x^3 + x - 1  
   Intervalo [0, 1]; tol 1e-6; máx 100; raiz ≈ 0.68232780.  
7) **Ponto Fixo** — f(x) = cos(x) - x; φ(x) = cos(x)  
   Chute 0.5; tol 1e-6; máx 100; raiz ≈ 0.73908513.  
8) **Ponto Fixo** — f(x) = x - exp(-x); φ(x) = exp(-x)  
   Chute 0.5; tol 1e-6; máx 100; raiz ≈ 0.56714329.  
9) **Ponto Fixo** — f(x) = x^3 - x - 1; φ(x) = (x + 1)^(1/3)  
   Chute 1.0; tol 1e-6; máx 100; raiz ≈ 1.32471796.  
10) **Newton** — f(x) = x^2 - 2; f’(x) = 2*x  
    Chute 1.0; tol 1e-10; máx 50; raiz ≈ 1.41421356.  
11) **Newton** — f(x) = x^3 - 2*x - 5; f’(x) = 3*x^2 - 2  
    Chute 2.0; tol 1e-8; máx 100; raiz ≈ 2.09455148.  
12) **Newton** — f(x) = ln(x) + x - 3; f’(x) = 1/x + 1  
    Chute 2.0; tol 1e-8; máx 100; raiz ≈ 2.20794003.  
13) **Secante** — f(x) = x^3 - x - 1  
    Chutes 1.0 e 1.5; tol 1e-6; máx 100; raiz ≈ 1.32471796.  
14) **Secante** — f(x) = x^4 - 18  
    Chutes 1.5 e 2.5; tol 1e-6; máx 100; raiz ≈ 2.05976714.  
15) **Secante** — f(x) = sin(x) - x/2  
    Chutes 1.0 e 2.0; tol 1e-6; máx 100; raiz ≈ 1.89549427.  

## Sistemas lineares (`/api/linear-systems/solve`)

Métodos: `0=Gauss`, `1=GaussPartialPivoting`, `2=GaussFullPivoting`, `3=LU`, `4=Cholesky`, `5=Jacobi`, `6=GaussSeidel`. Campos: `a` (matriz), `b` (vetor), `method`, `iterativeParams` para métodos iterativos (`tolerance`, `maxIterations`, `stopCondition`), `returnSteps`.

1) **Gauss (0)** — A = [[4, 2], [1, 3]]; b = [20, 11]; solução ≈ [3.8, 2.4].  
2) **Gauss (0)** — A = [[3, 2, -4], [2, 3, 3], [5, -3, 1]]; b = [-1, 9, 20]; solução ≈ [3, -1, 2].  
3) **Gauss c/ pivoteamento parcial (1)** — A = [[0.0001, 1, 1], [2, -1, 3], [4, 1, 1]]; b = [5.0001, 9, 9]; solução ≈ [1, 2, 3].  
4) **Gauss c/ pivoteamento parcial (1)** — A = [[0.1, 1, 1], [1, 0.1, 2], [1, 2, 0.1]]; b = [0.1, 2.9, -0.9]; solução ≈ [1, -1, 1].  
5) **Gauss c/ pivoteamento completo (2)** — A = [[1, 2, 3], [4, 0, 6], [7, 8, 9]]; b = [5, 16, 17]; solução ≈ [1, -1, 2].  
6) **Gauss c/ pivoteamento completo (2)** — A = [[2, 1, 1], [1, 3, 2], [1, 0, 0]]; b = [19, 31, 4]; solução ≈ [4, 5, 6].  
7) **LU (3)** — A = [[2, 3, 1], [4, 7, 7], [6, 18, 22]]; b = [3, 11, 28]; solução ≈ [1, 0, 1].  
8) **LU (3)** — A = [[4, 1, 0, 0], [1, 4, 1, 0], [0, 1, 4, 1], [0, 0, 1, 3]]; b = [6, 12, 18, 15]; solução ≈ [1, 2, 3, 4].  
9) **Cholesky (4)** — A = [[6, 2, 1], [2, 5, 2], [1, 2, 4]] (SPD); b = [8, 6, 9]; solução ≈ [1, 0, 2].  
10) **Cholesky (4)** — A = [[4, 1, 0, 0], [1, 4, 1, 0], [0, 1, 4, 1], [0, 0, 1, 3]] (SPD); b = [9, 6, 0, -3]; solução ≈ [2, 1, 0, -1].  
11) **Jacobi (5)** — A = [[10, -1, 2, 0], [-1, 11, -1, 3], [2, -1, 10, -1], [0, 3, -1, 8]]; b = [11, 12, 10, 10]; tol 1e-8; máx 100; stopCondition 0; solução ≈ [1, 1, 1, 1].  
12) **Jacobi (5)** — A = [[5, 2, 1], [1, 8, 1], [2, 1, 6]]; b = [15, 13, 23]; tol 1e-8; máx 100; stopCondition 1; solução ≈ [2, 1, 3].  
13) **Gauss–Seidel (6)** — A = [[4, 1, 1], [2, 7, 1], [1, -3, 12]]; b = [6, 10, 10]; tol 1e-8; máx 100; stopCondition 0; solução ≈ [1, 1, 1].  
14) **Gauss–Seidel (6)** — A = [[9, 1, 1, 0], [2, 10, 1, 2], [1, 0, 8, 1], [0, 2, 1, 7]]; b = [14, 33, 29, 35]; tol 1e-8; máx 100; stopCondition 0; solução ≈ [1, 2, 3, 4].  
15) **Gauss (0)** — A = [[10, 2, 3, 4], [1, 12, -1, 2], [3, -2, 15, 1], [2, 3, 4, 13]]; b = [39, 30, 48, 72]; solução ≈ [1, 2, 3, 4].  
