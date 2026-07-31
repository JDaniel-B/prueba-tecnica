<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { ApiError } from '@/lib/http'

const email = ref('admin@norte.test')
const password = ref('Sitec.2026')
const error = ref('')
const cargando = ref(false)
const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

async function enviar() {
  error.value = ''; cargando.value = true
  try {
    await auth.login(email.value, password.value)
    await router.push(typeof route.query.redirect === 'string' ? route.query.redirect : '/solicitudes')
  } catch (e) { error.value = e instanceof ApiError ? e.message : 'No fue posible iniciar sesión.' }
  finally { cargando.value = false }
}
</script>

<template>
  <main class="login-page">
    <form class="card login" @submit.prevent="enviar">
      <p class="eyebrow">Mesa de servicio</p><h1>Bienvenido a MesaSitec</h1>
      <p>Ingresa con las credenciales de tu organización.</p>
      <div class="field"><label for="email">Correo</label><input id="email" v-model="email" data-testid="login-email" type="email" required></div>
      <div class="field"><label for="password">Contraseña</label><input id="password" v-model="password" data-testid="login-password" type="password" required></div>
      <p v-if="error" class="error" data-testid="login-error">{{ error }}</p>
      <button class="btn" data-testid="login-submit" :disabled="cargando">{{ cargando ? 'Ingresando…' : 'Ingresar' }}</button>
    </form>
  </main>
</template>

<style scoped>
.login-page{display:grid;min-height:100vh;padding:1rem;place-items:center;background:linear-gradient(135deg,#eef4ff,#f8fafc)}
.login{display:grid;gap:1rem;width:min(100%,28rem);padding:2rem}.login h1,.login p{margin:0}.eyebrow{color:#2767dc;font-size:.8rem;font-weight:800;letter-spacing:.1em;text-transform:uppercase}
</style>
