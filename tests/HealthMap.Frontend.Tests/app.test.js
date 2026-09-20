'use strict';

/*
 * Testes das funções utilitárias do frontend (js/app.js)
 * Executados com o test runner nativo do Node: `node --test`
 */

const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const APP_JS_PATH = path.join(__dirname, '..', '..', 'js', 'app.js');
const appJsSource = fs.readFileSync(APP_JS_PATH, 'utf8');

function loadApp() {
  globalThis.document = {
    addEventListener: () => {},
    querySelectorAll: () => [],
    getElementById: () => null,
  };
  globalThis.window = { innerWidth: 1024, location: { pathname: '/' } };
  globalThis.localStorage = {
    getItem: () => null,
    setItem: () => {},
  };

  const wrapped = `(function () {\n${appJsSource}\nglobalThis.__app = { paginate, formatDate, formatDateLong, getStatusLabel, getStatusBadgeClass };\n})();`;
  vm.runInThisContext(wrapped, { filename: 'app.js' });

  return globalThis.__app;
}

test('paginate pagina uma lista e calcula total de páginas', () => {
  const app = loadApp();
  const lista = Array.from({ length: 25 }, (_, i) => i + 1);

  const page1 = app.paginate(lista, 1);
  assert.equal(page1.items.length, 10);
  assert.equal(page1.total, 25);
  assert.equal(page1.totalPages, 3);
  assert.equal(page1.current, 1);
  assert.deepEqual(page1.items, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
});

test('paginate retorna a última página quando solicitada além do limite', () => {
  const app = loadApp();
  const lista = Array.from({ length: 25 }, (_, i) => i + 1);

  const page = app.paginate(lista, 99);

  assert.equal(page.current, 3);
  assert.equal(page.items.length, 5);
});

test('paginate com lista vazia retorna uma página vazia', () => {
  const app = loadApp();

  const page = app.paginate([], 1);

  assert.equal(page.total, 0);
  assert.equal(page.totalPages, 1);
  assert.equal(page.items.length, 0);
});

test('formatDate converte ISO para dd/mm/aaaa', () => {
  const app = loadApp();

  assert.equal(app.formatDate('2026-09-20'), '20/09/2026');
});

test('getStatusLabel e getStatusBadgeClass cobrem todos os status', () => {
  const app = loadApp();

  assert.equal(app.getStatusLabel('pendente'), 'Pendente');
  assert.equal(app.getStatusLabel('reagendada'), 'Reagendada');
  assert.equal(app.getStatusBadgeClass('cancelada'), 'badge-error');
  assert.equal(app.getStatusBadgeClass('desconhecido'), 'badge-info');
});
