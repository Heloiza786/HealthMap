# 📚 HealthMap (MedCloud) — Base de Conhecimento do Projeto

> Documento de conhecimento gerado para orientar o desenvolvimento. Projeto acadêmico de **gestão hospitalar** (agendamento de consultas, histórico de exames e consultas, perfis de paciente/médico/secretária-administrador).
>
> Última atualização: 16/11/2026 · Branch ativa: `healthmap-integrado`

---

## 1. O que é o projeto

Sistema web de **agendamento e gestão de consultas médicas**. O objetivo é ser intuitivo para três perfis:

| Perfil | O que faz |
|--------|-----------|
| **Paciente** | Agenda consultas, acompanha histórico de consultas/exames, gerencia perfil |
| **Médico** | Vê agenda, gerencia consultas e pacientes, define disponibilidade |
| **Secretária / Administrador** | Gerencia usuários, médicos, pacientes, consultas, relatórios e configurações |

Produto usa dois nomes — **HealthMap** (títulos de UI) e **MedCloud** (namespaces JS, servidor, README). São o mesmo sistema. Toda a UI e os textos estão em **pt-BR**.

---

## 2. Tecnologias e arquitetura

O projeto tem **duas frentes** (frontend + backend) que, atualmente, **ainda não estão de fato integradas** (ver §7).

### 2.1 Frontend (SPA pura, sem framework)

Stack: **HTML5 + CSS3 + JavaScript (ES6+)**, sem build, sem módulos (nada de `import`/`export`), sem framework.

Camadas de script (carregadas via `<script>` em ordem):

1. `js/data.js` → `MedCloud` (IIFE). Camada de dados: persistência em `localStorage` (chave `medcloud_data`), dados demo semeados, e toda a lógica de negócio (auth, CRUD de usuários/consultas, estatísticas, reset de senha, agendamento de notificações). Toda leitura passa por `load()`, que faz deep-clone e mescla defaults.
2. `js/app.js` → helpers globais: `requireAuth(tipo)`, `toggleTheme()`, `showToast()`, `formatDate()`, `openModal()/closeModal()`, `showLoading()`, `initBottomNav()`.
3. `js/email-service.js` → `MedCloudEmail` (IIFE). Envio de e-mail client-side, 5 provedores (EmailJS, SMTP-via-backend, SendGrid, Mailgun, Resend).
4. `js/notification-worker.js` → `MedCloudNotificationWorker` (IIFE). Auto-inicia, chama `processarNotificacoesPendentes()` a cada 60s.

`server/server.js` → API Express com `/api/health` e `/api/send-email` (opcional, só para modo SMTP; lê `.env`).

CSS carregado em ordem: `design-system.css` → `reset.css` → `components.css` → `animations.css` → `admin.css` → `responsive.css` → CSS local da página.

### 2.2 Backend (.NET)

Stack: **.NET 9.0**, **ASP.NET Core Minimal API**, `System.Text.Json`, **Swashbuckle** (Swagger). Solução `HealthMap.sln` com 3 projetos:

| Projeto | Responsabilidade |
|---------|------------------|
| `src/HealthMap.Domain/` | Entidades, interfaces de repositório, regras de negócio (services) |
| `src/HealthMap.Infrastructure/` | Repositórios concretos + persistência JSON (`JsonDatabase`, `DatabaseDocument`) |
| `src/HealthMap.Api/` | Endpoints, DTOs, middleware (erros/validação) |

Persistência: **arquivo JSON** em `src/HealthMap.Api/data/database.json` (thread-safe, camelCase, enum como string).

---

## 3. Modelo de dados

### 3.1 Frontend (`localStorage`, chave `medcloud_data`)

- `administradores`, `medicos`, `pacientes` — arrays. Campos comuns: `id` (prefixos `admin-`/`med-`/`pac-`/`cons-`/`notif-`), `nome`, `email`, `senha` (texto puro — **intencional**, demo), `telefone`, `foto`.
  - Médico acrescenta `crm`, `especialidade` (string).
  - Paciente acrescenta `cpf`.
- `consultas` — `id`, `pacienteId`, `medicoId`, `data` (`YYYY-MM-DD`), `hora` (`HH:MM`), `status`, `observacoes`, `criadaEm`.
- `currentUser` (ganha `tipo`: `'admin' | 'medico' | 'paciente'` no login), `resetTokens`, `emailLog`, `notificationSettings`.

**Status de consulta (frontend e backend):** `pendente | confirmada | concluida | cancelada | reagendada`.

### 3.2 Backend (.NET / JSON)

Entidades em `src/HealthMap.Domain/Entities/`. Todas usam GUID como PK.

