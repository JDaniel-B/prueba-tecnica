# Desarrollo local

## Preparación inicial

Desde la raíz del repositorio:

```sh
copy .env.example .env
dotnet restore backend/MesaSitec.sln
npm install
```

## Levantar el proyecto

```sh
npm run dev
```

El comando inicia ambos procesos:

- API: `http://localhost:5080`
- Swagger: `http://localhost:5080/swagger`
- Frontend: `http://localhost:5173`

Para detenerlos, presiona `Ctrl+C`.

La API crea automáticamente `backend/src/MesaSitec.Api/mesasitec.db` y aplica las
migraciones pendientes. El archivo es local y está excluido de Git.

Si la base está vacía, también crea automáticamente los dos tenants, siete usuarios,
ocho categorías y 33 solicitudes requeridas. Todos los usuarios usan:

```text
Contraseña: Sitec.2026
```

Algunas credenciales disponibles:

```text
admin@norte.test
agente1@norte.test
user1@norte.test
admin@sur.test
user1@sur.test
```

Puedes probar `POST /api/v1/auth/login` desde Swagger. Copia el valor de
`accessToken`, pulsa **Authorize** y pégalo para consultar `GET /api/v1/me`.
El token dura ocho horas y contiene los claims `sub`, `tenantId`, `rol` y
`email`.

Con la sesión autorizada también puedes probar el listado paginado:

```text
GET /api/v1/categorias
GET /api/v1/solicitudes?page=1&pageSize=20&sort=-fechaCreacion
GET /api/v1/solicitudes?estado=EnProceso&prioridad=Alta&vencidas=true
GET /api/v1/solicitudes/40000000-0000-0000-0000-000000000014
```

`GET /api/v1/categorias` devuelve las categorías activas que pueden usarse en
los filtros y formularios.

Un usuario `Admin` o `Agente` ve las solicitudes de su organización. Un usuario
`Solicitante` ve únicamente las que él creó; el servidor obtiene ambos límites
desde el JWT, nunca desde parámetros enviados por el cliente.

En el detalle, un recurso inexistente o perteneciente a otro tenant responde
`404 / RECURSO_NO_ENCONTRADO`. Un usuario `Solicitante` que intenta consultar
una solicitud ajena de su propia organización recibe
`403 / OPERACION_NO_PERMITIDA`.

Para crear una solicitud:

```http
POST /api/v1/solicitudes
Content-Type: application/json

{
  "titulo": "No puedo acceder al portal",
  "descripcion": "El portal rechaza mis credenciales de acceso.",
  "categoriaId": "20000000-0000-0000-0000-000000000001",
  "prioridad": "Alta"
}
```

El servidor obtiene tenant y solicitante desde el JWT, genera el código, calcula
el SLA y responde `201` con una cabecera `Location`. Los IDs de categoría del
ejemplo pertenecen a los datos semilla de Cooperativa Norte.

Para actualizar una solicitud existente se envía el mismo contrato de escritura:

```http
PUT /api/v1/solicitudes/40000000-0000-0000-0000-000000000001
Content-Type: application/json

{
  "titulo": "No puedo acceder al portal desde ayer",
  "descripcion": "El portal continúa rechazando mis credenciales de acceso.",
  "categoriaId": "20000000-0000-0000-0000-000000000001",
  "prioridad": "Critica"
}
```

`Admin` y `Agente` pueden editar solicitudes de su organización. Un
`Solicitante` únicamente puede editar una solicitud propia mientras permanezca
en estado `Nueva`. Si cambia la categoría o prioridad de una solicitud no
terminal, el SLA se recalcula desde su fecha de creación original.

El flujo de estados se ejecuta desde el detalle de Vue o desde Swagger con:

```http
POST /api/v1/solicitudes/{id}/transiciones
Content-Type: application/json

{ "accion": "asignar", "agenteId": "10000000-0000-0000-0000-000000000002" }
{ "accion": "iniciar" }
{ "accion": "resolver", "motivo": "Se corrigió el incidente y el usuario validó el acceso." }
```

También están disponibles `cerrar`, `reabrir` y `cancelar`. El endpoint auxiliar
`GET /api/v1/agentes` lista únicamente administradores y agentes activos del
tenant actual para alimentar el selector de asignación.

El frontend en `http://localhost:5173` incluye login, listado con filtros y
paginación server-side, creación, edición, detalle y acciones por estado. El
cliente HTTP centralizado agrega el JWT y elimina la sesión si recibe un `401`.

Las fechas se calculan a partir de `SEED_FECHA_BASE`. Su valor predeterminado es
`2026-01-15T08:00:00Z` y solo se utiliza cuando la base todavía está vacía.

## Crear una migración

Restaura la herramienta local la primera vez:

```sh
dotnet tool restore
```

Después de cambiar el modelo o sus configuraciones:

```sh
dotnet ef migrations add NombreDeLaMigracion --project backend/src/MesaSitec.Infraestructura --startup-project backend/src/MesaSitec.Api --context MesaSitecDbContext --output-dir Persistencia/Migraciones
```

## Comandos individuales

```sh
npm run dev:api
npm run dev:web
npm run build
npm test
npm run lint
```
