export const LinearSolverMethod = {
  Gauss: 0,
  GaussPartialPivoting: 1,
  GaussFullPivoting: 2,
  LU: 3,
  Cholesky: 4,
  Jacobi: 5,
  GaussSeidel: 6,
} as const;
export type LinearSolverMethodValue = (typeof LinearSolverMethod)[keyof typeof LinearSolverMethod];

export const IterativeStopCondition = {
  ByResidual: 0,
  ByDeltaX: 1,
} as const;
export type IterativeStopConditionValue = (typeof IterativeStopCondition)[keyof typeof IterativeStopCondition];

export const SolverStatus = {
  Success: 0,
  SingularMatrix: 1,
  NotSPD: 2,
  Divergence: 3,
  MaxIterationsReached: 4,
  InvalidInput: 5,
  NotImplemented: 6,
} as const;
export type SolverStatusValue = (typeof SolverStatus)[keyof typeof SolverStatus];

export const RootFindingMethod = {
  Bisection: 0,
  FixedPoint: 1,
  Newton: 2,
  RegulaFalsi: 3,
  Secant: 4,
} as const;
export type RootFindingMethodValue = (typeof RootFindingMethod)[keyof typeof RootFindingMethod];

export interface IterativeParamsDto {
  tolerance: number;
  maxIterations: number;
  stopCondition: IterativeStopConditionValue;
}

export interface LinearSystemSolveRequestDto {
  a: number[][];
  b: number[];
  method: LinearSolverMethodValue;
  iterativeParams?: IterativeParamsDto;
  returnSteps: boolean;
}

export interface LinearSystemSolveResponseDto {
  status: SolverStatusValue;
  solution: number[];
  iterations: number;
  elapsedMs: number;
  message: string;
}

export interface RootFindingSolveRequestDto {
  functionExpression: string;
  method: RootFindingMethodValue;
  a?: number;
  b?: number;
  initialGuess?: number;
  secondGuess?: number;
  tolerance: number;
  maxIterations: number;
  returnSteps: boolean;
}

export interface RootFindingSolveResponseDto {
  status: SolverStatusValue;
  root?: number | null;
  iterations: number;
  elapsedMs: number;
  message: string;
}
