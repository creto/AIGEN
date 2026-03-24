# 🗺️ AIGEN — Derrotero de Desarrollo v4.2
**Proyecto:** AI-Powered Software Generator (AIGEN)  
**Stack:** .NET Core 8 · Scriban · EF Core · Dapper · Angular 18 · PrimeNG  
**BD Objetivo:** SQL Server · PostgreSQL · Oracle (roadmap)  
**Convención BD:** Línea base de almacenamiento (TM_ / TB_ / TBR_ / TP_ / TR_ / TA_ / TS_ / TH_)  
**Repositorio:** github.com/creto/AIGEN  
**Última actualización:** Marzo 2026 — Semana 11 completada

---

## 📊 Progreso General

```
Semana 1  ████████████████████ 100% ✅  Fundamentos CLI y schema reader
Semana 2  ████████████████████ 100% ✅  Clasificación de tablas y convenciones
Semana 3  ████████████████████ 100% ✅  Motor de plantillas Scriban
Semana 4  ████████████████████ 100% ✅  EF Core + Dapper híbrido
Semana 5  ████████████████████ 100% ✅  Primera generación real Doc4UsAIGen
Semana 6  ████████████████████ 100% ✅  Corrección masiva 1075 → 0 errores
Semana 7  ████████████████████ 100% ✅  Calidad código + docs + Git
Semana 8  ████████████████████ 100% ✅  API funcional + Swagger + DI auto-registro
Semana 9  ████████████████████ 100% ✅  Endpoints CRUD + validación BD real
Semana 10 ████████████████████ 100% ✅  Soft-delete + PostgreSQL + excludedPrefixes
Semana 11 ████████████████████ 100% ✅  Frontend Angular + JWT end-to-end + Naming fixes
Semana 12 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Microservicios + API Gateway (YARP)
Semana 13 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Auditoría de negocio + Logs + Caché
Semana 14 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Stored Procedures + CRUD vía SP
Semana 15 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Menús dinámicos por roles + UX jerárquica
Semana 16 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Notificaciones + Mensajería + SignalR
Semana 17 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Dockerización completa + Nginx
Semana 18 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Testing completo (Unit + Integration + E2E + Load)
Semana 19 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Seguridad + OWASP + Headers + Rate Limiting
Semana 20 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  CI/CD completo (GitHub Actions + SonarCloud + NuGet)
Semana 21 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  IA: AI Assistant + Agente Autónomo + MCP Server
Semana 22 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Multi-frontend + Temas visuales + Logo
Semana 23 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Documentación completa + empaquetado NuGet
```

---

## ✅ SEMANAS 1–11 — COMPLETADAS
> Ver detalle completo en versiones anteriores del derrotero.  
> **Resumen:** 2077 archivos · 0 errores · 0 warnings · JWT end-to-end · PostgreSQL · Soft-delete · Frontend Angular conectado

---

## ⏳ SEMANA 12 — Microservicios + API Gateway
> **Estado: PENDIENTE**

### Feature — Arquitectura de Microservicios
- [ ] `separateSolutionPerService: true` en `aigen.json` — un `.sln` por prefijo de tabla
- [ ] Agrupación automática: `TM_`→`MovementService` · `TP_`→`ParameterService` · `TB_`→`BasicService`
- [ ] `Dockerfile.scriban` — imagen multistage por microservicio
- [ ] `docker-compose.scriban` — orquestación base de todos los microservicios
- [ ] API Gateway con YARP — `gateway.scriban` con rutas hacia cada microservicio
- [ ] Health checks — `/health` y `/ready` por servicio
- [ ] Service discovery — variables de entorno por servicio en compose

**Nuevos archivos:** `solution_microservice.scriban`, `docker_compose.scriban`, `dockerfile.scriban`, `gateway.scriban`  
**Esfuerzo estimado:** 5-7 días

---

## ⏳ SEMANA 13 — Auditoría de Negocio + Logs + Caché
> **Estado: PENDIENTE** — *nuevo — incorporado desde preguntas de arquitectura*

### Feature 1 — Auditoría de Negocio por Campo/Tabla

#### Filosofía: compatibilidad con generador antiguo + modernización
- [ ] `aigen.json` — sección `"auditing"` granular:
  ```json
  "auditing": {
    "provider": "EFInterceptor" | "StoredProcedure" | "Both",
    "globalEnabled": true,
    "tables": {
      "TB_Funcionario": { "enabled": true,  "fields": ["Nombre","Email","Cargo"] },
      "TM_Documento":   { "enabled": true,  "fields": "*" },
      "TP_Ciudad":      { "enabled": false }
    }
  }
  ```
- [ ] **Provider `EFInterceptor`** (modo moderno):
  - `audit_interceptor.scriban` — `AuditSaveChangesInterceptor : SaveChangesInterceptor`
  - Captura `Added`, `Modified`, `Deleted` en `SaveChangesAsync`
  - Compara valores anteriores vs nuevos campo por campo
  - Escribe en tabla `TA_AuditoriaTransacciones` con: entidad, campo, valor_anterior, valor_nuevo, usuario, fecha, IP
  - Filtro por campos configurados en `auditing.tables`
  - Claims del JWT como fuente de `usuario` e IP
- [ ] **Provider `StoredProcedure`** (compatibilidad con generador antiguo):
  - `sp_audit.scriban` — genera SP `sp_Audit_{TableName}` igual que el generador anterior
  - SP recibe: tabla, campo, valor_anterior, valor_nuevo, id_registro, id_usuario
  - Inserta en tabla de auditoría configurada
  - `repository_ef_audit.scriban` — llama al SP desde el repositorio EF
