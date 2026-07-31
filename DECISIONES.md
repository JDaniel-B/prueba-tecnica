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
   porque permitiría consultar otra organización manipulando la petición. En
   consultas por ID, un recurso de otro tenant se trata como inexistente (`404`);
   las restricciones de rol dentro del mismo tenant se informan como `403`.
5. El código de una solicitud se obtiene buscando el mayor correlativo del tenant
   y año actuales y sumando uno. El índice único evita duplicados persistidos, pero
   no se agregó bloqueo distribuido ni reintentos porque la concurrencia del
   correlativo está explícitamente fuera del alcance del ejercicio.
6. La entidad `Solicitud` es responsable de aplicar una edición y recalcular el SLA.
   El cálculo siempre parte de `FechaCreacion`, que es inmutable, y solo se repite
   cuando cambia la categoría o prioridad de una solicitud no terminal. Se preserva
   el SLA histórico de solicitudes resueltas, cerradas o canceladas.
7. Las transiciones viven en métodos explícitos de `Solicitud` (`Asignar`,
   `Iniciar`, `Resolver`, etc.). El servicio de aplicación decide permisos y valida
   recursos externos, mientras el dominio impide saltos de estado incluso si se lo
   invoca sin pasar por HTTP. Al reabrir se limpian la fecha y el motivo de la
   resolución porque la solicitud vuelve a estar pendiente.
8. El frontend usa `fetch` mediante un único módulo HTTP, Pinia para la sesión y
   Vue Router para proteger rutas. Los DTO se escribieron en TypeScript estricto y
   las páginas se dividieron por responsabilidad. Se agregó `GET /agentes` como
   endpoint auxiliar para evitar IDs hardcodeados en el selector de asignación.
9. Se eligió Vitest con Vue Test Utils y jsdom para probar el frontend porque se
   integra con Vite y permite comprobar componentes sin mantener una segunda
   configuración de compilación. La decisión de qué acciones mostrar se extrajo a
   una función pura compartida por la vista y sus pruebas, evitando duplicar la
   matriz de permisos en el test.

## Uso de IA

Usé Codex como acompañamiento para analizar el enunciado, proponer la arquitectura,
generar el scaffolding y explicar Vue 3 y ASP.NET Core. Reviso cada incremento antes
de incorporarlo y mantendré aquí las partes en las que la IA haya intervenido.

## Pendiente para el cierre

Antes de entregar se documentarán el principal bloqueo y qué mejoraría con una
semana adicional.