| Entidade | Campos principais |
|----------|-------------------|
| `Usuario` | `IdUsuario`, `Nome`, `Email`, `SenhaHash`, `Cpf`, `Telefone`, `DataNascimento?` |
| `Paciente` | `IdUsuario` (FK), `PlanoSaude`, `Endereco` |
| `Medico` | `IdUsuario` (FK), `IdEspecialidade`, `Crm` |
| `Secretaria` | `IdUsuario` (FK), `TurnoTrabalho` |
| `Especialidade` | `IdEspecialidade`, `Nome` |
| `DisponibilidadeMedico` | `IdMedico`, `DiaSemana`, `HoraInicio`, `HoraFim` |
| `Consulta` | `IdPaciente`, `IdMedico`, `DataHora`, `Status`, `QueixaPrincipal` |
| `Agendamento` | `IdConsulta`, `IdUsuarioSolicitante`, `DataSolicitacao` |
| `HistoricoMedico` | `IdPaciente`, `Descricao`, `DataRegistro` |
| `Feedback` | `IdConsulta`, `NotaAtendimento`, `Comentario`, `DataFeedback` |
| `ConfirmacaoMensagem` | `IdConsulta`, `TipoMensagem`, `TextoMensagem`, `StatusEnvio`, `RespostaPaciente` |

Enums: `ConsultaStatus` (5 status acima) e `StatusEnvio` (`pendente | enviado | erro`).

Repositórios (interfaces + implementações) cobrem: `Usuario`, `Paciente`, `Medico`, `Secretaria`, `Especialidade`, `Consulta`, `Agendamento`, `DisponibilidadeMedico`, `HistoricoMedico`, `Feedback`, `ConfirmacaoMensagem`.

---

## 4. Regras de negócio (backend services)

- **`AuthService`** — `Autenticar(email, senha)` via `PasswordHasher` (PBKDF2-SHA256, 100k iterações, salt 16 bytes, formato `salt:hash` base64). Token de login = base64 de `id:ticks`.
- **`ConsultaService`**
  - `Agendar(...)`: valida paciente/médico existentes, data futura, e **conflito de horário** (janela de 30min para o mesmo médico). Cria `Consulta` (status `pendente`) + `Agendamento` vinculado.
  - `Cancelar(...)`: impede cancelar consulta concluída; seta `cancelada`.
  - `Concluir(...)`: seta `concluida`.
- **`FeedbackService`** — `Registrar(...)`: valida consulta existe, sem feedback duplicado, nota 1–5.

---

## 5. Endpoints HTTP (backend)

Base da API: `http://localhost:5177` (ver `appsettings` / `launchSettings`).

| Método | Caminho | O que faz |
|--------|---------|-----------|
| GET | `/api/health` | Healthcheck |
| POST | `/api/auth/login` | Login → `LoginResponse` (token, id, nome, email) |
| GET/POST | `/api/usuarios/` | Listar / criar usuário |
| GET | `/api/usuarios/{id}` | Buscar usuário |
| GET/POST | `/api/especialidades/` | Listar / criar especialidade |
| GET/POST/PUT/DELETE | `/api/medicos/` | CRUD médicos (valida CRM único) |
| GET/POST/PUT/DELETE | `/api/pacientes/` | CRUD pacientes |
| GET | `/api/consultas/` | Listar consultas |
| GET | `/api/consultas/{id}` | Buscar consulta |
| POST | `/api/consultas/agendar` | Agendar (via `ConsultaService`) |
| POST | `/api/consultas/{id}/cancelar` | Cancelar |
| POST | `/api/consultas/{id}/concluir` | Concluir |
| POST | `/api/feedback/` | Registrar feedback |

> Registro de médico/paciente/secretária: referenciados no `INTEGRACAO-BACK-FRONT.md` como `POST /api/auth/register-*`, mas **ainda não implementados** nos `AuthEndpoints` atuais (só `login` existe). Ver §7.

---

## 6. Como executar

```bash
# Backend .NET (porta 5177)
dotnet run --project src/HealthMap.Api

# Frontend (qualquer servidor estático na raiz)
npx serve .            # ou: python -m http.server 5500

# Servidor de e-mail (opcional, só SMTP)
cd server && npm install && npm start   # porta 3000
```

**Credenciais demo (frontend/localStorage):**

| Perfil | E-mail | Senha |
|--------|--------|-------|
| Administrador | `admin@medcloud.com` | `admin123` |
| Médico | `medico@gmail.com` | `123` |
| Paciente | `paciente@gmail.com` | `123` |

> No backend `database.json`, as `senhaHash` estão com placeholder `"demo"` — o seed ainda não tem senha real hasheada.

---

## 7. 🔴 Estado atual e lacunas críticas

### Desalinhamento front ⇄ back (o ponto mais importante)

O commit `cf10706 feat: integra front e back` **não aplicou de fato a integração nos arquivos da raiz do repositório**. Evidências:

- A raiz `js/data.js` ainda é a versão **antiga, 100% `localStorage`** (não contém `API_BASE`, `apiRequest`, `fetch`, `syncApiData`, nenhuma chamada à API). Ela usa o modelo antigo (`medicos[].especialidade` como string, `consultas.data`+`consulta.hora`).
- A versão **integrada** (`data.js` com `API_BASE = http://localhost:5177/api`, `apiRequest`, `syncApiData` que mapeia `idUsuario`/`idEspecialidade` e converte `dataHora` → `data`+`hora`) existe **apenas** dentro da pasta não versionada `Downloads/HealthMap-integrado-completo/HealthMap/`.
- O `INTEGRACAO-BACK-FRONT.md` descreve esse estado integrado, mas ele não corresponde ao que está no working tree atual.