- [ ] **Provider `Both`** — interceptor EF + SP para migración gradual
- [ ] `audit_table.scriban` — script SQL para crear `TA_AuditoriaTransacciones` si no existe
- [ ] `audit_query_service.scriban` — servicio de consulta de auditoría:
  - `GetAuditTrailAsync(tableName, recordId)` — historial completo de un registro
  - `GetAuditByUserAsync(userId, dateRange)` — auditoría por usuario
  - `GetAuditByDateAsync(dateRange)` — auditoría por período
- [ ] `audit_controller.scriban` — endpoints REST de consulta de auditoría (solo rol Admin)
- [ ] Angular: `audit-trail.component.scriban` — timeline visual de cambios por registro con PrimeNG `p-timeline`

#### Logging técnico (Serilog estructurado)
- [ ] `logging_config.scriban` — `appsettings.json` con Serilog sinks:
  - Sink `Console` (desarrollo)
  - Sink `File` con rotación diaria (producción)
  - Sink `Seq` opcional para dashboards
  - Sink `ApplicationInsights` opcional (Azure)
- [ ] Enriquecer logs con: `UserId`, `RequestId`, `Environment`, `MachineName`
- [ ] Logging de eventos de autenticación: login · logout · token refresh · acceso denegado
- [ ] Logging de operaciones CRUD: entidad + ID + usuario + duración

### Feature 2 — Caché
- [ ] `aigen.json` — configuración de caché:
  ```json
  "features": {
    "cache": "None" | "InMemory" | "Redis",
    "cacheConnectionString": "localhost:6379",
    "cacheDefaultExpirySeconds": 300,
    "cacheByTable": {
      "TP_Ciudad":      { "enabled": true,  "expirySeconds": 3600 },
      "TP_TipoDocumento": { "enabled": true, "expirySeconds": 7200 },
      "TM_Documento":   { "enabled": false }
    }
  }
  ```
- [ ] **InMemory:** `IMemoryCache` .NET built-in — `cache_service_memory.scriban`
- [ ] **Redis:** `IDistributedCache` + `StackExchange.Redis` — `cache_service_redis.scriban`
- [ ] `cache_decorator.scriban` — decorator pattern sobre repositorios con caché automático:
  - `GetByIdAsync` → cache hit/miss con key `{Entity}:{id}`
  - `GetPagedAsync` — cache de primera página de tablas TP_* por 1h
  - Invalidación automática en `CreateAsync`, `UpdateAsync`, `DeleteAsync`
- [ ] Cache de menús por rol — `GetMenusByRoleAsync` cacheado (ver S15)
- [ ] Cache de respuestas IA — mismo prompt → misma respuesta sin llamar al modelo (ver S21)
- [ ] `cache_health.scriban` — healthcheck de Redis en `/ready` endpoint

**Nuevos archivos:** `audit_interceptor.scriban`, `sp_audit.scriban`, `audit_query_service.scriban`, `audit_controller.scriban`, `audit_trail.component.scriban`, `cache_service_memory.scriban`, `cache_service_redis.scriban`, `cache_decorator.scriban`  
**Esfuerzo estimado:** 6-8 días

---

## ⏳ SEMANA 14 — Stored Procedures + CRUD vía SP
> **Estado: PENDIENTE**

### Feature — CRUD vía Stored Procedures (compatibilidad generador antiguo)

#### Lectura de SPs desde la BD
- [ ] `SqlServerSchemaReader` — leer `INFORMATION_SCHEMA.ROUTINES` (SPs + funciones escalares/tabla)
- [ ] `PostgreSqlSchemaReader` — leer funciones PL/pgSQL
- [ ] `StoredProcedureMetadata` — nombre, parámetros IN/OUT, tipo de retorno, schema

#### Estrategia configurable
- [ ] `aigen.json`: `"crudStrategy": "direct" | "storedProcedures" | "mixed"`
  - `"direct"` (default) — EF Core + Dapper sin SPs
  - `"storedProcedures"` — genera SPs para todas las tablas (igual generador antiguo)
  - `"mixed"` — SPs solo en tablas marcadas con `"useSP": true`

#### Plantillas de SPs generados
- [ ] `sp_crud.scriban` — T-SQL completo por tabla:
  - `sp_Insert_{TableName}` — INSERT + retorna ID generado
  - `sp_Update_{TableName}` — UPDATE con parámetros opcionales (solo campos enviados)
  - `sp_Delete_{TableName}` — DELETE físico o soft-delete según config
  - `sp_GetById_{TableName}` — SELECT por PK
  - `sp_GetPaged_{TableName}` — SELECT paginado con filtros
  - `sp_ToggleEstado_{TableName}` — si tabla tiene campo Estado
- [ ] `sp_crud_postgres.scriban` — PL/pgSQL equivalente para PostgreSQL
- [ ] `repository_sp.scriban` — repositorio Dapper que ejecuta los SPs:
  - `DynamicParameters` de Dapper para mapear parámetros
  - Manejo de OUTPUT parameters para IDs
- [ ] `sp_audit.scriban` — SP de auditoría por tabla (ver S13) integrado aquí

#### CLI y scripts
- [ ] CLI: `aigen generate-sp-script --db SqlServer|Postgres` — genera script `.sql` sin código C#
- [ ] Output: script ordenado con dependencias (SPs de TP_ primero, luego TB_, luego TM_)
- [ ] Script de despliegue: `DROP IF EXISTS` + `CREATE` para idempotencia

