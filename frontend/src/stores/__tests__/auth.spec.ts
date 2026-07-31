import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { api } from '@/services/api'
import { useAuthStore } from '@/stores/auth'
import type { Usuario } from '@/types/api'

vi.mock('@/services/api', () => ({
  api: {
    login: vi.fn(),
    me: vi.fn(),
  },
}))

const usuario: Usuario = {
  id: 'usuario-1',
  nombre: 'Administradora Norte',
  email: 'admin@norte.test',
  rol: 'Admin',
  tenantId: 'tenant-1',
  tenantNombre: 'Cooperativa Norte',
}

describe('sesión Pinia', () => {
  beforeEach(() => {
    localStorage.clear()
    setActivePinia(createPinia())
  })

  it('descarta una sesión local corrupta sin romper la aplicación', () => {
    localStorage.setItem('mesasitec_token', 'token-invalido')
    localStorage.setItem('mesasitec_usuario', '{json-invalido')

    const auth = useAuthStore()

    expect(auth.autenticado).toBe(false)
    expect(auth.usuario).toBeNull()
  })

  it('recupera el perfil cuando existe token pero falta el usuario', async () => {
    localStorage.setItem('mesasitec_token', 'token-valido')
    vi.mocked(api.me).mockResolvedValue(usuario)
    const auth = useAuthStore()

    const restaurada = await auth.restaurar()

    expect(restaurada).toBe(true)
    expect(auth.usuario).toEqual(usuario)
    expect(localStorage.getItem('mesasitec_usuario')).toContain('Administradora Norte')
  })

  it('limpia el token si no puede recuperar el perfil', async () => {
    localStorage.setItem('mesasitec_token', 'token-vencido')
    vi.mocked(api.me).mockRejectedValue(new Error('No autenticado'))
    const auth = useAuthStore()

    const restaurada = await auth.restaurar()

    expect(restaurada).toBe(false)
    expect(auth.autenticado).toBe(false)
    expect(auth.usuario).toBeNull()
  })
})
