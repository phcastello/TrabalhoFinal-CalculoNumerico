<template>
  <div class="app-shell">
    <header class="app-header">
      <h1>Métodos Numéricos</h1>
      <nav class="app-nav">
        <RouterLink to="/linear-systems" active-class="active">Sistemas Lineares</RouterLink>
        <RouterLink to="/root-finding" active-class="active">Raízes de Funções</RouterLink>
      </nav>
    </header>
    <main>
      <RouterView />
    </main>
  </div>
</template>

<script setup lang="ts">
</script>

<style scoped>
.app-shell {
  margin: 0 auto;
  max-width: 1200px;
  padding: 2rem;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.app-header {
  border-bottom: 1px solid #d0d7de;
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
}

.app-nav {
  display: flex;
  gap: 1rem;
  margin-top: 0.75rem;
}

.app-nav a {
  text-decoration: none;
  color: #2563eb;
  font-weight: 600;
}

.app-nav a.active {
  color: #1d4ed8;
  border-bottom: 2px solid #1d4ed8;
}
</style>
