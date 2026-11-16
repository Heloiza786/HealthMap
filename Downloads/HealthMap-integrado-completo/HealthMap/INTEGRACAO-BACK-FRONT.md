# HealthMap — Integração completa Front-end + Back-end

## Executar

### Back-end
```powershell
cd src\HealthMap.Api
dotnet run
```
API padrão: `http://localhost:5177`

### Front-end
Em outro terminal, na raiz `HealthMap`:
```powershell
python -m http.server 5500
```
Front: `http://localhost:5500`

## Principais endpoints

- `GET /api/health`
- `POST /api/auth/login`
- `POST /api/auth/register-medico`
- `POST /api/auth/register-paciente`
- `POST /api/auth/register-secretaria`
- `GET /api/usuarios/`
- `PUT /api/usuarios/{id}`
- `GET/POST /api/especialidades/`
- `GET/POST/PUT/DELETE /api/medicos/`
- `GET/POST/PUT/DELETE /api/pacientes/`
- `GET/PUT/DELETE /api/secretarias/`
- `GET /api/consultas/`
- `POST /api/consultas/agendar`
- `POST /api/consultas/{id}/cancelar`
- `POST /api/consultas/{id}/concluir`
- `POST /api/consultas/{id}/reagendar`
- `POST /api/feedback/`

## Fluxos integrados

Cadastro de médico e paciente criam o usuário e o perfil correspondente no `database.json`. O cadastro de médico também cria/usa a especialidade.

Login usa a API e armazena o token retornado.

Consultas são criadas, canceladas, concluídas e reagendadas pela API.

Atualização de médico/paciente atualiza usuário e perfil no back-end. Exclusão remove o perfil e o usuário correspondente.

O `js/data.js` usa a API como fonte principal. O localStorage mantém somente estado de sessão/cache e dados de demonstração para compatibilidade quando a API estiver indisponível.
