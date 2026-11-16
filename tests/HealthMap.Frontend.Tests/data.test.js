'use strict';

/*
 * Testes da camada de dados do frontend (MedCloud / js/data.js)
 * Executados com o test runner nativo do Node: `node --test`
 */

const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const DATA_JS_PATH = path.join(__dirname, '..', '..', 'js', 'data.js');
const dataJsSource = fs.readFileSync(DATA_JS_PATH, 'utf8');

// ── Mocks de ambiente de navegador ──
function createLocalStorageMock() {
  const store = new Map();
  return {
    getItem: (key) => (store.has(key) ? store.get(key) : null),
    setItem: (key, value) => store.set(key, String(value)),
    removeItem: (key) => store.delete(key),
    clear: () => store.clear(),
  };
}

function createWindowMock() {
  return {
    location: {
      origin: 'http://localhost:3000',
      pathname: '/pages/EsqueciSenha/EsqueciSenha.html',
    },
  };
}

function createEmailServiceMock() {
  return {
    buildPasswordResetEmail: (link) => ({ html: link, text: link }),
    buildAppointmentReminderEmail: () => ({ html: '', text: '' }),
    sendEmail: async () => ({ success: true }),
  };
}

/** Carrega uma nova instância do MedCloud com armazenamento limpo. */
function loadMedCloud() {
  globalThis.localStorage = createLocalStorageMock();
  globalThis.window = createWindowMock();
  globalThis.MedCloudEmail = createEmailServiceMock();

  // Envolve o IIFE para que "const MedCloud" não colida entre chamadas.
  const wrapped = `(function () {\n${dataJsSource}\nglobalThis.__MedCloud = MedCloud;\n})();`;
  vm.runInThisContext(wrapped, { filename: 'data.js' });

  return globalThis.__MedCloud;
}

function hoje() {
  return new Date().toISOString().split('T')[0];
}

function daquiA(dias) {
  const d = new Date();
  d.setDate(d.getDate() + dias);
  return d.toISOString().split('T')[0];
}

function ontem() {
  return daquiA(-1);
}

// ════════════════════════════════════════════════════════════════════
// Autenticação
// ════════════════════════════════════════════════════════════════════

test('login com credenciais válidas de admin retorna sucesso e tipo admin', () => {
  const MedCloud = loadMedCloud();

  const result = MedCloud.login('admin@medcloud.com', 'admin123');

  assert.equal(result.success, true);
  assert.equal(result.user.tipo, 'admin');
  assert.equal(MedCloud.getCurrentUser().email, 'admin@medcloud.com');
});

test('login com credenciais válidas de médico retorna tipo medico', () => {
  const MedCloud = loadMedCloud();

  const result = MedCloud.login('medico@gmail.com', '123');

  assert.equal(result.success, true);
  assert.equal(result.user.tipo, 'medico');
  assert.equal(MedCloud.isMedico(), true);
});

test('login com credenciais válidas de paciente retorna tipo paciente', () => {
  const MedCloud = loadMedCloud();

  const result = MedCloud.login('paciente@gmail.com', '123');

  assert.equal(result.success, true);
  assert.equal(result.user.tipo, 'paciente');
  assert.equal(MedCloud.isPaciente(), true);
});

test('login com senha incorreta retorna erro', () => {
  const MedCloud = loadMedCloud();

  const result = MedCloud.login('paciente@gmail.com', 'senha-errada');

  assert.equal(result.success, false);
  assert.ok(result.error);
  assert.equal(MedCloud.isLoggedIn(), false);
});

test('logout limpa o usuário atual', () => {
  const MedCloud = loadMedCloud();
  MedCloud.login('admin@medcloud.com', 'admin123');

  MedCloud.logout();

  assert.equal(MedCloud.isLoggedIn(), false);
});

// ════════════════════════════════════════════════════════════════════
// Consultas: criação, cancelamento, conclusão e reagendamento
// ════════════════════════════════════════════════════════════════════

test('criarConsulta com dados válidos cria consulta pendente', () => {
  const MedCloud = loadMedCloud();

  const result = MedCloud.criarConsulta('pac-1', 'med-2', daquiA(10), '09:30', 'Rotina');

  assert.equal(result.success, true);
  assert.equal(result.consulta.status, 'pendente');
  assert.equal(result.consulta.pacienteId, 'pac-1');
  assert.equal(result.consulta.medicoId, 'med-2');
});

test('criarConsulta com data passada retorna erro', () => {
  const MedCloud = loadMedCloud();

  const result = MedCloud.criarConsulta('pac-1', 'med-2', ontem(), '09:30', '');

  assert.equal(result.success, false);
  assert.match(result.error, /futura/);
});

test('criarConsulta com conflito de médico/data/hora retorna erro', () => {
  const MedCloud = loadMedCloud();
  // O seed possui consulta confirmada do med-1 hoje às 10:00.
  const result = MedCloud.criarConsulta('pac-1', 'med-1', hoje(), '10:00', '');

  assert.equal(result.success, false);
  assert.match(result.error, /Já existe uma consulta/);
});

