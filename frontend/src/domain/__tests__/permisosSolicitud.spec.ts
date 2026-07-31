import { describe, expect, it } from 'vitest'
import { accionesPermitidas, puedeEditarSolicitud } from '@/domain/permisosSolicitud'

describe('permisos de solicitudes', () => {
  it('muestra a un solicitante solo cerrar cuando la solicitud está resuelta', () => {
    expect(accionesPermitidas('Nueva', 'Solicitante')).toEqual([])
    expect(accionesPermitidas('Resuelta', 'Solicitante')).toEqual(['cerrar'])
    expect(accionesPermitidas('Cerrada', 'Solicitante')).toEqual([])
  })

  it('muestra al agente las acciones del flujo excepto cancelar', () => {
    expect(accionesPermitidas('Nueva', 'Agente')).toEqual(['asignar'])
    expect(accionesPermitidas('Asignada', 'Agente')).toEqual(['asignar', 'iniciar'])
    expect(accionesPermitidas('EnProceso', 'Agente')).toEqual(['asignar', 'resolver'])
    expect(accionesPermitidas('Resuelta', 'Agente')).toEqual(['cerrar', 'reabrir'])
  })

  it('permite al administrador cancelar estados activos', () => {
    expect(accionesPermitidas('Nueva', 'Admin')).toEqual(['asignar', 'cancelar'])
    expect(accionesPermitidas('Cerrada', 'Admin')).toEqual([])
    expect(accionesPermitidas('Cancelada', 'Admin')).toEqual([])
  })

  it('restringe edición del solicitante a estado Nueva', () => {
    expect(puedeEditarSolicitud('Nueva', 'Solicitante')).toBe(true)
    expect(puedeEditarSolicitud('Asignada', 'Solicitante')).toBe(false)
    expect(puedeEditarSolicitud('Cerrada', 'Admin')).toBe(true)
  })
})
