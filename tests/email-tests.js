/* ============================================
   🧪 MedCloud — Email System Tests
   Tests for password reset, reminders, tokens
   Run in browser console or via test runner
   ============================================ */

const MedCloudTests = (function () {
    let passed = 0;
    let failed = 0;
    let testResults = [];

    // ── Test Framework ──
    function assert(condition, message) {
        if (condition) {
            passed++;
            testResults.push({ status: '✅', message });
            console.log(`  ✅ ${message}`);
        } else {
            failed++;
            testResults.push({ status: '❌', message });
            console.error(`  ❌ ${message}`);
        }
    }

    function describe(name, fn) {
        console.log(`\n📋 ${name}`);
        console.log('─'.repeat(50));
        fn();
    }

    function resetCounters() {
        passed = 0;
        failed = 0;
        testResults = [];
    }

    // ── Helpers ──
    function generateTestEmail() {
        return `test-${Date.now()}@medcloud-test.com`;
    }

    function cleanTestTokens() {
        const data = MedCloud.getRawData();
        data.resetTokens = [];
        localStorage.setItem('medcloud_data', JSON.stringify(data));
    }

    // ── Test Suites ──

    function testPasswordResetFlow() {
        describe('🔐 Password Reset Flow', () => {

            // Test 1: Solicitar reset com e-mail existente
            describe('  Solicitar reset de senha', () => {
                const result = MedCloud.solicitarResetSenha('medico@gmail.com');
                assert(result.success === true, 'solicitarResetSenha deve retornar success=true para e-mail existente');
                assert(result._devToken !== undefined, 'Deve retornar _devToken em modo dev');
                assert(result._devToken.length > 10, 'Token deve ter mais de 10 caracteres');
            });

            // Test 2: Solicitar reset com e-mail inexistente (security: should return success)
            describe('  E-mail inexistente', () => {
                const result = MedCloud.solicitarResetSenha('naoexiste@test.com');
                assert(result.success === true, 'Por segurança, deve retornar success=true mesmo para e-mail inexistente');
                assert(result._devToken === undefined, 'Não deve retornar _devToken para e-mail inexistente');
            });

            // Test 3: Validar token válido
            describe('  Validação de token', () => {
                const result = MedCloud.solicitarResetSenha('paciente@gmail.com');
                const token = result._devToken;
                const validation = MedCloud.validarToken(token);
                assert(validation.success === true, 'Token válido deve ser aceito');
                assert(validation.email === 'paciente@gmail.com', 'Deve retornar o e-mail associado ao token');
            });

            // Test 4: Validar token inválido
            describe('  Token inválido', () => {
                const validation = MedCloud.validarToken('token-invalido-123');
                assert(validation.success === false, 'Token inválido deve ser rejeitado');
                assert(validation.error !== undefined, 'Deve retornar mensagem de erro');
            });

            // Test 5: Token expirado
            describe('  Token expirado', () => {
                const data = MedCloud.getRawData();
                data.resetTokens.push({
                    email: 'test@test.com',
                    token: 'expired-token-123',
                    expires: new Date(Date.now() - 3600000).toISOString(), // 1 hour ago
                    usado: false,
                });
                localStorage.setItem('medcloud_data', JSON.stringify(data));

                const validation = MedCloud.validarToken('expired-token-123');
                assert(validation.success === false, 'Token expirado deve ser rejeitado');
                assert(validation.error && validation.error.toLowerCase().includes('expirado'),
                    'Mensagem de erro deve mencionar expiração');
            });

            // Test 6: Redefinir senha com token válido
            describe('  Redefinir senha', () => {
                const result = MedCloud.solicitarResetSenha('medico@gmail.com');
                const token = result._devToken;
                const resetResult = MedCloud.redefinirSenha(token, 'nova-senha-123');
                assert(resetResult.success === true, 'Redefinição com token válido deve funcionar');

                // Verify password changed
                const loginResult = MedCloud.login('medico@gmail.com', 'nova-senha-123');
                assert(loginResult.success === true, 'Login com nova senha deve funcionar');
                MedCloud.logout();

                // Restore original password
                const restoreResult = MedCloud.solicitarResetSenha('medico@gmail.com');
                const restoreToken = restoreResult._devToken;
                MedCloud.redefinirSenha(restoreToken, '123');
            });

            // Test 7: Token usado não pode ser reutilizado
            describe('  Token não reutilizável', () => {
                const result = MedCloud.solicitarResetSenha('paciente@gmail.com');
                const token = result._devToken;

                // Use the token
                MedCloud.redefinirSenha(token, 'nova-senha-456');

                // Try to use again
                const secondUse = MedCloud.redefinirSenha(token, 'outra-senha');
                assert(secondUse.success === false, 'Token já usado deve ser rejeitado');

                // Restore original password
                const restoreResult = MedCloud.solicitarResetSenha('paciente@gmail.com');
                MedCloud.redefinirSenha(restoreResult._devToken, '123');
            });

            // Test 8: Token armazenado corretamente
            describe('  Armazenamento do token', () => {
                cleanTestTokens();
                const result = MedCloud.solicitarResetSenha('medico@gmail.com');
                const data = MedCloud.getRawData();
                const tokenEntry = data.resetTokens.find(t => t.token === result._devToken);
                assert(tokenEntry !== undefined, 'Token deve estar armazenado no sistema');
                assert(tokenEntry.email === 'medico@gmail.com', 'E-mail deve estar associado ao token');
                assert(tokenEntry.usado === false, 'Token deve iniciar como não usado');
                assert(tokenEntry.expires !== undefined, 'Token deve ter data de expiração');
                assert(new Date(tokenEntry.expires) > new Date(), 'Data de expiração deve ser futura');
            });
        });
    }

    function testAppointmentReminders() {
        describe('📅 Appointment Reminder System', () => {

            // Test 1: Agendar notificações ao criar consulta
            describe('  Agendamento de notificações', () => {
                cleanTestTokens();
                const result = MedCloud.criarConsulta('pac-1', 'med-1', '2026-12-25', '10:00', 'Teste notificação');
                assert(result.success === true, 'Criação de consulta deve funcionar');

                const data = MedCloud.getRawData();
                const consultaNotifs = data.emailLog.filter(e => e.consultaId === result.consulta.id);
                assert(consultaNotifs.length === 3, 'Devem ser criadas 3 notificações (7d, 1d, 1h)');

                const tipos = consultaNotifs.map(e => e.tipo);
                assert(tipos.includes('7 dias antes'), 'Deve ter notificação de 7 dias');
                assert(tipos.includes('1 dia antes'), 'Deve ter notificação de 1 dia');
                assert(tipos.includes('1 hora antes'), 'Deve ter notificação de 1 hora');
            });

            // Test 2: Notificações têm dados corretos
            describe('  Dados das notificações', () => {
                const data = MedCloud.getRawData();
                const lastNotif = data.emailLog[data.emailLog.length - 1];
                assert(lastNotif.pacienteNome !== undefined, 'Deve ter nome do paciente');
                assert(lastNotif.pacienteEmail !== undefined, 'Deve ter e-mail do paciente');
                assert(lastNotif.medicoNome !== undefined, 'Deve ter nome do médico');
                assert(lastNotif.dataConsulta !== undefined, 'Deve ter data da consulta');
                assert(lastNotif.horaConsulta !== undefined, 'Deve ter hora da consulta');
                assert(lastNotif.status === 'pendente' || lastNotif.status === 'enviando',
                    'Notificação deve iniciar como pendente ou enviando');
            });

            // Test 3: Processar notificações pendentes
            describe('  Processamento de notificações', () => {
                // Create a notification that's due (past date)
                const data = MedCloud.getRawData();
                data.emailLog.push({
                    id: 'test-notif-' + Date.now(),
                    consultaId: 'cons-1',
                    pacienteNome: 'Maria Oliveira',
                    pacienteEmail: 'paciente@gmail.com',
                    medicoNome: 'Dr. Carlos Silva',
                    dataConsulta: '2026-06-23',
                    horaConsulta: '10:00',
                    tipo: '1 hora antes',
                    agendadoPara: new Date(Date.now() - 60000).toISOString(), // 1 minute ago
                    criadoEm: new Date().toISOString(),
                    enviadoEm: null,
                    status: 'pendente',
                    erro: null,
                });
                localStorage.setItem('medcloud_data', JSON.stringify(data));

                // Process pending notifications
                MedCloud.processarNotificacoesPendentes();

                const updatedData = MedCloud.getRawData();
                const processedNotif = updatedData.emailLog.find(e => e.id === data.emailLog[data.emailLog.length - 1].id);
                assert(processedNotif !== undefined, 'Notificação deve existir após processamento');
                assert(processedNotif.status === 'enviando' || processedNotif.status === 'enviado' || processedNotif.status === 'erro',
                    'Notificação deve ser processada (enviando/enviado/erro)');
            });

            // Test 4: Configuração de notificações
            describe('  Configuração de notificações', () => {
                const settings = MedCloud.getNotificationSettings();
                assert(settings.enabled === true, 'Notificações devem estar habilitadas por padrão');
                assert(settings.send7Days === true, 'Envio 7 dias antes deve estar habilitado');
                assert(settings.send1Day === true, 'Envio 1 dia antes deve estar habilitado');
                assert(settings.send1Hour === true, 'Envio 1 hora antes deve estar habilitado');

                // Update settings
                MedCloud.updateNotificationSettings({ enabled: false });
                const updatedSettings = MedCloud.getNotificationSettings();
                assert(updatedSettings.enabled === false, 'Configuração deve ser atualizada');

                // Restore
                MedCloud.updateNotificationSettings({ enabled: true });
            });
        });
    }

    function testEmailService() {
        describe('📧 Email Service', () => {

            // Test 1: Build password reset email template
            describe('  Template de redefinição de senha', () => {
                const { html, text } = MedCloudEmail.buildPasswordResetEmail(
                    'https://medcloud.com/reset?token=abc123',
                    'João'
                );
                assert(html.includes('João'), 'HTML deve conter o nome do usuário');
                assert(html.includes('https://medcloud.com/reset?token=abc123'), 'HTML deve conter o link de reset');
                assert(html.includes('Redefinir Senha'), 'HTML deve conter botão de redefinição');
                assert(text.includes('João'), 'Texto deve conter o nome do usuário');
                assert(text.includes('https://medcloud.com/reset?token=abc123'), 'Texto deve conter o link de reset');
            });

            // Test 2: Build appointment reminder template
            describe('  Template de lembrete de consulta', () => {
                const consulta = { data: '2026-12-25', hora: '10:00', observacoes: 'Check-up' };
                const paciente = { nome: 'Maria' };
                const medico = { nome: 'Dr. Carlos', especialidade: 'Ortopedia' };

                const { html, text } = MedCloudEmail.buildAppointmentReminderEmail(consulta, paciente, medico, '1 dia antes');
                assert(html.includes('Maria'), 'HTML deve conter nome do paciente');
                assert(html.includes('Dr. Carlos'), 'HTML deve conter nome do médico');
                assert(html.includes('Ortopedia'), 'HTML deve conter especialidade');
                assert(html.includes('25'), 'HTML deve conter o dia da consulta');
                assert(html.includes('10:00'), 'HTML deve conter o horário');
                assert(text.includes('Maria'), 'Texto deve conter nome do paciente');
            });

            // Test 3: Different reminder types
            describe('  Tipos de lembrete', () => {
                const consulta = { data: '2026-12-25', hora: '10:00', observacoes: '' };
                const paciente = { nome: 'João' };
                const medico = { nome: 'Dr. A', especialidade: '' };

                const reminder7d = MedCloudEmail.buildAppointmentReminderEmail(consulta, paciente, medico, '7 dias antes');
                assert(reminder7d.html.includes('7 dias'), 'Lembrete de 7 dias deve mencionar "7 dias"');

                const reminder1d = MedCloudEmail.buildAppointmentReminderEmail(consulta, paciente, medico, '1 dia antes');
                assert(reminder1d.html.includes('AMANHÃ'), 'Lembrete de 1 dia deve mencionar "AMANHÃ"');

                const reminder1h = MedCloudEmail.buildAppointmentReminderEmail(consulta, paciente, medico, '1 hora antes');
                assert(reminder1h.html.includes('1 HORA'), 'Lembrete de 1 hora deve mencionar "1 HORA"');
            });

            // Test 4: Config management
            describe('  Gerenciamento de configuração', () => {
                const config = MedCloudEmail.getConfig();
                assert(config.emailProvider !== undefined, 'Config deve ter emailProvider');
                assert(config.smtpFrom !== undefined, 'Config deve ter smtpFrom');

                // Update config
                MedCloudEmail.updateConfig({ smtpFrom: 'test@medcloud.com' });
                const updatedConfig = MedCloudEmail.getConfig();
                assert(updatedConfig.smtpFrom === 'test@medcloud.com', 'Config deve ser atualizada');

                // Restore
                MedCloudEmail.updateConfig({ smtpFrom: 'noreply@medcloud.com' });
            });
        });
    }

    function testTokenExpiration() {
        describe('⏰ Token Expiration', () => {

            // Test 1: Token expira após 1 hora
            describe('  Período de expiração', () => {
                const result = MedCloud.solicitarResetSenha('medico@gmail.com');
                const data = MedCloud.getRawData();
                const tokenEntry = data.resetTokens.find(t => t.token === result._devToken);

                const expiresDate = new Date(tokenEntry.expires);
                const now = new Date();
                const diffHours = (expiresDate - now) / (1000 * 60 * 60);
                assert(diffHours > 0.9 && diffHours <= 1.1, 'Token deve expirar em aproximadamente 1 hora');
            });

            // Test 2: Múltiplos tokens para o mesmo e-mail
            describe('  Múltiplos tokens', () => {
                cleanTestTokens();
                MedCloud.solicitarResetSenha('medico@gmail.com');
                MedCloud.solicitarResetSenha('medico@gmail.com');
                MedCloud.solicitarResetSenha('medico@gmail.com');

                const data = MedCloud.getRawData();
                const userTokens = data.resetTokens.filter(t => t.email === 'medico@gmail.com');
                assert(userTokens.length === 3, 'Devem existir 3 tokens para o mesmo e-mail');
            });

            // Test 3: Apenas o primeiro token é válido (não usado)
            describe('  Primeiro token não usado', () => {
                cleanTestTokens();
                const r1 = MedCloud.solicitarResetSenha('medico@gmail.com');
                const r2 = MedCloud.solicitarResetSenha('medico@gmail.com');

                // Use first token
                MedCloud.redefinirSenha(r1._devToken, 'senha-teste');

                // First token should be invalid now
                const v1 = MedCloud.validarToken(r1._devToken);
                assert(v1.success === false, 'Primeiro token deve ser inválido após uso');

                // Second token should still be valid
                const v2 = MedCloud.validarToken(r2._devToken);
                assert(v2.success === true, 'Segundo token deve permanecer válido');

                // Restore password
                MedCloud.redefinirSenha(r2._devToken, '123');
            });
        });
    }

    function testErrorHandling() {
        describe('⚠️ Error Handling', () => {

            // Test 1: Reset com e-mail vazio (security: should return success)
            describe('  E-mail vazio', () => {
                const result = MedCloud.solicitarResetSenha('');
                assert(result.success === true, 'Por segurança, e-mail vazio deve retornar success=true');
                assert(result._devToken === undefined, 'Não deve retornar _devToken para e-mail vazio');
            });

            // Test 2: Reset com e-mail mal formatado (security: should return success)
            describe('  E-mail mal formatado', () => {
                const result = MedCloud.solicitarResetSenha('invalido');
                assert(result.success === true, 'Por segurança, e-mail inválido deve retornar success=true');
                assert(result._devToken === undefined, 'Não deve retornar _devToken para e-mail inválido');
            });

            // Test 3: Redefinir com token vazio
            describe('  Token vazio', () => {
                const result = MedCloud.redefinirSenha('', 'nova-senha');
                assert(result.success === false, 'Token vazio deve retornar erro');
            });

            // Test 4: Redefinir com senha vazia
            describe('  Senha vazia', () => {
                const result = MedCloud.solicitarResetSenha('medico@gmail.com');
                const resetResult = MedCloud.redefinirSenha(result._devToken, '');
                assert(resetResult.success === false, 'Senha vazia deve retornar erro');
            });

            // Test 5: Email service error handling (simulated)
            describe('  Falha no envio de e-mail', () => {
                // Test that sendEmail handles errors gracefully
                const result = MedCloudEmail.sendEmail('', '', '', '');
                // Should not throw, should return error object
                assert(result !== undefined, 'sendEmail não deve lançar exceção');
            });
        });
    }

    // ── Run All Tests ──
    function runAll() {
        resetCounters();
        console.log('\n' + '='.repeat(50));
        console.log('🧪 MedCloud — Email System Test Suite');
        console.log('='.repeat(50));
        console.log(`Started: ${new Date().toLocaleString('pt-BR')}`);
        console.log('='.repeat(50));

        testPasswordResetFlow();
        testAppointmentReminders();
        testEmailService();
        testTokenExpiration();
        testErrorHandling();

        console.log('\n' + '='.repeat(50));
        console.log(`📊 Results: ${passed} passed, ${failed} failed, ${passed + failed} total`);
        console.log('='.repeat(50));

        return {
            passed,
            failed,
            total: passed + failed,
            results: testResults,
            timestamp: new Date().toISOString(),
        };
    }

    // ── Run Specific Suite ──
    function run(suiteName) {
        resetCounters();
        const suites = {
            'password-reset': testPasswordResetFlow,
            'reminders': testAppointmentReminders,
            'email-service': testEmailService,
            'token-expiration': testTokenExpiration,
            'error-handling': testErrorHandling,
        };

        if (suites[suiteName]) {
            suites[suiteName]();
        } else {
            console.error(`Unknown suite: ${suiteName}. Available: ${Object.keys(suites).join(', ')}`);
        }

        return { passed, failed, total: passed + failed };
    }

    return {
        runAll,
        run,
        // Individual suites
        testPasswordResetFlow,
        testAppointmentReminders,
        testEmailService,
        testTokenExpiration,
        testErrorHandling,
    };
})();

// Auto-run if in test mode
if (typeof window !== 'undefined' && window.location.search.includes('runTests=true')) {
    console.log('🧪 Auto-running tests...');
    setTimeout(() => MedCloudTests.runAll(), 500);
}
