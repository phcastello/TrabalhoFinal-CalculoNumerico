<template>
  <section class="card">
    <h2>Raízes de Funções</h2>
    <form class="form-grid" @submit.prevent="solveRoot()">
      <label>
        Função f(x)
        <input type="text" v-model="rootRequest.functionExpression" placeholder="Ex.: x^3 - 2*x - 5" />
      </label>

      <label>
        Método
        <select v-model.number="rootRequest.method">
          <option v-for="option in rootMethods" :key="option.value" :value="option.value">
            {{ option.label }}
          </option>
        </select>
      </label>

      <label>
        Intervalo [a, b] - a
        <input type="number" step="any" :value="rootRequest.a ?? ''" @input="updateNullableField('a', $event)" />
      </label>

      <label>
        Intervalo [a, b] - b
        <input type="number" step="any" :value="rootRequest.b ?? ''" @input="updateNullableField('b', $event)" />
      </label>

      <label>
        Chute inicial
        <input type="number" step="any" :value="rootRequest.initialGuess ?? ''" @input="updateNullableField('initialGuess', $event)" />
      </label>

      <label>
        Segundo chute
        <input type="number" step="any" :value="rootRequest.secondGuess ?? ''" @input="updateNullableField('secondGuess', $event)" />
      </label>

      <label>
        Tolerância
        <input type="number" step="any" v-model.number="rootRequest.tolerance" />
      </label>

      <label>
        Máx. Iterações
        <input type="number" min="1" v-model.number="rootRequest.maxIterations" />
      </label>

      <label class="checkbox">
        <input type="checkbox" v-model="rootRequest.returnSteps" />
        Retornar passos (stub)
      </label>

      <button type="submit" :disabled="loading">{{ loading ? 'Calculando...' : 'Encontrar raiz' }}</button>
    </form>

    <section class="result" v-if="response">
      <header class="result-header">
        <span :class="statusClass">{{ statusLabel }}</span>
        <span class="result-meta">
          <span v-if="response.elapsedMs">Tempo: {{ response.elapsedMs.toFixed(3) }} ms</span>
          <span> · Iterações: {{ response.iterations }}</span>
        </span>
      </header>

      <p v-if="response.message" class="result-message">
        {{ response.message }}
      </p>

      <div v-if="response.root !== null && response.root !== undefined" class="solution-block">
        <h4>Raiz aproximada</h4>
        <p class="solution-value">x ≈ {{ response.root }}</p>
      </div>

      <button type="button" class="toggle-json" @click="showRawJson = !showRawJson">
        {{ showRawJson ? 'Ocultar JSON bruto' : 'Ver JSON bruto' }}
      </button>

      <pre v-if="showRawJson" class="raw-json">{{ JSON.stringify(response, null, 2) }}</pre>
    </section>
    <p class="error" v-if="error">{{ error }}</p>
  </section>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { api } from '../services/api';
import type { RootFindingSolveRequestDto, RootFindingSolveResponseDto, RootFindingMethodValue, SolverStatusValue } from '../types/numericalMethods';
import { RootFindingMethod, SolverStatus } from '../types/numericalMethods';

type NullableField = 'a' | 'b' | 'initialGuess' | 'secondGuess';

interface RootFormRequest {
  functionExpression: string;
  method: RootFindingMethodValue;
  a: number | null;
  b: number | null;
  initialGuess: number | null;
  secondGuess: number | null;
  tolerance: number;
  maxIterations: number;
  returnSteps: boolean;
}

const rootRequest = reactive<RootFormRequest>({
  functionExpression: '',
  method: RootFindingMethod.Bisection,
  a: null,
  b: null,
  initialGuess: null,
  secondGuess: null,
  tolerance: 1e-6,
  maxIterations: 100,
  returnSteps: false,
});

const response = ref<RootFindingSolveResponseDto | null>(null);
const error = ref<string | null>(null);
const loading = ref(false);
const showRawJson = ref(false);

const rootMethods = [
  { value: RootFindingMethod.Bisection, label: 'Bisseção' },
  { value: RootFindingMethod.FixedPoint, label: 'Ponto Fixo' },
  { value: RootFindingMethod.Newton, label: 'Newton' },
  { value: RootFindingMethod.RegulaFalsi, label: 'Regula Falsi' },
  { value: RootFindingMethod.Secant, label: 'Secante' },
];

function updateNullableField(field: NullableField, event: Event) {
  const target = event.target as HTMLInputElement | null;
  const rawValue = target?.value ?? '';

  if (!rawValue && rawValue !== '0') {
    rootRequest[field] = null;
    return;
  }

  const value = Number(rawValue);
  rootRequest[field] = Number.isFinite(value) ? value : null;
}

