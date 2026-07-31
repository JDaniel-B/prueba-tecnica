# Guía para la entrevista técnica

## Explicación de dos minutos

MesaSitec es un monorepo con una API ASP.NET Core y un frontend Vue 3. La API
está separada en Dominio, Aplicación, Infraestructura y API. El tenant y usuario
se obtienen siempre del JWT; nunca se aceptan desde el cliente. La entidad
`Solicitud` protege el workflow y el cálculo de SLA. Entity Framework persiste
en SQLite y Vue consume contratos TypeScript mediante un cliente HTTP único.

## Equivalencias con tu experiencia

| Next.js / React / Express | MesaSitec |
|---|---|
| Componente React | Componente `.vue` con `<script setup>` |
| `useState` | `ref` o `reactive` |
| `useMemo` | `computed` |
| Zustand/Redux | Pinia |
| React Router / rutas Next | Vue Router |
| Router de Express | Controller de ASP.NET Core |
| Middleware de errores | `GlobalExceptionHandler` |
| Servicio de Node | Servicio de Aplicación |
| Modelo/ORM | Entidad de Dominio + repositorio EF Core |
| Migración SQL | Migración de Entity Framework |

## Recorrido de una petición

Ejemplo: resolver una solicitud.

1. Vue llama `api.transicionar()` desde `SolicitudDetalleView.vue`.
2. `http.ts` agrega `Authorization: Bearer ...`.
3. `SolicitudesController` extrae `sub`, `tenantId` y `rol` del JWT.
4. `SolicitudTransicionService` carga por `id + tenantId` y valida permisos.
5. `Solicitud.Resolver()` valida estado, motivo y fecha UTC.
6. Entity Framework guarda y el repositorio de consulta arma la respuesta.
7. Los errores se convierten en `application/problem+json` con un `codigo`.

## Archivos para cambios frecuentes

- Nueva regla de estados: `Dominio/Entidades/Solicitud.cs` y sus tests.
- Nuevo permiso: `SolicitudTransicionService.cs` y
  `frontend/src/domain/permisosSolicitud.ts`.
- Nuevo filtro: contrato, servicio/repository de consulta y
  `SolicitudesView.vue`.
- Nuevo campo persistido: entidad, configuración EF, migración, DTO y formulario.
- Cambio de autenticación: `AuthService`, generador JWT, store Pinia y guard.

## Preguntas que debes poder responder

### ¿Por qué 404 para otro tenant?

Evita confirmar que el recurso existe. El repositorio filtra por tenant antes de
devolverlo, por eso para el usuario simplemente no existe.

### ¿Por qué la lógica de estados está en la entidad?

Para que ninguna entrada —HTTP, tests o un futuro proceso en segundo plano— pueda
saltar reglas modificando el estado directamente.

### ¿Por qué SQLite?

Es suficiente para la prueba, no requiere servidor y conserva migraciones y
relaciones reales. En producción se puede cambiar el proveedor manteniendo EF.

### ¿Qué problema apareció con las fechas?

SQLite devolvía `DateTime` sin `Kind.Utc`. El cálculo de SLA lo detectó en una
prueba HTTP real. Se agregó un `ValueConverter` y una prueba que limpia el
tracking para forzar lectura desde disco.

### ¿Qué mejorarías en producción?

Historial de transiciones, observabilidad, pruebas E2E en CI, concurrencia del
correlativo, rotación segura del secreto JWT y una base de datos administrada.

## Cambio en vivo recomendado para practicar

Agrega una prioridad visual distinta para solicitudes críticas:

1. Modifica solamente la clase calculada en `SolicitudesView.vue`.
2. Añade estilos para `Critica`, `Alta`, `Media` y `Baja`.
3. Actualiza o agrega un test Vue.
4. Ejecuta `npm run test:web`, `npm run build:web` y `npm run lint`.

La clave durante la entrevista es explicar primero qué capa debe cambiar y por
qué, antes de escribir código.