**Nuevos archivos:** `StoredProcedureMetadata.cs`, `sp_crud.scriban`, `sp_crud_postgres.scriban`, `repository_sp.scriban`  
**Esfuerzo estimado:** 4-5 días

---

## ⏳ SEMANA 15 — Menús Dinámicos por Roles + UX Jerárquica
> **Estado: PENDIENTE** — *nuevo — incorporado desde preguntas de arquitectura*

### Feature — Sistema de Menús Dinámicos con Gestión por Roles

#### Modelo de datos (tablas esperadas en la BD)
```
TS_Menu          — id, nombre, icono, ruta, orden, id_padre (FK self), id_modulo, activo
TS_Permiso       — id, nombre, codigo, descripcion (ej: "documentos.crear")
TR_RolMenu       — id_rol, id_menu, activo
TR_RolPermiso    — id_rol, id_permiso, activo
TS_Modulo        — id, nombre, icono, orden, activo
```

#### Backend generado
- [ ] `menu_service.scriban` — `MenuService` con:
  - `GetMenusByRoleAsync(roles[])` — árbol jerárquico filtrado por roles del JWT
  - `GetAllMenusAsync()` — árbol completo para panel de administración
  - `UpdateMenuOrderAsync()` — reordenamiento drag & drop
  - `ToggleMenuRoleAsync(menuId, roleId)` — activar/desactivar menú por rol
- [ ] `menu_controller.scriban` — endpoints:
  - `GET /api/menus/by-role` — menús del usuario autenticado (usa claims del JWT)
  - `GET /api/menus/admin` — todos los menús (solo rol Admin)
  - `PUT /api/menus/{id}/order` — reordenar
  - `POST /api/menus/{id}/roles/{roleId}` — asignar menú a rol
- [ ] `menu_seed.scriban` — `MenuSeed.sql`: genera ítems de menú iniciales automáticamente a partir de los controllers generados por AIGEN (1 controller = 1 ítem de menú)
- [ ] Cache de menús: `GetMenusByRoleAsync` cacheado por rol (invalida al cambiar permisos)

#### Frontend generado
- [ ] `angular_menu_service.scriban` — `MenuService` Angular:
  - `loadMenus()` — llama a `/api/menus/by-role` al login
  - Almacena árbol en Signal `menuTree`
  - Construye rutas dinámicas en `app.routes.ts` desde la respuesta
- [ ] `angular_sidebar.scriban` — `SidebarComponent` con PrimeNG `p-panelMenu`:
  - Renderiza árbol desde Signal `menuTree`
  - Íconos PrimeIcons configurables por ítem
  - Colapso/expansión por módulo
  - Highlight del ítem activo
  - Responsive: drawer en móvil, sidebar fijo en desktop
- [ ] `angular_topbar.scriban` — `TopbarComponent`:
  - Nombre del usuario + avatar
  - Notificaciones badge (conecta con S16)
  - Botón de logout
  - Breadcrumb dinámico
- [ ] `angular_menu_admin.scriban` — Panel de administración de menús:
  - `p-tree` con drag & drop para reordenar menús
  - Toggle por rol con `p-toggleButton`
  - Vista previa del menú resultante por rol
  - Búsqueda de permisos
- [ ] `angular_forbidden.scriban` — `ForbiddenComponent` (403) con mensaje y redirección
- [ ] `angular_not_found.scriban` — `NotFoundComponent` (404)

#### AuthGuard mejorado con permisos granulares
- [ ] `angular_permission_guard.scriban` — `permissionGuard(permiso: string)`:
  - Verifica si el usuario tiene el permiso específico (no solo el rol)
  - Redirige a `/forbidden` si no tiene acceso
  - Uso: `canActivate: [permissionGuard('documentos.crear')]`
- [ ] `angular_permission_directive.scriban` — directiva `*hasPermission="'documentos.eliminar'"`:
  - Oculta/muestra botones según permisos del usuario
  - Uso en templates: `<p-button *hasPermission="'documentos.eliminar'" .../>`

**Nuevos archivos:** `menu_service.scriban`, `menu_controller.scriban`, `menu_seed.scriban`, `angular_menu_service.scriban`, `angular_sidebar.scriban`, `angular_topbar.scriban`, `angular_menu_admin.scriban`, `angular_permission_guard.scriban`, `angular_permission_directive.scriban`  
**Esfuerzo estimado:** 7-9 días

---

## ⏳ SEMANA 16 — Notificaciones + Mensajería + SignalR
> **Estado: PENDIENTE** — *nuevo — incorporado desde preguntas de arquitectura*

### Feature — Servicios de Notificación y Mensajería

#### Notificaciones en tiempo real — SignalR
- [ ] `signalr_hub.scriban` — `NotificationHub : Hub`:
  - Grupos por usuario y por rol
  - Método `SendToUser(userId, notification)`
  - Método `SendToRole(role, notification)`
  - Método `Broadcast(notification)`
- [ ] `signalr_service.scriban` — `NotificationService` inyectable:
  - `NotifyAsync(userId, title, message, type, link?)` — notificación puntual
  - `NotifyRoleAsync(role, ...)` — notificación a todos los usuarios con un rol
  - Persistencia en `TM_Notificacion` (ya existe en Doc4Us)
- [ ] `program.scriban` — agregar `AddSignalR()` + `MapHub<NotificationHub>("/hubs/notifications")`
- [ ] `angular_notification_service.scriban` — servicio Angular con `@microsoft/signalr`:
  - Conexión automática al hub al login
  - Reconexión automática con backoff exponencial
  - Signal `notifications` con lista de notificaciones no leídas
  - Badge contador en topbar