async function solveRoot() {
  loading.value = true;
  error.value = null;
  response.value = null;
  showRawJson.value = false;

  try {
    const payload: RootFindingSolveRequestDto = {
      functionExpression: rootRequest.functionExpression,
      method: rootRequest.method,
      a: rootRequest.a ?? undefined,
      b: rootRequest.b ?? undefined,
      initialGuess: rootRequest.initialGuess ?? undefined,
      secondGuess: rootRequest.secondGuess ?? undefined,
      tolerance: rootRequest.tolerance,
      maxIterations: rootRequest.maxIterations,
      returnSteps: rootRequest.returnSteps,
    };

    const { data } = await api.post<RootFindingSolveResponseDto>('/api/roots/solve', payload);
    response.value = data;
  } catch (err) {
    error.value = 'Não foi possível enviar a requisição. Verifique o backend.';
    console.error(err);
  } finally {
    loading.value = false;
  }
}

const statusLabel = computed(() => {
  if (!response.value) {
    return '';
  }
  return mapStatusToLabel(response.value.status);
});

const statusClass = computed(() => {
  if (!response.value) {
    return 'status';
  }
  return mapStatusToClass(response.value.status);
});

function mapStatusToLabel(status: SolverStatusValue): string {
  switch (status) {
    case SolverStatus.Success:
      return 'Sucesso';
    case SolverStatus.SingularMatrix:
      return 'Matriz singular';
    case SolverStatus.NotSPD:
      return 'Matriz não SPD';
    case SolverStatus.Divergence:
      return 'Divergência';
    case SolverStatus.MaxIterationsReached:
      return 'Máx. iterações atingido';
    case SolverStatus.InvalidInput:
      return 'Entrada inválida';
    case SolverStatus.NotImplemented:
    default:
      return 'Não implementado';
  }
}

function mapStatusToClass(status: SolverStatusValue): string {
  switch (status) {
    case SolverStatus.Success:
      return 'status success';
    case SolverStatus.SingularMatrix:
    case SolverStatus.NotSPD:
    case SolverStatus.Divergence:
    case SolverStatus.MaxIterationsReached:
    case SolverStatus.InvalidInput:
      return 'status error';
    case SolverStatus.NotImplemented:
    default:
      return 'status warning';
  }
}
</script>

<style scoped>
.card {
  border: 1px solid #d0d7de;
  border-radius: 8px;
  padding: 1.5rem;
}

.form-grid {
  display: grid;
  gap: 1rem;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  margin-bottom: 1.5rem;
}

label {
  display: flex;
  flex-direction: column;
  font-weight: 600;
  gap: 0.35rem;
}

.checkbox {
  flex-direction: row;
  align-items: center;
  font-weight: 500;
}

input,
select,
button {
  padding: 0.5rem;
  border-radius: 4px;
  border: 1px solid #d0d7de;
}

button {
  background: #16a34a;
  color: #fff;
  border: none;
  cursor: pointer;
}

button:disabled {
  background: #86efac;
}

.result {
  background: #f8fafc;
  padding: 1rem;
  border-radius: 6px;
  margin-top: 1rem;
}

.result-header {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  margin-bottom: 0.5rem;
}

.status {
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  font-size: 0.85rem;
  font-weight: 600;
}

.status.success {
  background-color: #dcfce7;
  color: #166534;
}

.status.error {
  background-color: #fee2e2;
  color: #b91c1c;
}

.status.warning {
  background-color: #fef3c7;
  color: #92400e;
}

.result-meta {
  font-size: 0.85rem;
  color: #64748b;
}

.result-message {
  margin: 0.5rem 0;
  color: #334155;
}

.solution-block {
  margin-top: 0.75rem;
}

.solution-value {
  font-family: monospace;
  font-size: 1rem;
  margin: 0.25rem 0 0;
}

.toggle-json {
  margin-top: 0.75rem;
  font-size: 0.8rem;
  background: transparent;
  border: none;
  color: #2563eb;
  cursor: pointer;
  padding: 0;
}

.toggle-json:hover {
  text-decoration: underline;
}

.raw-json {
  margin-top: 0.5rem;
  padding: 0.5rem;
  background-color: #0f172a;
  color: #e5e7eb;
  border-radius: 0.5rem;
  max-height: 240px;
  overflow: auto;
  font-size: 0.75rem;
}

.error {
  color: #dc2626;
  font-weight: 600;
}
</style>
