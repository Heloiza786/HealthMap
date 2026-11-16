# AGENTS.md — HealthMap / MedCloud

Medical appointment scheduling system. Frontend-only SPA (vanilla JS + HTML + CSS) backed by `localStorage`, with a small optional Node/Express server for SMTP email. No build system, no tests, no framework.

> Note: the project brands itself both "HealthMap" (UI titles) and "MedCloud" (JS namespaces, server, README). These are the same system. UI is in Portuguese (pt-BR).

## Running

No build step. To view the app, serve the repo root over HTTP and open `index.html` (which redirects to `pages/LoginInicial/LoginInicial.html`).

- Frontend: open `index.html` directly, or `npx serve` / any static server from the repo root. Do NOT open individual pages via `file://` as JS uses relative paths.
- Email backend (optional, only for SMTP/backend email mode): `cd server && npm install && npm start` (defaults to port 3000, env `BACKEND_PORT`). There is no `test` or `build` script.

## Architecture

Three distinct layers, all plain `<script>` tags (no modules/imports/exports):

1. `js/data.js` — `MedCloud` (IIFE). The entire data layer: `localStorage` persistence under key `medcloud_data`, demo seed data, and all business logic (auth, CRUD for users/consultas, stats, password reset, email notification scheduling). Every read goes through `load()` which deep-clones and merges defaults.
2. `js/app.js` — global helper functions: `requireAuth(tipo)`, `toggleTheme()`, `showToast()`, `formatDate()`, `openModal()/closeModal()`, `showLoading()`, `initBottomNav()`. Loaded on every page.
3. `js/email-service.js` — `MedCloudEmail` (IIFE). Client-side email sending, supports 5 providers (emailjs, smtp-via-backend, sendgrid, mailgun, resend) configured from `localStorage`.
4. `js/notification-worker.js` — `MedCloudNotificationWorker` (IIFE). Auto-starts on page load, calls `MedCloud.processarNotificacoesPendentes()` every 60s.

`server/server.js` — Express API with `/api/health` and `/api/send-email`. Reads `.env` (via `dotenv` pointed at `../.env`). Only needed for SMTP mode.

### Data model key shapes (stored in `medcloud_data`)

- `administradores`, `medicos`, `pacientes` — arrays of users. All have `id` (prefixed `admin-`, `med-`, `pac-`, `cons-`, `notif-`), `nome`, `email`, `senha` (plaintext — demo only), `telefone`, `foto`. Médicos add `crm`, `especialidade`; pacientes add `cpf`.
- Users gain a `tipo` field (`'admin' | 'medico' | 'paciente'`) only when set as `currentUser` at login.
- `consultas` — appointments: `id`, `pacienteId`, `medicoId`, `data` (ISO `YYYY-MM-DD`), `hora` (`HH:MM`), `status`, `observacoes`, `criadaEm`. Status enum: `pendente | confirmada | concluida | cancelada | reagendada`.
- `currentUser`, `resetTokens`, `emailLog`, `notificationSettings`.

## Conventions & patterns

### Script/style loading order (critical)
Every page `<head>` must load CSS/JS in this relative order, otherwise globals are undefined at `DOMContentLoaded`:

```
<script src="../../js/data.js"></script>
<script src="../../js/app.js"></script>
<script src="../../js/email-service.js"></script>
<script src="../../js/notification-worker.js"></script>
```

Paths use `../../` from `pages/<Subfolder>/`. `data.js` must precede `app.js` (which calls `MedCloud`), and both precede `email-service.js`/`notification-worker.js`.

CSS is loaded in this order: `design-system.css` → `reset.css` → `components.css` → `animations.css` → `admin.css` → `responsive.css`, then a page-local `.css` (e.g. `Dashboard.css`).

### Auth guard (page init pattern)
Every protected page starts its inline `<script>` with:

```js
document.addEventListener('DOMContentLoaded', function () {
    const user = requireAuth('medico'); // or 'paciente' / 'admin'
    if (!user) return;
    // ... render using user.id
});
```

`requireAuth(tipo)` redirects to login if not logged in, or to the correct dashboard if `tipo` mismatches. Pages use `user.tipo` and `user.id` to drive `MedCloud` queries.

### One page = one folder
Each page lives in `pages/<PageName>/<PageName>.html` with an optional `<PageName>.css`. Inline `<script>` at the bottom of the body holds page-specific logic (no separate `.js` per page). Navigation between pages uses `../` or `../../` relative links.

### Consistency rules
- Always use the shared `MedCloud` API rather than touching `localStorage` directly — `data.js` owns all reads/writes.
- UI copy and messages are Portuguese.
- Use CSS custom properties from `design-system.css` (`--color-*`, `--spacing-*`, etc.) — do not hardcode hex colors.
- Reuse existing helpers rather than reinventing: `formatDate`, `getStatusLabel`, `getStatusBadgeClass`, `showToast`, `openModal`.
- IDs are generated with `prefix + '-' + Date.now()` for new records.
- New notification types/messages: return consistently shaped objects `{ success: true/false, error/message }`.

## Gotchas

- **No modules**: declarations are global. Keep the three namespaces (`MedCloud`, `MedCloudEmail`, `MedCloudNotificationWorker`) intact; do not convert to ESM.
- **`getFutureDate` seed data** uses offsets relative to "now", so demo consultations always appear fresh; past/completed appts use negative offsets.
- **Plaintext passwords** (`senha`) are stored and compared directly. This is intentional demo behavior; do not "fix" it unless asked.
- **Demo logins** seeded in `data.js`: admin `admin@medcloud.com`/`admin123`; médico `medico@gmail.com`/`123`; paciente `paciente@gmail.com`/`123`.
- **Email config lives in `localStorage`**, not only `.env`. `email-service.js` reads keys from `localStorage` first (`EMAILJS_*`, `SMTP_*`, `EMAIL_PROVIDER`, etc.). The admin settings UI writes these.
- **Password reset** always returns success (anti-enumeration) and includes a `_devToken` field for testing even when email isn't configured.
- **Conflict checking** in `criarConsulta`/`reagendarConsulta` is on `(medicoId, data, hora)` ignoring cancelled appts; hour is compared as string, date as `YYYY-MM-DD`.
- **`notification-worker.js`** runs a `setInterval` forever on every page load and logs to console — spammy but intentional. It polls `localStorage.emailLog` for `status === 'pendente'` entries whose `agendadoPara` has passed.
- **Images**: a `plans/audit-and-improvement-plan.md` documents a UI/UX audit; it references file paths that may be stale (mentions `configemail`, `tests/`, `components/`, `assert/` that no longer exist). Treat it as historical, not authoritative.
- **`index.html`** is just a meta-refresh to the login page.
- **Empty dirs**: `plans/` is the only docs/plan folder; there is no `tests/` or CI.
