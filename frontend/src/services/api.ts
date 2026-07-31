import { http } from '@/lib/http'
import type { Categoria, LoginResponse, Pagina, SolicitudDetalle, SolicitudEscritura, SolicitudListado, TransicionRequest, Usuario, UsuarioResumen } from '@/types/api'

export const api = {
  login: (email: string, password: string) => http<LoginResponse>('/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) }),
  me: () => http<Usuario>('/me'),
  categorias: () => http<Categoria[]>('/categorias'),
  agentes: () => http<UsuarioResumen[]>('/agentes'),
  solicitudes: (params: URLSearchParams) => http<Pagina<SolicitudListado>>(`/solicitudes?${params}`),
  solicitud: (id: string) => http<SolicitudDetalle>(`/solicitudes/${id}`),
  crearSolicitud: (body: SolicitudEscritura) => http<SolicitudDetalle>('/solicitudes', { method: 'POST', body: JSON.stringify(body) }),
  editarSolicitud: (id: string, body: SolicitudEscritura) => http<SolicitudDetalle>(`/solicitudes/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  transicionar: (id: string, body: TransicionRequest) => http<SolicitudDetalle>(`/solicitudes/${id}/transiciones`, { method: 'POST', body: JSON.stringify(body) }),
}
