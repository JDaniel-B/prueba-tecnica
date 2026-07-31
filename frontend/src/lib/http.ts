import type { ProblemaApi } from '@/types/api'

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5080/api/v1'

export class ApiError extends Error {
  constructor(public status: number, public problema: ProblemaApi) {
    super(problema.detail ?? problema.title ?? 'No fue posible completar la solicitud.')
  }
}

export async function http<T>(ruta: string, opciones: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem('mesasitec_token')
  const headers = new Headers(opciones.headers)
  headers.set('Accept', 'application/json')
  if (opciones.body) headers.set('Content-Type', 'application/json')
  if (token) headers.set('Authorization', `Bearer ${token}`)

  const respuesta = await fetch(`${API_URL}${ruta}`, { ...opciones, headers })
  if (!respuesta.ok) {
    const problema = (await respuesta.json().catch(() => ({}))) as ProblemaApi
    if (respuesta.status === 401 && ruta !== '/auth/login') {
      localStorage.removeItem('mesasitec_token')
      localStorage.removeItem('mesasitec_usuario')
      window.location.assign('/login')
    }
    throw new ApiError(respuesta.status, problema)
  }
  return respuesta.json() as Promise<T>
}
