export type Rol = 'Admin' | 'Agente' | 'Solicitante'
export type Estado = 'Nueva' | 'Asignada' | 'EnProceso' | 'Resuelta' | 'Cerrada' | 'Cancelada'
export type Prioridad = 'Critica' | 'Alta' | 'Media' | 'Baja'
export type Accion = 'asignar' | 'iniciar' | 'resolver' | 'cerrar' | 'reabrir' | 'cancelar'

export interface Usuario { id: string; nombre: string; email: string; rol: Rol; tenantId: string; tenantNombre: string }
export interface UsuarioResumen { id: string; nombre: string }
export interface Categoria { id: string; nombre: string; slaHoras: number }
export interface LoginResponse { accessToken: string; expiraEn: number; usuario: Usuario }
export interface SolicitudListado { id: string; codigo: string; titulo: string; estado: Estado; prioridad: Prioridad; categoria: Categoria; agente: UsuarioResumen | null; fechaCreacion: string; fechaLimiteSla: string; vencida: boolean }
export interface SolicitudDetalle extends SolicitudListado { descripcion: string; solicitante: UsuarioResumen; fechaResolucion: string | null; motivoResolucion: string | null; motivoCancelacion: string | null }
export interface Pagina<T> { items: T[]; page: number; pageSize: number; total: number; totalPaginas: number }
export interface SolicitudEscritura { titulo: string; descripcion: string; categoriaId: string; prioridad: Prioridad }
export interface TransicionRequest { accion: Accion; agenteId?: string; motivo?: string }
export interface ProblemaApi { title?: string; detail?: string; codigo?: string; errores?: Record<string, string[]> }
