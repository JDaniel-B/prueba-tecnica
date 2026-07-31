<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'

const router = useRouter()
const auth = useAuthStore()
const toast = useToastStore()
function salir() { auth.logout(); void router.push('/login') }
</script>

<template>
  <header v-if="auth.autenticado" class="nav" data-testid="app-nav">
    <RouterLink class="brand" to="/solicitudes">MesaSitec</RouterLink>
    <div class="nav__user">
      <span data-testid="nav-usuario-nombre">{{ auth.usuario?.nombre }}</span>
      <small data-testid="nav-usuario-rol">{{ auth.usuario?.rol }}</small>
      <button class="btn btn--ghost" data-testid="btn-logout" @click="salir">Salir</button>
    </div>
  </header>
  <div v-if="toast.mensaje" class="toast" data-testid="toast-mensaje" role="status" aria-live="polite">{{ toast.mensaje }}</div>
  <RouterView />
</template>

<style>
:root {
  color: #172033;
  background: #f4f7fb;
  font-family:
    Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI",
    sans-serif;
  font-synthesis: none;
  text-rendering: optimizeLegibility;
}

* {
  box-sizing: border-box;
}

body {
  min-width: 320px;
  min-height: 100vh;
  margin: 0;
}

a { color: inherit; text-decoration: none; }
.nav { display:flex; align-items:center; justify-content:space-between; padding:1rem clamp(1rem,5vw,4rem); background:#11213c; color:white; }
.brand { font-size:1.25rem; font-weight:800; }
.nav__user { display:flex; align-items:center; gap:.8rem; }
.nav__user small { color:#aebbd1; }
.page { width:min(1180px, calc(100% - 2rem)); margin:2rem auto; }
.card { padding:1.25rem; border:1px solid #dce4ef; border-radius:1rem; background:white; box-shadow:0 .5rem 2rem rgb(32 55 89 / 7%); }
.btn { display:inline-flex; justify-content:center; align-items:center; min-height:2.65rem; padding:.65rem 1rem; border:0; border-radius:.65rem; background:#2767dc; color:white; cursor:pointer; font-weight:700; }
.btn:disabled { opacity:.55; cursor:not-allowed; }
.btn--ghost { border:1px solid #bdc9dc; background:transparent; }
.btn--danger { background:#b42318; }
.field { display:grid; gap:.4rem; }
.field label { color:#45546d; font-size:.88rem; font-weight:700; }
.field input,.field select,.field textarea { width:100%; padding:.7rem .8rem; border:1px solid #bdc9d8; border-radius:.55rem; background:white; }
.error { color:#b42318; }
.toast { position:fixed; z-index:20; right:1rem; bottom:1rem; max-width:24rem; padding:1rem; border-radius:.7rem; background:#11213c; color:white; box-shadow:0 1rem 3rem #0003; }
.badge { display:inline-flex; padding:.25rem .55rem; border-radius:999px; background:#e8eef8; font-size:.8rem; font-weight:700; }
.loading,.empty,.error-box { padding:2rem; text-align:center; }

@media (max-width: 560px) {
  .nav { align-items:flex-start; gap:.75rem; }
  .nav__user { justify-content:flex-end; flex-wrap:wrap; }
  .nav__user span { width:100%; text-align:right; }
  .page { margin:1rem auto; }
}

button,
input,
select,
textarea {
  font: inherit;
}
</style>
