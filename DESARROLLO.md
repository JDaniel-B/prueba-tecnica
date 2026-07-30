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
