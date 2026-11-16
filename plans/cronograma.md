# Cronograma — HealthMap (Aulas 10–15)

> Tema: **Segurança, LGPD e persistência SQL**.
> Período: 29/10/2026 a 03/12/2026 (aulas quinzenais).

| Aula | Data | Etapa/Fase | Tarefas Previstas | Objetivo da Aula | Entregável Esperado |
|------|------|-----------|-------------------|------------------|---------------------|
| 10 | 29/10/2026 | Banco de Dados SQL e Persistência | Definição do banco de dados SQL; criação/ajuste das tabelas e relacionamentos; definição de chaves e restrições; configuração da conexão segura entre a aplicação e o banco; migração ou organização dos dados existentes. | Estruturar a persistência dos dados em um banco SQL consistente e integrado ao sistema. | Banco SQL configurado, estrutura inicial criada e aplicação conectada ao banco. |
| 11 | 05/11/2026 | Controle de Acesso por Médico | Implementação de autenticação e autorização; associação dos registros ao médico responsável pelo atendimento; aplicação de regras de acesso no backend e nas consultas ao banco; bloqueio de acesso a prontuários de pacientes atendidos por outros médicos; registro de tentativas de acesso relevantes. | Garantir que cada médico acesse somente os dados dos pacientes sob sua responsabilidade no atendimento, sem depender apenas de restrições na interface. | Controle de acesso por médico implementado e cenários de acesso permitido/negado documentados. |
| 12 | 12/11/2026 | LGPD, Privacidade e Segurança dos Dados | Mapeamento dos dados pessoais e dados pessoais sensíveis tratados; revisão de finalidade e necessidade de coleta; definição de perfis e privilégios mínimos; proteção de credenciais e dados em trânsito; revisão de logs, retenção e descarte; elaboração de avisos e orientações de privacidade conforme o contexto do projeto. | Adequar o ambiente e os fluxos do sistema aos princípios e requisitos aplicáveis da LGPD, considerando especialmente os dados de saúde. | Checklist de privacidade e segurança preenchido e ajustes prioritários de proteção aplicados. |
| 13 | 19/11/2026 | Confirmação de E-mail e Senha | Implementação de confirmação de senha no cadastro; validação de formato e unicidade do e-mail; envio de mensagem de confirmação de e-mail com link ou código temporário; validação do token no servidor, expiração e uso único; mensagens de erro claras sem expor informações desnecessárias. | Reduzir erros de cadastro e verificar que o usuário controla o endereço de e-mail informado. | Cadastro com confirmação de senha e fluxo de verificação de e-mail funcionando. |
| 14 | 26/11/2026 | Redefinição de Senha por E-mail | Criação do fluxo de 'esqueci minha senha'; geração de token aleatório, temporário e de uso único; envio de e-mail com link de redefinição; validação e expiração do token; atualização segura da senha; invalidação do token após uso e testes para links expirados ou inválidos. | Permitir a recuperação de acesso sem revelar a senha atual ou permitir redefinições não autorizadas. | E-mail de redefinição de senha integrado e fluxo testado para sucesso e falhas. |
| 15 | 03/12/2026 | Testes de Segurança e Validação Integrada — Documentação e Entrega | Testes de acesso entre médicos diferentes; verificação de tentativas de consulta direta a registros não autorizados; testes de cadastro, confirmação de e-mail e redefinição de senha; revisão de validações, sessões, mensagens de erro e permissões; correção e registro de falhas encontradas. | Validar os controles de acesso, os fluxos de conta e a integração entre frontend, backend e banco SQL. | Relatório de testes, falhas prioritárias corrigidas e fluxos críticos validados. |

---

## Estado atual vs. cronograma

| Etapa | Situação no projeto | Observações |
|-------|--------------------:|-------------|
| Banco SQL (aula 10) | ❌ Não iniciado | Persistência ainda é JSON (`JsonDatabase.cs`). É o maior gap e o próximo passo. |
| Controle de acesso por médico (aula 11) | ❌ Não iniciado | Token de login gerado mas **não validado** nos endpoints; CORS aberto; sem RBAC. |
| LGPD e privacidade (aula 12) | ❌ Não iniciado | Sem consentimento, retenção, logs de acesso ou criptografia em repouso. |
| Confirmação de e-mail (aula 13) | ⚠️ Parcial | Não há confirmação de e-mail no backend. Reset de senha existe só no frontend. |
| Redefinição de senha (aula 14) | ⚠️ Parcial | `solicitarResetSenha`/`redefinirSenha` existem no `data.js` (client-side); hash PBKDF2 no backend. Falta token de uso único no servidor. |
| Testes e validação (aula 15) | ⚠️ Parcial | Projetos de teste existem (`Api.Tests`, `Domain.Tests`, `Frontend.Tests`); falta cobertura de segurança/controle de acesso. |

## Correções aplicadas às datas originais

- Aula 11: `05/10/2026` → `05/11/2026` (manter sequência quinzenal após 29/10).
- Aula 12: `012/11/2026` → `12/11/2026` (typo de digitação).