test('criarConsulta ignora consulta cancelada na verificação de conflito', () => {
  const MedCloud = loadMedCloud();
  const cancelada = MedCloud.getConsultaById('cons-4'); // cancelada no seed

  assert.equal(cancelada.status, 'cancelada');
  const result = MedCloud.criarConsulta(cancelada.pacienteId, cancelada.medicoId, cancelada.data, cancelada.hora, '');

  assert.equal(result.success, true);
});

test('cancelarConsulta atualiza o status para cancelada', () => {
  const MedCloud = loadMedCloud();

  const criada = MedCloud.criarConsulta('pac-1', 'med-2', daquiA(5), '08:00', '');
  const result = MedCloud.cancelarConsulta(criada.consulta.id);

  assert.equal(result.success, true);
  assert.equal(MedCloud.getConsultaById(criada.consulta.id).status, 'cancelada');
});

test('cancelarConsulta inexistente retorna erro', () => {
  const MedCloud = loadMedCloud();

  const result = MedCloud.cancelarConsulta('cons-inexistente');

  assert.equal(result.success, false);
  assert.match(result.error, /não encontrada/);
});

test('cancelarConsulta concluída retorna erro (regra de negócio)', () => {
  const MedCloud = loadMedCloud();
  const criada = MedCloud.criarConsulta('pac-1', 'med-2', daquiA(5), '08:00', '');
  MedCloud.concluirConsulta(criada.consulta.id);

  const result = MedCloud.cancelarConsulta(criada.consulta.id);

  assert.equal(result.success, false);
  assert.match(result.error, /concluída/);
});

test('concluirConsulta atualiza o status para concluida', () => {
  const MedCloud = loadMedCloud();
  const criada = MedCloud.criarConsulta('pac-1', 'med-2', daquiA(5), '08:00', '');

  const result = MedCloud.concluirConsulta(criada.consulta.id);

  assert.equal(result.success, true);
  assert.equal(MedCloud.getConsultaById(criada.consulta.id).status, 'concluida');
});

test('concluirConsulta cancelada retorna erro (regra de negócio)', () => {
  const MedCloud = loadMedCloud();
  const criada = MedCloud.criarConsulta('pac-1', 'med-2', daquiA(5), '08:00', '');
  MedCloud.cancelarConsulta(criada.consulta.id);

  const result = MedCloud.concluirConsulta(criada.consulta.id);

  assert.equal(result.success, false);
  assert.match(result.error, /cancelada/);
});

test('reagendarConsulta atualiza data, hora e define status reagendada', () => {
  const MedCloud = loadMedCloud();
  const criada = MedCloud.criarConsulta('pac-1', 'med-2', daquiA(5), '08:00', '');

  const result = MedCloud.reagendarConsulta(criada.consulta.id, daquiA(10), '15:00');

  assert.equal(result.success, true);
  const atualizada = MedCloud.getConsultaById(criada.consulta.id);
  assert.equal(atualizada.status, 'reagendada');
  assert.equal(atualizada.data, daquiA(10));
  assert.equal(atualizada.hora, '15:00');
});

test('reagendarConsulta com data passada retorna erro', () => {
  const MedCloud = loadMedCloud();
  const criada = MedCloud.criarConsulta('pac-1', 'med-2', daquiA(5), '08:00', '');

  const result = MedCloud.reagendarConsulta(criada.consulta.id, ontem(), '10:00');

  assert.equal(result.success, false);
  assert.match(result.error, /futura/);
});

test('reagendarConsulta com conflito retorna erro', () => {
  const MedCloud = loadMedCloud();
  const criada = MedCloud.criarConsulta('pac-1', 'med-2', daquiA(5), '08:00', '');
  // Segunda consulta no mesmo médico, em outro horário.
  MedCloud.criarConsulta('pac-2', 'med-2', daquiA(10), '10:00', '');

  const result = MedCloud.reagendarConsulta(criada.consulta.id, daquiA(10), '10:00');

  assert.equal(result.success, false);
  assert.match(result.error, /Já existe uma consulta/);
});

test('reagendarConsulta concluída retorna erro (regra de negócio)', () => {
  const MedCloud = loadMedCloud();
  const criada = MedCloud.criarConsulta('pac-1', 'med-2', daquiA(5), '08:00', '');
  MedCloud.concluirConsulta(criada.consulta.id);

  const result = MedCloud.reagendarConsulta(criada.consulta.id, daquiA(10), '10:00');

  assert.equal(result.success, false);
  assert.match(result.error, /concluída/);
});

// ════════════════════════════════════════════════════════════════════
// Consultas: filtros e estatísticas
// ════════════════════════════════════════════════════════════════════