- [ ] `angular_notification_panel.scriban` — `p-overlayPanel` con lista de notificaciones:
  - Ítem por notificación con tipo (info/warning/error/success)
  - Marcar como leída al hacer click
  - Marcar todas como leídas
  - Navegación a la entidad relacionada

#### Email — SMTP
- [ ] `email_service.scriban` — `EmailService` con MimeKit:
  - `SendAsync(to, subject, htmlBody, attachments?)`
  - Configuración desde `appsettings.json` (`TM_Empresa_Smtp` o sección `Email`)
  - Templates de email en Scriban: bienvenida, reset password, notificación
  - Cola de envío asíncrona con `Channel<T>` para no bloquear request
- [ ] `email_template_welcome.scriban` — template HTML bienvenida
- [ ] `email_template_reset.scriban` — template HTML reset de contraseña
- [ ] `email_template_notification.scriban` — template HTML notificación genérica

#### Cola de mensajes — opcional (configurable)
- [ ] `aigen.json`: `"messaging": { "provider": "None" | "InMemory" | "RabbitMQ" | "AzureServiceBus" }`
- [ ] **InMemory:** `Channel<T>` .NET — para procesamiento asíncrono en un solo proceso
- [ ] **RabbitMQ:** `MassTransit` + RabbitMQ — para microservicios (se activa en S12)
- [ ] **Azure Service Bus:** `MassTransit` + Azure SB — para deployments en Azure (S20)
- [ ] `message_publisher.scriban` — `IMessagePublisher` con implementaciones por provider
- [ ] `message_consumer.scriban` — consumidores base para eventos de negocio

#### SMS — opcional
- [ ] `aigen.json`: `"sms": { "provider": "None" | "Twilio" | "Infobip" }`
- [ ] `sms_service.scriban` — `ISmsService` con implementaciones Twilio e Infobip
- [ ] Uso: verificación 2FA (conecta con `SecurityConfig.TwoFactor`)

#### Webhooks — opcional
- [ ] `webhook_service.scriban` — `WebhookService`:
  - Registro de endpoints externos
  - Envío de eventos de negocio (documento creado, estado cambiado)
  - Retry con backoff exponencial
  - Verificación HMAC de payload

**Nuevos archivos:** `signalr_hub.scriban`, `signalr_service.scriban`, `email_service.scriban`, `angular_notification_service.scriban`, `angular_notification_panel.scriban`, `message_publisher.scriban`, `message_consumer.scriban`, `sms_service.scriban`, `webhook_service.scriban`  
**Esfuerzo estimado:** 6-8 días

---

## ⏳ SEMANA 17 — Dockerización Completa
> **Estado: PENDIENTE**

### Feature — Containerización de proyectos generados

#### Dockerfile por proyecto
- [ ] `dockerfile_api.scriban` — multistage: SDK build → aspnet runtime · usuario no-root
- [ ] `dockerfile_frontend.scriban` — multistage: node build → nginx:alpine
- [ ] `nginx.conf.scriban` — reverse proxy: SPA routing + proxy `/api` + gzip + cache headers + security headers

#### docker-compose
- [ ] `docker_compose.scriban` — producción: api + frontend + db + redis (si cache=Redis) + rabbitmq (si messaging=RabbitMQ)
- [ ] `docker_compose_override.scriban` — desarrollo: volúmenes, hot reload, puertos de debug
- [ ] `docker_compose_prod.scriban` — producción: secrets externos, límites recursos, logging drivers

#### Health checks y utilidades
- [ ] `healthcheck.scriban` — `/health` (liveness) + `/ready` (readiness con BD + Redis)
- [ ] `wait_for_it.scriban` — script para esperar BD antes de arrancar API
- [ ] `.dockerignore.scriban` · `.env.example.scriban` · `docker_secrets.scriban`

**Esfuerzo estimado:** 5-6 días

---

## ⏳ SEMANA 18 — Testing Completo
> **Estado: PENDIENTE**

### Feature — Suite de Tests Generados Automáticamente

#### Tests unitarios — xUnit + Moq
- [ ] `test_service_unit.scriban` — por cada Service: GetPaged, GetById, Create, Update, Delete, ToggleEstado
- [ ] `test_audit_unit.scriban` — tests del interceptor de auditoría: verifica captura de cambios
- [ ] `test_menu_unit.scriban` — tests de MenuService: árbol jerárquico, filtro por rol
- [ ] `test_naming_convention.scriban` — casos kebab, PascalCase, plural español

#### Tests de integración — WebApplicationFactory + TestContainers
- [ ] `test_integration_controller.scriban` — por controller: GET 200 · POST 201 · PUT 204 · DELETE 204 · 401 sin token · 403 sin rol
- [ ] `test_integration_auth.scriban` — flujo completo: login → token → request autenticado → refresh → logout
- [ ] `test_integration_audit.scriban` — verifica que operaciones CRUD generan registros de auditoría
- [ ] `test_webfactory.scriban` — `CustomWebApplicationFactory` con TestContainers SQL Server/PostgreSQL
- [ ] `test_integration_signalr.scriban` — tests de hub: conexión, envío, recepción de notificaciones

#### Tests E2E — Playwright
- [ ] `playwright_config.scriban` — configuración multi-browser (Chromium, Firefox, WebKit)
- [ ] `test_e2e_login.scriban` — flujo de autenticación completo
- [ ] `test_e2e_crud.scriban` — crear, editar, eliminar registro por entidad
- [ ] `test_e2e_menu.scriban` — verificar menús visibles según rol
- [ ] `test_e2e_notification.scriban` — recibir notificación en tiempo real

