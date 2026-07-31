import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { api } from '@/services/api'
import type { Usuario } from '@/types/api'

const USUARIO_KEY = 'mesasitec_usuario'
const TOKEN_KEY = 'mesasitec_token'

export const useAuthStore = defineStore('auth', () => {
  const guardado = localStorage.getItem(USUARIO_KEY)
  const usuario = ref<Usuario | null>(guardado ? (JSON.parse(guardado) as Usuario) : null)
  const autenticado = computed(() => Boolean(localStorage.getItem(TOKEN_KEY)))

  async function login(email: string, password: string) {
    const respuesta = await api.login(email, password)
    localStorage.setItem(TOKEN_KEY, respuesta.accessToken)
    localStorage.setItem(USUARIO_KEY, JSON.stringify(respuesta.usuario))
    usuario.value = respuesta.usuario
  }

  async function restaurar() {
    if (!autenticado.value) return false
    try {
      usuario.value = await api.me()
      localStorage.setItem(USUARIO_KEY, JSON.stringify(usuario.value))
      return true
    } catch { return false }
  }

  function logout() {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USUARIO_KEY)
    usuario.value = null
  }

  return { usuario, autenticado, login, restaurar, logout }
})
