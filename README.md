# 🏥 HealthMap (MedCloud)

Sistema de agendamento de consultas médicas para clínicas e profissionais de saúde. Permite o cadastro e a gestão de médicos, pacientes e consultas, com painéis específicos para cada perfil de usuário (médico, paciente e administrador), agendamento, acompanhamento de consultas e envio de notificações por e-mail.

> O sistema também é referenciado como **MedCloud** (namespaces em JavaScript, servidor e camada de dados). Trata-se do mesmo produto.

---

## 👥 Equipe

| Integrante | RA |
|------------|-----|
| Heloiza Custodio | 06009234 |
| Larissa Ferreira | 06011175 |
| Rafael de Alcantara | 06010477 |

---

## 📋 Sumário

- [Visão Geral](#-visão-geral)
- [Funcionalidades](#-funcionalidades)
- [Tecnologias Utilizadas](#-tecnologias-utilizadas)
- [Arquitetura do Sistema](#-arquitetura-do-sistema)
- [Requisitos](#-requisitos)
- [Como Executar](#-como-executar)
- [Credenciais de Demonstração](#-credenciais-de-demonstração)
- [Estrutura de Pastas](#-estrutura-de-pastas)
- [Modelo de Dados](#-modelo-de-dados)
- [Configuração de E-mail](#-configuração-de-e-mail)
- [Manual do Usuário](#-manual-do-usuário)

---

## 🌐 Visão Geral

O HealthMap é um aplicativo web de agendamento médico dividido em dois grandes blocos:

- **Frontend (SPA)** — interface em HTML, CSS e JavaScript puro (sem frameworks), executada diretamente no navegador.
- **Backend (.NET)** — API em .NET estruturada em camadas (Domain, Infrastructure e Api), responsável pela persistência dos dados em JSON.

O sistema atende a três perfis de usuário, cada um com funcionalidades e telas próprias:

| Perfil | Descrição |
|--------|-----------|
| **Médico** | Gerencia consultas, agenda horários e acompanha seus pacientes. |
| **Paciente** | Agenda consultas, acompanha seu histórico e gerencia seu perfil. |
| **Administrador** | Gerencia usuários, médicos, pacientes, consultas e relatórios do sistema. |

---

## ✨ Funcionalidades

- **Autenticação e controle de acesso** — login com perfis distintos e redirecionamento para o painel correto.
- **Cadastro de usuários** — médicos, pacientes e secretários.
- **Agendamento de consultas** — com validação de conflito de horário por médico.
- **Reagendamento e cancelamento** de consultas.
- **Painéis (dashboards)** por perfil, com visão de calendário e estatísticas.
- **Gestão administrativa** — usuários, médicos, pacientes e consultas.
- **Relatórios e estatísticas** do sistema.
- **Recuperação de senha** com token (anti-enumeração de contas).
- **Notificações por e-mail** — agendamento automático de envio com múltiplos provedores (EmailJS, SMTP, SendGrid, Mailgun, Resend).
- **Modo escuro** e design responsivo.

---

## 🛠 Tecnologias Utilizadas

| Camada | Tecnologias |
|--------|-------------|
| Frontend | HTML5, CSS3, JavaScript (ES6+) |
| Backend | .NET (C#), ASP.NET Core Minimal API |
| Persistência | JSON (arquivo local) |
| E-mail | EmailJS, SMTP (Nodemailer), SendGrid, Mailgun, Resend |
| Servidor auxiliar | Node.js + Express (opcional, apenas para SMTP) |

---

## 🏗 Arquitetura do Sistema

O backend .NET segue o padrão de **camadas**, separando responsabilidades:

```
HealthMap.sln
├── src/
│   ├── HealthMap.Domain/        # Entidades, interface (contratos) e regras de negócio
│   ├── HealthMap.Infrastructure/ # Implementação de repositórios e persistência JSON
│   └── HealthMap.Api/           # API (endpoints, DTOs, middleware)
├── js/                          # Frontend (SPA em JavaScript puro)
├── pages/                       # Páginas HTML
├── styles/                      # Estilos e design system
└── server/                      # Servidor Node.js (opcional, para SMTP)
```

### Camadas do backend

1. **Domain** — entidades de domínio (Consulta, Medico, Paciente, Usuario etc.), interfaces de repositório e serviços de regra de negócio.
2. **Infrastructure** — repositórios concretos que implementam as interfaces, com persistência em arquivo JSON.
3. **Api** — camada de apresentação, com `Endpoints`, `DTOs` e `Middleware` de tratamento de erros.

---

## 📦 Requisitos

- **Frontend**: qualquer navegador moderno e um servidor estático (ex.: `npx serve`).
- **Backend .NET**: [.NET SDK 8.0+](https://dotnet.microsoft.com/download).
- **Servidor de e-mail (opcional)**: [Node.js](https://nodejs.org/) 18+.

---

## ▶️ Como Executar

### 1. Frontend

O frontend não possui etapa de build. Basta servir a raiz do repositório por HTTP e abrir o `index.html` (que redireciona para a tela de login):

```bash
npx serve .
```

> Evite abrir as páginas diretamente via `file://`, pois o JavaScript usa caminhos relativos.

### 2. Backend .NET

```bash
dotnet restore
dotnet run --project src/HealthMap.Api
```

A API inicia por padrão em `http://localhost:5xxx` (configuração em `src/HealthMap.Api/appsettings.json`).

### 3. Servidor de e-mail (opcional)

Necessário apenas para o modo SMTP:

```bash
cd server
npm install
npm start
```

O servidor escuta na porta `3000` por padrão (configurável via variável `BACKEND_PORT`).

---

## 🔑 Credenciais de Demonstração

| Perfil | E-mail | Senha |
|--------|--------|-------|
| Administrador | `admin@medcloud.com` | `admin123` |
| Médico | `medico@gmail.com` | `123` |
| Paciente | `paciente@gmail.com` | `123` |

---

## 📁 Estrutura de Pastas

```
HealthMap/
├── index.html                    # Redireciona para a tela de login
├── js/                           # Camada de dados e utilitários (SPA)
│   ├── data.js                   # Camada de dados (MedCloud)
│   ├── app.js                    # Utilitários compartilhados
│   ├── email-service.js          # Serviço de e-mail (MedCloudEmail)
│   └── notification-worker.js    # Worker de notificações
├── styles/                       # Design system e estilos
├── pages/                        # Páginas HTML (uma pasta por página)
├── src/                          # Backend .NET em camadas
│   ├── HealthMap.Domain/
│   ├── HealthMap.Infrastructure/
│   └── HealthMap.Api/
├── server/                       # Servidor Node.js (opcional, SMTP)
├── docs/                         # Documentos e diagramas auxiliares
├── images/                       # Imagens estáticas
└── plans/                        # Documentos de planejamento
```

---

## 🗃 Modelo de Dados

### Entidades principais

- **Usuarios** — base comum (`administradores`, `medicos`, `pacientes`), com `id`, `nome`, `email`, `senha`, `telefone`, `foto`.
- **Medico** — adiciona `crm` e `especialidade`.
- **Paciente** — adiciona `cpf`.
- **Consulta** — `pacienteId`, `medicoId`, `data`, `hora`, `status`, `observacoes`, `criadaEm`.

### Estados de consulta

| Status | Significado |
|--------|-------------|
| `pendente` | Aguardando confirmação |
| `confirmada` | Confirmada |
| `concluida` | Realizada |
| `cancelada` | Cancelada |
| `reagendada` | Remarcada |

---

## 📧 Configuração de E-mail

As configurações de e-mail são armazenadas no `localStorage` (não apenas em `.env`) e podem ser editadas na tela de configurações do administrador. O serviço `email-service.js` lê as chaves diretamente do armazenamento local.

Provedores suportados:

- EmailJS
- SMTP (via servidor backend)
- SendGrid
- Mailgun
- Resend

---

## 📖 Manual do Usuário

### Acessar o sistema

1. Abra a aplicação no navegador (veja [Como Executar](#-como-executar)).
2. Informe as credenciais do perfil desejado (veja [Credenciais de Demonstração](#-credenciais-de-demonstração)).
3. Você será redirecionado ao painel correspondente ao seu perfil.

### Como médico

- Acesse o painel (`Dashboard`) para ver suas consultas e estatísticas.
- Em `Consultas`, visualize e gerencie seus agendamentos.
- Em `Pacientes`, veja a lista de pacientes atendidos.
- Edite seu perfil em `Perfil`.

### Como paciente

- Agende uma consulta em `Nova Consulta` (escolha a especialidade e o médico).
- Acompanhe seus agendamentos em `Minhas Consultas`.
- Gerencie seu perfil em `Perfil`.

### Como administrador

- Gerencie usuários, médicos e pacientes.
- Controle as consultas e realize novos agendamentos.
- Consulte relatórios e estatísticas.
- Configure o envio de e-mails.

---

## ⚠️ Observações

- As senhas são armazenadas em texto puro — comportamento **intencional** para fins de demonstração acadêmica.
- O worker de notificações executa uma checagem a cada 60 segundos.
- Os dados das consultas de demonstração usam datas relativas à data atual, mantendo-as sempre "frescas".
