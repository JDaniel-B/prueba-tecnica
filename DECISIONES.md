# Decisiones técnicas

## Decisiones

1. Se eligió un monorepo con `backend/` y `frontend/`, siguiendo el contrato del
   ejercicio. Se descartó separar las aplicaciones en repositorios distintos porque
   complicaría la instalación y la trazabilidad de una entrega pequeña.
2. El backend usa cuatro proyectos (`Api`, `Aplicacion`, `Dominio` e
   `Infraestructura`). Se descartó colocar toda la lógica en la API porque las reglas
   de estados, permisos y SLA deben poder probarse sin iniciar el servidor.
3. Se mantuvo ESLint como único linter del frontend. Se descartó Oxlint porque las
   versiones generadas por `create-vue` tenían un conflicto de dependencias y un
   segundo linter no aporta valor suficiente al alcance de esta prueba.
4. Las consultas multi-tenant reciben el `tenantId` desde el token y lo aplican como
   primer predicado en el repositorio. Para un usuario `Solicitante`, el servicio
   agrega además su `usuarioId`. Se descartó aceptar el tenant desde parámetros HTTP
   porque permitiría consultar otra organización manipulando la petición.

## Uso de IA

Usé Codex como acompañamiento para analizar el enunciado, proponer la arquitectura,
generar el scaffolding y explicar Vue 3 y ASP.NET Core. Reviso cada incremento antes
de incorporarlo y mantendré aquí las partes en las que la IA haya intervenido.

## Pendiente para el cierre

Antes de entregar se documentarán el principal bloqueo y qué mejoraría con una
semana adicional.
