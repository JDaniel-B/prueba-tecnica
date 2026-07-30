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

## Comandos individuales

```sh
npm run dev:api
npm run dev:web
npm run build
npm test
npm run lint
```
