# Desarrollo local

## Preparación inicial

Desde la raíz del repositorio:

```sh
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