#### Tests de carga — k6
- [ ] `test_load_smoke.scriban` — 1 usuario, 1 min (verificar arranque)
- [ ] `test_load_average.scriban` — 10-50 usuarios (carga normal)
- [ ] `test_load_stress.scriban` — ramp-up hasta 200 usuarios (punto de quiebre)
- [ ] `test_load_spike.scriban` — 0→500 en 10s (picos súbitos)
- [ ] `test_load_soak.scriban` — 50 usuarios, 2 horas (memory leaks)
- [ ] Thresholds: `p95 < 500ms` · `error_rate < 1%` · `rps > 100`

#### Tests de contrato
- [ ] `test_contract_openapi.scriban` — valida que API cumple su contrato OpenAPI (todos los endpoints responden con schemas correctos)

**Esfuerzo estimado:** 7-9 días

---

## ⏳ SEMANA 19 — Seguridad + OWASP + Hardening
> **Estado: PENDIENTE**

### Feature — Seguridad en código generado y pipeline

#### Headers HTTP + Rate Limiting
- [ ] `security_middleware.scriban` — `SecurityHeadersMiddleware`: CSP · HSTS · X-Frame-Options · X-Content-Type-Options · Referrer-Policy · Permissions-Policy
- [ ] `nginx_security.scriban` — headers de seguridad en Nginx para el frontend
- [ ] `rate_limiting.scriban` — Rate Limiting .NET 8 built-in:
  - Policy `"fixed"` 100 req/min (general) · Policy `"auth"` 5 req/min (login) · Policy `"strict"` 10 req/min (sensibles)
  - Response 429 con `Retry-After`

#### OWASP Top 10 — Mitigaciones en código generado
- [ ] A01 Broken Access Control — `[Authorize]` en todos los controllers
- [ ] A02 Cryptographic Failures — JWT HS256 32+ chars · BCrypt/Argon2 para passwords · HttpOnly cookies
- [ ] A03 Injection — EF Core parameterizado · Dapper `@param` · sin concatenación en queries
- [ ] A05 Security Misconfiguration — CORS restrictivo · HTTPS forzado · Swagger solo Development
- [ ] A07 Auth Failures — Rate limit en login · Token expiry · Refresh token rotation
- [ ] A09 Logging Failures — Serilog structured · Sin PII en logs · Audit trail completo

#### OWASP para IA — LLM Top 10 (aplica al módulo IA de S21)
- [ ] LLM01 Prompt Injection — sanitización inputs · system prompt inmutable por usuario
- [ ] LLM02 Insecure Output — validar/sanitizar respuestas antes de ejecutar código
- [ ] LLM06 Sensitive Disclosure — sin connection strings/keys/PII en prompts · filtro automático
- [ ] LLM08 Excessive Agency — permisos mínimos · sin auto-ejecución de código sin revisión humana
- [ ] LLM09 Overreliance — `// AI-generated — review required` en código generado por agente
- [ ] Guardrails en `aigen.json`: `ai.maxPromptLength` · `ai.allowCodeExecution` · `ai.sanitizeOutputs` · `ai.auditLog`

#### Análisis estático y de dependencias
- [ ] `semgrep_config.scriban` — `.semgrep.yml` OWASP .NET + Angular
- [ ] `gitleaks_config.scriban` — `.gitleaks.toml` + `.pre-commit-config.yaml`
- [ ] `owasp_dependency_check.scriban` — `dotnet list package --vulnerable` + `npm audit`

#### DAST — OWASP ZAP
- [ ] `zap_config.scriban` — `zap-baseline.yml` contra API y frontend
- [ ] `zap_auth.scriban` — script login → token → scans autenticados

**Esfuerzo estimado:** 6-8 días

---

## ⏳ SEMANA 20 — CI/CD Completo
> **Estado: PENDIENTE**

### Feature — Pipeline GitHub Actions end-to-end

#### Pipeline principal — `ci.yml`
- [ ] Jobs encadenados: `build → unit-tests → integration-tests → e2e-tests → load-tests → security-scan → docker-build → deploy`
- [ ] Cache NuGet + node_modules · Coverage Coverlet → Codecov · Artifacts de test

#### Pipeline de seguridad — `security.yml`
- [ ] Secrets scan (gitleaks) · SAST (semgrep) · Dependency check NuGet + npm · OWASP ZAP · Resumen en PR comment

#### SonarCloud — `sonar.yml`
- [ ] Quality Gate: coverage ≥ 70% · duplicación < 3% · deuda técnica < 1h · bloquea PR si no pasa

#### Pipeline de release — `release.yml`
- [ ] Build + test + docker push (ghcr.io) + NuGet publish + GitHub Release automático con changelog

#### Deploy
- [ ] `github_actions_deploy_ssh.scriban` — `docker-compose pull && up -d` + healthcheck + rollback
- [ ] `github_actions_deploy_azure.scriban` — Azure App Service o AKS + Key Vault + zero-downtime slot swap

#### Archivos de repo
- [ ] `dependabot.yml` · `pull_request_template.md` · `CODEOWNERS` · `.editorconfig`

**Esfuerzo estimado:** 6-8 días

---

## ⏳ SEMANA 21 — IA: AI Assistant + Agente Autónomo + MCP Server
> **Estado: PENDIENTE** — *expandido con agente autónomo y MCP*

