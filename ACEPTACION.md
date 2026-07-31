# Validación de aceptación

Última ejecución: 31 de julio de 2026.

## Instalación limpia

La API se inició usando una ruta SQLite temporal vacía, sin reutilizar la base
de desarrollo. En el arranque se verificó que:

- las migraciones se aplican automáticamente;
- la semilla crea 25 solicitudes de Cooperativa Norte y 8 de Bufete Sur;
- cada tenant obtiene sus 4 categorías;
- `GET /api/v1/health` responde `{ "estado": "ok" }` sin autenticación;
- Swagger publica exactamente las 9 operaciones exigidas;
- la base temporal se eliminó después de la prueba.

## Recorrido de API

| Caso | Resultado |
|---|---|
| Login y `GET /me` | 200, perfil y rol correctos |
| Solicitante Norte | Solo 13 solicitudes propias |
| Usuario Sur consulta un ID de Norte | 404 |
| Crear solicitud | 201, `Location`, código `SOL-2026-00026` |
| Editar solicitud propia Nueva | 200 |
| Asignar | `Asignada` |
| Iniciar | `EnProceso` |
| Resolver con motivo | `Resuelta` y fecha de resolución |
| Cerrar por el solicitante propietario | `Cerrada` |
| Agente intenta cancelar | 403 |

## Frontend y calidad

- Las rutas privadas usan un guard de Vue Router.
- El cliente HTTP central agrega el JWT y procesa `problem+json`.
- Los botones de acción se calculan combinando rol y estado y no se renderizan
  cuando no están permitidos.
- Los `data-testid` exigidos están implementados en las vistas y componentes.
- El texto de paginación conserva `Página X de Y — Z resultados`.
- No hay `any` explícito en `frontend/src`.
- No hay `.env`, bases SQLite, `bin`, `obj`, `dist` ni `node_modules`
  versionados.

Comandos ejecutados satisfactoriamente:

```sh
npm test       # 85 pruebas xUnit + 12 pruebas Vitest
npm run build  # .NET, vue-tsc y Vite
npm run lint   # ESLint
```

`npm audit --omit=dev` reporta 0 vulnerabilidades de producción. Las alertas
del árbol de desarrollo provienen de dependencias transitivas de Vue Test Utils
y su corrección automática requiere un downgrade forzado.

## Acción externa pendiente

Antes de entregar, el propietario del repositorio debe confirmar en GitHub que
el usuario `osanchezm` tiene acceso. Esta acción no puede verificarse desde una
instalación local.

La inspección automatizada del navegador depende del entorno del evaluador. El
recorrido visual por rol que debe ejecutarse antes de entregar está en
`REVISION_MANUAL.md`.