test('getConsultasFuturasPaciente exclui canceladas', () => {
  const MedCloud = loadMedCloud();

  const futuras = MedCloud.getConsultasFuturasPaciente('pac-1');

  assert.ok(futuras.length > 0);
  assert.ok(futuras.every((c) => c.status !== 'cancelada'));
});

test('getConsultasByMedico retorna apenas consultas do médico', () => {
  const MedCloud = loadMedCloud();

  const consultas = MedCloud.getConsultasByMedico('med-1');

  assert.ok(consultas.length > 0);
  assert.ok(consultas.every((c) => c.medicoId === 'med-1'));
});

test('getStatsPaciente conta consultas reagendadas como agendadas', () => {
  const MedCloud = loadMedCloud();
  const criada = MedCloud.criarConsulta('pac-1', 'med-2', daquiA(5), '08:00', '');
  MedCloud.reagendarConsulta(criada.consulta.id, daquiA(10), '10:00');

  const stats = MedCloud.getStatsPaciente('pac-1');

  assert.ok(stats.agendadas >= 1);
});

test('getStatsAdmin retorna contadores do sistema', () => {
  const MedCloud = loadMedCloud();

  const stats = MedCloud.getStatsAdmin();

  assert.ok(stats.totalMedicos >= 3);
  assert.ok(stats.totalPacientes >= 4);
  assert.ok(stats.totalConsultas >= 10);
});

// ════════════════════════════════════════════════════════════════════
// Recuperação de senha (anti-enumeração de contas)
// ════════════════════════════════════════════════════════════════════

test('solicitarResetSenha retorna sucesso mesmo para e-mail inexistente', () => {
  const MedCloud = loadMedCloud();

  const result = MedCloud.solicitarResetSenha('nao@existe.com');

  assert.equal(result.success, true);
  assert.equal(result._devToken, undefined);
});

test('solicitarResetSenha para e-mail existente cria token válido', () => {
  const MedCloud = loadMedCloud();

  const result = MedCloud.solicitarResetSenha('paciente@gmail.com');

  assert.equal(result.success, true);
  assert.ok(result._devToken);
  const validacao = MedCloud.validarToken(result._devToken);
  assert.equal(validacao.success, true);
  assert.equal(validacao.email, 'paciente@gmail.com');
});

test('redefinirSenha com token válido altera a senha e invalida o token', () => {
  const MedCloud = loadMedCloud();
  const solicitacao = MedCloud.solicitarResetSenha('paciente@gmail.com');

  const result = MedCloud.redefinirSenha(solicitacao._devToken, 'novaSenha');

  assert.equal(result.success, true);
  const loginAntigo = MedCloud.login('paciente@gmail.com', '123');
  assert.equal(loginAntigo.success, false);
  const loginNovo = MedCloud.login('paciente@gmail.com', 'novaSenha');
  assert.equal(loginNovo.success, true);

  const reuso = MedCloud.redefinirSenha(solicitacao._devToken, 'outra');
  assert.equal(reuso.success, false);
});

test('redefinirSenha com senha menor que 3 caracteres retorna erro', () => {
  const MedCloud = loadMedCloud();
  const solicitacao = MedCloud.solicitarResetSenha('paciente@gmail.com');

  const result = MedCloud.redefinirSenha(solicitacao._devToken, '12');

  assert.equal(result.success, false);
  assert.match(result.error, /pelo menos 3/);
});

// ════════════════════════════════════════════════════════════════════
// Gestão de dados
// ════════════════════════════════════════════════════════════════════

test('criarMedico e criarPaciente adicionam registros com id único', () => {
  const MedCloud = loadMedCloud();

  const medico = MedCloud.criarMedico({ nome: 'Dr. Novo', email: 'novo@medcloud.com', crm: '111111-SP', especialidade: 'Ortopedia' });
  const paciente = MedCloud.criarPaciente({ nome: 'Novo Paciente', email: 'novo.paciente@email.com', cpf: '111.111.111-11' });

  assert.equal(medico.success, true);
  assert.equal(paciente.success, true);
  assert.ok(medico.user.id.startsWith('med-'));
  assert.ok(paciente.user.id.startsWith('pac-'));
});

test('searchPacientes filtra por nome ou email ou cpf', () => {
  const MedCloud = loadMedCloud();

  assert.ok(MedCloud.searchPacientes('Maria').some((p) => p.nome === 'Maria Oliveira'));
  assert.ok(MedCloud.searchPacientes('joao.santos@email.com').some((p) => p.email === 'joao.santos@email.com'));
  assert.ok(MedCloud.searchPacientes('987.654.321-00').some((p) => p.cpf === '987.654.321-00'));
});

test('resetData restaura os dados padrão', () => {
  const MedCloud = loadMedCloud();
  MedCloud.criarConsulta('pac-1', 'med-2', daquiA(10), '09:00', '');

  MedCloud.resetData();

  assert.equal(MedCloud.getConsultas().length, 10);
});