### Feature 1 — AI Assistant (conversacional)
- [ ] `Aigen.AI.csproj` con SDK Anthropic .NET
- [ ] Modelos: Claude API (primario) · OpenAI GPT-4 (fallback) · Ollama local (offline)
- [ ] `aigen ai-assist` — modo conversacional con contexto del proyecto actual
- [ ] `aigen ai-review [archivo]` — revisión de calidad + seguridad (OWASP + buenas prácticas)
- [ ] `aigen ai-explain [entidad]` — explicación de la entidad en lenguaje natural
- [ ] RAG sobre código generado: Qdrant indexa entidades → respuestas con contexto semántico real
- [ ] Cache de respuestas IA — mismo prompt + contexto → respuesta cacheada (Redis si disponible)
- [ ] Historial de conversaciones por proyecto en SQLite local
- [ ] Guardrails OWASP LLM: sanitización · sin PII · audit log de consultas

### Feature 2 — Agente Autónomo AIGEN
- [ ] Distinción clara: Assistant = conversacional (humano lidera) · Agente = autónomo (IA lidera)
- [ ] `AigenAgent.cs` — agente con herramientas:
  - `read_schema` — lee el schema de la BD configurada
  - `generate_template` — ejecuta una plantilla Scriban específica
  - `run_build` — ejecuta `dotnet build` y retorna errores
  - `run_tests` — ejecuta suite de tests y retorna resultados
  - `analyze_security` — ejecuta Semgrep y retorna hallazgos
  - `open_pr` — crea PR en GitHub con los cambios generados
- [ ] Casos de uso del agente:
  - "Analiza la BD, detecta tablas sin auditoría, genera SPs faltantes y abre un PR"
  - "Encuentra todos los controllers sin `[Authorize]` y corrígelos"
  - "Genera tests unitarios para las entidades que no tienen cobertura"
  - "Detecta vulnerabilidades en el código generado y propone fixes"
- [ ] `aigen agent-run "[instrucción en lenguaje natural]"` — modo agente CLI
- [ ] Confirmación humana antes de ejecutar acciones destructivas (open_pr, run_build)
- [ ] Límites de seguridad OWASP LLM08: sin ejecución de código arbitrario sin aprobación

### Feature 3 — MCP Server AIGEN
- [ ] `Aigen.MCP.csproj` — servidor MCP que expone AIGEN como herramienta para Claude Desktop, Cursor, VS Code
- [ ] Herramientas MCP expuestas:
  - `aigen_read_schema(connectionString, db)` — lee y retorna schema de BD
  - `aigen_generate(tableName, templates[])` — genera código para una tabla
  - `aigen_get_generated_files(path)` — lista archivos generados
  - `aigen_build(solutionPath)` — compila y retorna errores
  - `aigen_get_audit_config()` — retorna configuración de auditoría actual
- [ ] Transporte: stdio (para Claude Desktop) + SSE (para uso web)
- [ ] Configuración en `claude_desktop_config.json`: `{ "aigen": { "command": "aigen", "args": ["mcp-server"] } }`

### Feature 4 — Conectores MCP externos en aplicaciones generadas
- [ ] `aigen.json`: `"mcpConnectors": ["github", "jira", "slack", "gdrive"]`
- [ ] `mcp_connector_github.scriban` — integración GitHub: crear issues, PRs, comentarios desde la app
- [ ] `mcp_connector_jira.scriban` — integración Jira: crear tickets, actualizar estados
- [ ] `mcp_connector_slack.scriban` — envío de notificaciones a canales Slack
- [ ] `mcp_client_service.scriban` — `IMcpClientService` genérico con implementaciones por conector

**Nuevos archivos:** `Aigen.AI.csproj`, `AigenAssistant.cs`, `AigenAgent.cs`, `Aigen.MCP.csproj`, `mcp_connector_github.scriban`, `mcp_connector_jira.scriban`, `mcp_connector_slack.scriban`  
**Esfuerzo estimado:** 10-14 días

---

## ⏳ SEMANA 22 — Multi-Frontend + Temas Visuales + Logo
> **Estado: PENDIENTE**

### Feature — Multi-Framework
- [ ] `frontend.framework: "angular" | "react" | "vue" | "blazor"` en `aigen.json`
- [ ] React: hooks + Axios + Ant Design — `ListPage.tsx`, `FormPage.tsx`, `AuthService.ts`, sidebar dinámico
- [ ] Vue 3: Composition API + Axios + PrimeVue — `ListView.vue`, `FormView.vue`
- [ ] Blazor: .razor + MudBlazor — `ListPage.razor`, `FormPage.razor`

### Feature — Temas Visuales Seleccionables
- [ ] `aigen.json`: `"theme": "lara-light" | "lara-dark" | "material" | "bootstrap" | "custom"`
- [ ] Angular: `styles.scss` con variables CSS PrimeNG del tema seleccionado
- [ ] `primaryColor`, `secondaryColor`, `accentColor`, `darkMode` en `aigen.json`
- [ ] Preview del tema en el CLI antes de generar: ASCII art de colores seleccionados
- [ ] Validación contraste WCAG AA automática

### Feature — Logo e Imagen de Marca
- [ ] `logoPath` en `aigen.json` — PNG/SVG/JPG/ICO
- [ ] Copiar a `assets/` + inyectar en navbar, sidebar, login y splash screen
- [ ] Generar `favicon.ico` automáticamente desde PNG

**Esfuerzo estimado:** ~10 días

---

## ⏳ SEMANA 23 — Documentación Completa + Empaquetado
> **Estado: PENDIENTE**

