# Revisión manual de interfaz

Levanta el proyecto con `npm run dev` y completa este recorrido antes de enviar
el repositorio. No modifica configuración ni requiere herramientas adicionales.

## Administrador — `admin@norte.test`

1. Inicia sesión con `Sitec.2026`.
2. Confirma que la barra muestra nombre, rol y botón de salida.
3. En `/solicitudes`, prueba estado, prioridad, categoría, SLA y búsqueda.
4. Confirma el texto `Página X de Y — Z resultados` y navega entre páginas.
5. Abre una solicitud `Nueva`: deben aparecer editar, asignar y cancelar.
6. Abre el modal de asignación y confirma que lista administradores y agentes.
7. Crea una solicitud y verifica el mensaje de éxito y su detalle.

## Agente — `agente1@norte.test`

1. En una solicitud `Asignada`, verifica asignar e iniciar.
2. En una `EnProceso`, verifica asignar y resolver.
3. En una `Resuelta`, verifica cerrar y reabrir.
4. Confirma que cancelar no existe en el DOM.

## Solicitante — `user1@norte.test`

1. Confirma que el listado contiene únicamente solicitudes propias.
2. En una `Nueva`, verifica editar y ausencia de acciones de workflow.
3. En una `Resuelta`, verifica que solo cerrar esté disponible.
4. Confirma que una solicitud asignada ya no muestra editar.

## Aislamiento — `user1@sur.test`

1. Inicia sesión y abre directamente una URL con un ID de Cooperativa Norte,
   por ejemplo `40000000-0000-0000-0000-000000000001`.
2. La pantalla debe mostrar recurso no encontrado; nunca datos de Norte.

## Tamaños de pantalla

- Escritorio: tabla y filtros en una fila cuando exista espacio.
- Tableta: filtros en dos columnas y detalle apilado.
- Móvil (320–560 px): filtros en una columna, navegación compacta y tabla con
  desplazamiento horizontal.
- Comprueba que links, inputs y botones puedan recorrerse con `Tab`.

Al terminar ejecuta `npm test`, `npm run build` y `npm run lint`.
