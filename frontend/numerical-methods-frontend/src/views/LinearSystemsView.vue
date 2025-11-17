<template>
  <section class="card">
    <h2>Sistemas Lineares</h2>
    <form class="form-grid" @submit.prevent="solveSystem">
      <label>
        Dimensão (n)
        <input type="number" min="1" max="10" v-model.number="matrixSize" />
      </label>

      <label>
        Método
        <select v-model.number="method">
          <option v-for="option in linearSolverOptions" :key="option.value" :value="option.value">
            {{ option.label }}
          </option>
        </select>
      </label>

      <label>
        Tolerância
        <input type="number" step="any" v-model.number="iterativeParams.tolerance" />
      </label>

      <label>
        Máx. Iterações
        <input type="number" min="1" v-model.number="iterativeParams.maxIterations" />
      </label>

      <label>
        Condição de parada
        <select v-model.number="iterativeParams.stopCondition">
          <option :value="IterativeStopCondition.ByResidual">Resíduo</option>
          <option :value="IterativeStopCondition.ByDeltaX">Delta X</option>
        </select>
      </label>

      <button type="submit" :disabled="loading">{{ loading ? 'Resolvendo...' : 'Resolver sistema' }}</button>
    </form>

    <div class="matrix-inputs">
      <h3>Matriz A</h3>
      <table>
        <tbody>
          <tr v-for="(row, rowIndex) in aMatrix" :key="rowIndex">
            <td v-for="(_, colIndex) in row" :key="colIndex">
              <input
                type="number"
                step="any"
                  :value="row[colIndex]"
                @input="updateMatrixValue(rowIndex, colIndex, $event)"
              />
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="vector-inputs">
      <h3>Vetor b</h3>
      <div class="vector-list">
        <input
          v-for="(_, index) in bVector"
          :key="index"
          type="number"
          step="any"
          :value="bVector[index]"
          @input="updateVectorValue(index, $event)"
        />
      </div>
    </div>

    <section class="result" v-if="response">
      <header class="result-header">
        <span :class="statusClass">{{ statusLabel }}</span>
        <span class="result-meta">
          <span v-if="response.elapsedMs">Tempo: {{ response.elapsedMs.toFixed(3) }} ms</span>
          <span v-if="response.iterations !== undefined"> · Iterações: {{ response.iterations }}</span>
        </span>
      </header>

      <p v-if="response.message" class="result-message">
        {{ response.message }}
      </p>

      <div v-if="response.solution && response.solution.length" class="solution-block">
        <h4>Solução aproximada</h4>
        <ul class="solution-list">
          <li v-for="(value, index) in response.solution" :key="index">
            x{{ index + 1 }} = {{ value }}
          </li>
        </ul>
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
import { computed, reactive, ref, watch } from 'vue';
import type { Ref } from 'vue';
import { api } from '../services/api';
import type { LinearSystemSolveResponseDto, LinearSystemSolveRequestDto, IterativeStopConditionValue, LinearSolverMethodValue, SolverStatusValue } from '../types/numericalMethods';
import { IterativeStopCondition, LinearSolverMethod, SolverStatus } from '../types/numericalMethods';

const matrixSize = ref(3);
const method = ref<LinearSolverMethodValue>(LinearSolverMethod.Gauss);
const iterativeParams = reactive({
  tolerance: 1e-6,
  maxIterations: 100,
  stopCondition: IterativeStopCondition.ByResidual as IterativeStopConditionValue,
});
const aMatrix = ref(createMatrix(matrixSize.value)) as Ref<number[][]>;
const bVector = ref(createVector(matrixSize.value)) as Ref<number[]>;
const response = ref<LinearSystemSolveResponseDto | null>(null);
const error = ref<string | null>(null);
const loading = ref(false);
const showRawJson = ref(false);

const linearSolverOptions = [
  { value: LinearSolverMethod.Gauss, label: 'Gauss' },
  { value: LinearSolverMethod.GaussPartialPivoting, label: 'Gauss (pivoteamento parcial)' },
  { value: LinearSolverMethod.GaussFullPivoting, label: 'Gauss (pivoteamento completo)' },
  { value: LinearSolverMethod.LU, label: 'Fatoração LU' },
  { value: LinearSolverMethod.Cholesky, label: 'Cholesky' },
  { value: LinearSolverMethod.Jacobi, label: 'Jacobi' },
  { value: LinearSolverMethod.GaussSeidel, label: 'Gauss-Seidel' },
];

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

watch(matrixSize, (newSize) => {
  const size = Math.max(1, Math.min(10, newSize));
  if (size !== matrixSize.value) {
    matrixSize.value = size;
    return;
  }
  aMatrix.value = createMatrix(size);
  bVector.value = createVector(size);
});

function createMatrix(n: number) {
  return Array.from({ length: n }, () => Array.from({ length: n }, () => 0));
}

function createVector(n: number) {
  return Array.from({ length: n }, () => 0);
}

function updateMatrixValue(row: number, col: number, event: Event) {
  const target = event.target as HTMLInputElement | null;
  const value = target ? Number(target.value) : 0;
  const matrix = aMatrix.value!.map((r) => [...r]);
  const colCount = matrix[0]?.length ?? matrix.length;
  const rowValues = matrix[row] ?? Array.from({ length: colCount }, () => 0);
  rowValues[col] = Number.isFinite(value) ? value : 0;
  matrix[row] = rowValues;
  aMatrix.value = matrix;
}

function updateVectorValue(index: number, event: Event) {
  const target = event.target as HTMLInputElement | null;
  const value = target ? Number(target.value) : 0;
  const vector = [...bVector.value!];
  vector[index] = Number.isFinite(value) ? value : 0;
  bVector.value = vector;
}

async function solveSystem() {
  loading.value = true;
  error.value = null;
  response.value = null;
  showRawJson.value = false;

  try {
    const payload: LinearSystemSolveRequestDto = {
      a: aMatrix.value,
      b: bVector.value,
      method: method.value,
      iterativeParams: { ...iterativeParams },
    };

    const { data } = await api.post<LinearSystemSolveResponseDto>('/api/linear-systems/solve', payload);
    response.value = data;
  } catch (err) {
    error.value = 'Não foi possível enviar o sistema. Verifique o backend.';
    console.error(err);
  } finally {
    loading.value = false;
  }
}

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
  margin-bottom: 2rem;
}

.form-grid {
  display: grid;
  gap: 1rem;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  margin-bottom: 1.5rem;
}

label {
  display: flex;
  flex-direction: column;
  font-weight: 600;
  gap: 0.35rem;
}

input,
select,
button {
  padding: 0.5rem;
  border-radius: 4px;
  border: 1px solid #d0d7de;
}

button {
  background: #2563eb;
  color: #fff;
  border: none;
  cursor: pointer;
}

button:disabled {
  background: #93c5fd;
}

.matrix-inputs table {
  border-collapse: collapse;
}

.matrix-inputs td {
  padding: 0.15rem;
}

.matrix-inputs input,
.vector-list input {
  width: 100px;
}

.vector-list {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.result {
  background: #f8fafc;
  padding: 1rem;
  border-radius: 6px;
  margin-top: 1rem;
}

pre {
  white-space: pre-wrap;
  word-break: break-word;
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

.solution-list {
  list-style: none;
  padding-left: 0;
}

.solution-list li {
  font-family: monospace;
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