### Feature — Documentación Auto-Generada
- [ ] `ARCHITECTURE.md` — diagrama ASCII de capas + entidades por dominio
- [ ] `DEPLOYMENT.md` — Docker + BD + variables de entorno + SSL
- [ ] `SECURITY.md` — política de seguridad + vulnerabilidades conocidas + contacto
- [ ] `CODE_SUMMARY.md` — estadísticas: tablas, CRUD, tests, cobertura
- [ ] `CHANGELOG.md` — diff entre generaciones + Conventional Commits
- [ ] `api_catalog.scriban` — catálogo de endpoints con ejemplos request/response + auth
- [ ] `db_dictionary.scriban` — diccionario de datos completo
- [ ] CLI: `aigen generate-docs --format pdf|html|md`

### Empaquetado final
- [ ] `dotnet tool install -g aigen` — herramienta global .NET
- [ ] Pruebas contra 3+ BDs de clientes reales
- [ ] NuGet publish con versionado semántico

**Esfuerzo estimado:** 5-6 días

---

## 🚀 Features — Resumen Completo y Roadmap

| # | Feature | Semana | Impacto | Estado |
|---|---------|--------|---------|--------|
| 1-11 | Semanas 1-11 completadas | S1-S11 | — | ✅ |
| 12 | Microservicios + YARP Gateway | S12 | Alto | ⏳ |
| 13 | Auditoría negocio por campo/tabla (EF + SP) | S13 | Alto | ⏳ |
| 14 | Caché InMemory + Redis por tabla | S13 | Alto | ⏳ |
| 15 | Logging estructurado Serilog | S13 | Medio | ⏳ |
| 16 | Stored Procedures CRUD (compatibilidad) | S14 | Medio | ⏳ |
| 17 | Menús dinámicos por roles + jerarquía | S15 | Alto | ⏳ |
| 18 | Panel admin de menús con drag & drop | S15 | Alto | ⏳ |
| 19 | Permiso granular + directiva Angular | S15 | Alto | ⏳ |
| 20 | NotificacionesSignalR tiempo real | S16 | Alto | ⏳ |
| 21 | Email service + templates | S16 | Medio | ⏳ |
| 22 | Mensajería asíncrona RabbitMQ/Azure SB | S16 | Medio | ⏳ |
| 23 | SMS Twilio/Infobip | S16 | Bajo | ⏳ |
| 24 | Webhooks salientes | S16 | Bajo | ⏳ |
| 25 | Dockerización completa + Nginx | S17 | Alto | ⏳ |
| 26 | Testing Unit + Integration + E2E + Load | S18 | Alto | ⏳ |
| 27 | Seguridad OWASP + Headers + Rate Limit | S19 | Alto | ⏳ |
| 28 | OWASP LLM Top 10 para módulo IA | S19 | Alto | ⏳ |
| 29 | CI/CD GitHub Actions completo | S20 | Alto | ⏳ |
| 30 | SonarCloud + Quality Gate | S20 | Medio | ⏳ |
| 31 | Deploy SSH + Azure automático | S20 | Alto | ⏳ |
| 32 | AI Assistant conversacional + RAG | S21 | Alto | ⏳ |
| 33 | Agente autónomo AIGEN | S21 | Alto | ⏳ |
| 34 | MCP Server AIGEN (Claude Desktop/Cursor) | S21 | Alto | ⏳ |
| 35 | Conectores MCP externos (GitHub/Jira/Slack) | S21 | Medio | ⏳ |
| 36 | Multi-frontend React/Vue/Blazor | S22 | Alto | ⏳ |
| 37 | Temas visuales seleccionables | S22 | Medio | ⏳ |
| 38 | Logo + favicon automático | S22 | Bajo | ⏳ |
| 39 | Docs completa + NuGet package | S23 | Alto | ⏳ |

---

## 🐛 Backlog Técnico

| # | Archivo | Descripción | Impacto | Estado |
|---|---------|-------------|---------|--------|
| 1-9 | Varios | Resueltos en S1-S10 | — | ✅ |
| 10 | `NamingConventionService` | KebabName acrónimos | Bajo | ✅ S11 |
| 11 | `FileGeneratorService` | Duplicados ClassNamePlural | Bajo | ✅ S11 |
| 12 | `FileGeneratorService` | TA_* generaban frontend | Medio | ✅ S11 |
| 13 | `NamingConventionService` | EXpedientes → e-xpedientes | Bajo | ✅ S11 |
| 14 | `program.scriban` | Swagger solo Development | Bajo | ✅ S11 |
| 15 | `api_csproj.scriban` | Faltaban paquetes JWT | Alto | ✅ S11 |
| 16 | `FeaturesConfig` | Cache solo como flag, sin implementación | Alto | ⏳ S13 |
| 17 | `program.scriban` | Sin middleware de auditoría de negocio | Alto | ⏳ S13 |
| 18 | `app.routes.ts` | Rutas estáticas — no dinámicas por rol | Alto | ⏳ S15 |
| 19 | `SecurityConfig` | TwoFactor definido pero no implementado | Medio | ⏳ S16 |
| 20 | `angular_app_config.scriban` | Sin registro de SignalR | Medio | ⏳ S16 |

---

## 🔐 Modelo de Seguridad AIGEN — Capas de Protección

