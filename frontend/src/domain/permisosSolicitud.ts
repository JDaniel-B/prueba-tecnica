import type { Accion, Estado, Rol } from '@/types/api'

const estadosAsignables: Estado[] = ['Nueva', 'Asignada', 'EnProceso']

export function puedeEditarSolicitud(estado: Estado, rol: Rol): boolean {
  return rol === 'Admin' || rol === 'Agente' || estado === 'Nueva'
}

export function accionesPermitidas(estado: Estado, rol: Rol): Accion[] {
  const esStaff = rol === 'Admin' || rol === 'Agente'
  const acciones: Accion[] = []

  if (esStaff && estadosAsignables.includes(estado)) acciones.push('asignar')
  if (esStaff && estado === 'Asignada') acciones.push('iniciar')
  if (esStaff && estado === 'EnProceso') acciones.push('resolver')
  if (estado === 'Resuelta') acciones.push('cerrar')
  if (esStaff && estado === 'Resuelta') acciones.push('reabrir')
  if (rol === 'Admin' && estadosAsignables.includes(estado)) acciones.push('cancelar')

  return acciones
}