Ou seja: **rodar o frontend da raiz hoje ainda usa só `localStorage`**, ignorando o backend .NET.

### Outras lacunas / inconsistências

1. **Endpoints de registro ausentes** — `AuthEndpoints` só tem `POST /api/auth/login`. Os registros `register-medico`/`register-paciente`/`register-secretaria` citados na doc não existem no código atual.
2. **`HistoricoMedico`, `DisponibilidadeMedico`, `Agendamento`, `ConfirmacaoMensagem`** têm entidade, interface e repositório registrados, mas **não têm endpoints nem serviço** expostos ainda (dados de "histórico de exames" não estão acessíveis via API).
3. **Senha demo vs hash** — frontend guarda senha em texto puro; backend espera `SenhaHash` PBKDF2, mas o seed usa `"demo"` (não hasheado). Login real via backend não funciona com as credenciais de demonstração.
4. **CORS liberado** (`AllowAnyOrigin/Header/Method`) e senhas em texto puro são intencionais para demo acadêmica — não "corrigir" sem pedido.
5. **`Downloads/HealthMap-integrado-completo/`** é uma cópia de trabalho (não versionada) que contém a versão mais avançada (front integrado + `tests/` com xUnit + `README.md`/`AGENTS.md`). Tratar como material de referência do que *deveria* estar na raiz.

### Mudanças não commitadas (working tree)

- `src/HealthMap.Api/HealthMap.Api.csproj` — adiciona `Swashbuckle.AspNetCore 6.6.2`.
- `src/HealthMap.Api/Program.cs` — troca `AddOpenApi`/`MapOpenApi` por `AddEndpointsApiExplorer` + `AddSwaggerGen`/`UseSwagger`/`UseSwaggerUI`.

---

## 8. Convenções obrigatórias (não quebrar)

- **Sem módulos ESM.** Manter os 3 namespaces globais: `MedCloud`, `MedCloudEmail`, `MedCloudNotificationWorker`.
- **Ordem de script no `<head>` (crítica):** `data.js` → `app.js` → `email-service.js` → `notification-worker.js`. Caminhos usam `../../` a partir de `pages/<Subpasta>/`.
- **Auth guard:** toda página protegida inicia com `document.addEventListener('DOMContentLoaded', ...)` chamando `requireAuth(tipo)` e retornando cedo se `!user`.
- **Uma página = uma pasta** em `pages/<Nome>/<Nome>.html` (+ CSS opcional). Lógica em `<script>` inline no fim do body.
- **Sempre usar a API `MedCloud`**, nunca tocar `localStorage` direto.
- **UI e mensagens em português.** Usar design tokens (`--color-*`, `--spacing-*`) do `design-system.css`; **não** hardcodar hex.
- **Reusar helpers** (`formatDate`, `getStatusLabel`, `getStatusBadgeClass`, `showToast`, `openModal`).
- **IDs novos:** `prefixo + '-' + Date.now()`.
- **Notificações/retornos:** shape consistente `{ success: true/false, error/message }`.

## 9. Gotchas

- **`getFutureDate`** usa offsets relativos a "agora" (negativos para passado/concluídas) — dados demo sempre parecem frescos.
- **Reset de senha** sempre retorna sucesso (anti-enumeração) + campo `_devToken` para teste.
- **Conflito de consulta** no frontend (`criarConsulta`/`reagendarConsulta`) compara `(medicoId, data, hora)`, ignorando canceladas, `hora` como string.
- **`notification-worker.js`** roda `setInterval` para sempre e loga no console (intencional, mas "spammy").
- **Config de e-mail** vive no `localStorage` (`EMAILJS_*`, `SMTP_*`, `EMAIL_PROVIDER`...), não só `.env`.
- **`plans/audit-and-improvement-plan.md`** é auditoria UI/UX histórica; menciona caminhos defasados (`configemail`, `tests/`, `components/`, `assert/`). Não é fonte autoritativa.
- **`index.html`** é só meta-refresh para login (`pages/LoginInicial/LoginInicial.html`).
- **Testes:** existem projetos `HealthMap.Api.Tests`, `HealthMap.Domain.Tests` e `HealthMap.Frontend.Tests` (na pasta `Downloads/`), mas a raiz versionada **não contém a pasta `tests/`** no working tree (verificada via `git status` — não aparecem como untracked). Rodar via `dotnet test` dentro da pasta onde estiverem.

---

## 10. Cronograma / urgência

- **Marco crítico: até 19/11 (esta semana).** O desenvolvimento está **atrasado**.
- Prioridade máxima de alinhamento com o prazo: **fazer o frontend da raiz falar de fato com o backend** (promover a integração que hoje só existe em `Downloads/`) e **implementar os endpoints de registro** para que paciente, médico e secretária consigam criar conta e operar agendamentos/histórico via API.