```
┌─────────────────────────────────────────────────────────────┐
│  PIPELINE CI/CD (S20)                                       │
│  gitleaks · semgrep · OWASP dep-check · ZAP DAST · Sonar  │
├─────────────────────────────────────────────────────────────┤
│  INFRAESTRUCTURA (S17)                                      │
│  Nginx headers · Docker non-root · Secrets externos        │
├─────────────────────────────────────────────────────────────┤
│  API GENERADA (S19)                                         │
│  Rate limiting · CORS · HTTPS · CSP · HSTS · X-Frame      │
├─────────────────────────────────────────────────────────────┤
│  AUTORIZACIÓN (S15)                                         │
│  Menús por rol · Permisos granulares · AuthGuard           │
├─────────────────────────────────────────────────────────────┤
│  AUTENTICACIÓN (S11)                                        │
│  JWT · Refresh HttpOnly · BCrypt · OWASP A01-A09           │
├─────────────────────────────────────────────────────────────┤
│  AUDITORÍA (S13)                                            │
│  EF Interceptor · SP Auditoría · por campo/tabla           │
├─────────────────────────────────────────────────────────────┤
│  IA (S21)                                                   │
│  OWASP LLM Top 10 · Guardrails · Audit log · No PII       │
└─────────────────────────────────────────────────────────────┘
```

### OWASP Top 10 × Semana de implementación
| OWASP | Vulnerabilidad | Semana | Mitigación generada |
|-------|---------------|--------|---------------------|
| A01 | Broken Access Control | S11+S15+S19 | `[Authorize]` · Menús por rol · Rate limiting |
| A02 | Cryptographic Failures | S11 | JWT HS256 · BCrypt · HttpOnly cookies |
| A03 | Injection | S4+S19 | EF parameterizado · Dapper `@param` |
| A04 | Insecure Design | S19 | Security middleware · CSP · HSTS |
| A05 | Security Misconfiguration | S11+S19 | CORS restrictivo · Swagger solo dev |
| A06 | Vulnerable Components | S20 | `dotnet audit` + `npm audit` en CI |
| A07 | Auth Failures | S11+S19 | Rate limit login · Token expiry · HttpOnly |
| A08 | Software Integrity | S20 | Dependency check · Supply chain |
| A09 | Logging Failures | S7+S13 | Serilog structured · Auditoría de negocio |
| A10 | SSRF | S19 | Validación URLs · Whitelist dominios |

### OWASP LLM Top 10 (S21)
| LLM | Vulnerabilidad | Guardrail en AIGEN |
|-----|---------------|---------------------|
| LLM01 | Prompt Injection | Sanitización inputs · system prompt inmutable |
| LLM02 | Insecure Output | Validar/sanitizar antes de ejecutar |
| LLM06 | Sensitive Disclosure | Sin conn strings/keys/PII en prompts |
| LLM08 | Excessive Agency | Confirmación humana antes de acciones |
| LLM09 | Overreliance | `// AI-generated — review required` |

---

## 🔑 Decisiones Arquitectónicas Tomadas

| Decisión | Justificación |
|----------|---------------|
| Scriban como motor de plantillas | Sintaxis limpia, soporte .NET nativo |
| Prefijos de tabla como señales de generación | Estándar Línea base de almacenamiento |
| Clean Architecture por defecto | Separación de responsabilidades, testabilidad |
| EF Core + Dapper híbrido | EF para CRUD simple, Dapper para queries complejas |
| `ISchemaReader` como interfaz | Permite PostgreSQL/Oracle sin cambiar el generador |
| Soft-delete árbol de decisión | `Eliminado` → flag; `Estado` → estado; ninguno → físico |
| JWT parametrizable por `jwtSource` | `DatabaseTable` / `Hardcoded` / `OIDC` |
| Refresh token en cookie HttpOnly | Más seguro que localStorage |
| Angular Signals para estado de auth | Reactividad moderna Angular 17+ |
| `authGuard` + `roleGuard` + `permissionGuard` | Granularidad: autenticación · rol · permiso específico |
| Menús dinámicos desde API | Rutas Angular generadas en runtime según rol — no hardcodeadas |
| `MenuSeed.sql` auto-generado | Cada controller AIGEN = ítem de menú inicial automático |
| Auditoría por campo/tabla configurable | Compatibilidad con generador antiguo + flexibilidad nueva |
| EF Interceptor como provider de auditoría | Sin SPs extra, captura automática en SaveChanges |
| Caché por tabla en `aigen.json` | Tablas TP_* (parámetros) ideales para cache largo · TM_* no |
| SignalR para notificaciones real-time | Nativo .NET, escalable con Redis backplane en microservicios |
| MassTransit para mensajería | Abstracción sobre RabbitMQ/Azure SB — cambiar provider sin código |
| `crudStrategy` configurable | `direct` (default) · `storedProcedures` · `mixed` |
| Docker multistage build | Imagen final mínima — reduce superficie de ataque |
| k6 para load testing | JS scripting, CI-native, métricas p95/p99 |
| Playwright para E2E | Multi-browser, mejor integración CI que Cypress |
| TestContainers para integración | BD real en Docker durante tests — sin mocks de BD |
| Rate limiting .NET 8 built-in | Sin dependencias externas |
| Semgrep para SAST | Reglas OWASP .NET + Angular, pocos falsos positivos |
| OWASP LLM guardrails en agente | AIGEN genera código — responsabilidad de no amplificar vulnerabilidades |
| MCP Server AIGEN | Expone AIGEN como herramienta para Claude Desktop/Cursor/VS Code |
| Agente autónomo con confirmación humana | OWASP LLM08 — no ejecutar acciones destructivas sin aprobación |
| IA con modelo base configurable | Claude primario · OpenAI fallback · Ollama offline |
| RAG con Qdrant | Semántica sobre código generado para respuestas precisas |
| Microservicios agrupados por prefijo | TM_→MovementService · TP_→ParameterService |
