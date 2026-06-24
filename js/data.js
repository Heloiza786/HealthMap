/* ============================================
   🏥 MedCloud — Data Layer
   localStorage-based persistence with demo data
   ============================================ */

const MedCloud = (function () {
    const STORAGE_KEY = 'medcloud_data';

    // ── Default Demo Data ──
    const defaultData = {
        // Admin users
        administradores: [
            {
                id: 'admin-1',
                nome: 'Administrador',
                email: 'admin@medcloud.com',
                senha: 'admin123',
                telefone: '(11) 99999-0000',
            },
        ],
        // Users
        medicos: [
            {
                id: 'med-1',
                nome: 'Dr. Carlos Silva',
                email: 'medico@gmail.com',
                senha: '123',
                crm: '123456-SP',
                especialidade: 'Ortopedia',
                telefone: '(11) 99999-0001',
                foto: '',
            },
            {
                id: 'med-2',
                nome: 'Dra. Ana Costa',
                email: 'ana.costa@medcloud.com',
                senha: '123',
                crm: '654321-SP',
                especialidade: 'Dermatologia',
                telefone: '(11) 99999-0002',
                foto: '',
            },
            {
                id: 'med-3',
                nome: 'Dr. Roberto Lima',
                email: 'roberto.lima@medcloud.com',
                senha: '123',
                crm: '789012-SP',
                especialidade: 'Cardiologia',
                telefone: '(11) 99999-0003',
                foto: '',
            },
        ],
        pacientes: [
            {
                id: 'pac-1',
                nome: 'Maria Oliveira',
                email: 'paciente@gmail.com',
                senha: '123',
                cpf: '123.456.789-00',
                telefone: '(11) 98888-0001',
                foto: '',
            },
            {
                id: 'pac-2',
                nome: 'João Santos',
                email: 'joao.santos@email.com',
                senha: '123',
                cpf: '987.654.321-00',
                telefone: '(11) 98888-0002',
                foto: '',
            },
            {
                id: 'pac-3',
                nome: 'Pedro Lima',
                email: 'pedro.lima@email.com',
                senha: '123',
                cpf: '456.789.123-00',
                telefone: '(11) 98888-0003',
                foto: '',
            },
            {
                id: 'pac-4',
                nome: 'Ana Beatriz Costa',
                email: 'ana.beatriz@email.com',
                senha: '123',
                cpf: '321.654.987-00',
                telefone: '(11) 98888-0004',
                foto: '',
            },
        ],
        // Appointments
        consultas: [
            {
                id: 'cons-1',
                pacienteId: 'pac-1',
                medicoId: 'med-1',
                data: getFutureDate(0),
                hora: '10:00',
                status: 'confirmada',
                observacoes: 'Consulta de rotina',
                criadaEm: new Date().toISOString(),
            },
            {
                id: 'cons-2',
                pacienteId: 'pac-2',
                medicoId: 'med-1',
                data: getFutureDate(0),
                hora: '11:00',
                status: 'pendente',
                observacoes: 'Retorno',
                criadaEm: new Date().toISOString(),
            },
            {
                id: 'cons-3',
                pacienteId: 'pac-3',
                medicoId: 'med-3',
                data: getFutureDate(0),
                hora: '15:30',
                status: 'confirmada',
                observacoes: 'Check-up',
                criadaEm: new Date().toISOString(),
            },
            {
                id: 'cons-4',
                pacienteId: 'pac-4',
                medicoId: 'med-2',
                data: getFutureDate(0),
                hora: '14:00',
                status: 'cancelada',
                observacoes: 'Primeira consulta',
                criadaEm: new Date().toISOString(),
            },
            {
                id: 'cons-5',
                pacienteId: 'pac-1',
                medicoId: 'med-1',
                data: getFutureDate(1),
                hora: '09:00',
                status: 'confirmada',
                observacoes: 'Exame de sangue',
                criadaEm: new Date().toISOString(),
            },
            {
                id: 'cons-6',
                pacienteId: 'pac-2',
                medicoId: 'med-2',
                data: getFutureDate(3),
                hora: '14:30',
                status: 'pendente',
                observacoes: 'Consulta dermatológica',
                criadaEm: new Date().toISOString(),
            },
            {
                id: 'cons-7',
                pacienteId: 'pac-3',
                medicoId: 'med-1',
                data: getFutureDate(-5),
                hora: '10:00',
                status: 'concluida',
                observacoes: 'Consulta de rotina',
                criadaEm: new Date().toISOString(),
            },
            {
                id: 'cons-8',
                pacienteId: 'pac-4',
                medicoId: 'med-3',
                data: getFutureDate(-10),
                hora: '16:00',
                status: 'concluida',
                observacoes: 'Check-up cardiológico',
                criadaEm: new Date().toISOString(),
            },
            {
                id: 'cons-9',
                pacienteId: 'pac-1',
                medicoId: 'med-3',
                data: getFutureDate(-3),
                hora: '11:00',
                status: 'concluida',
                observacoes: 'Retorno cardiologia',
                criadaEm: new Date().toISOString(),
            },
            {
                id: 'cons-10',
                pacienteId: 'pac-2',
                medicoId: 'med-1',
                data: getFutureDate(7),
                hora: '08:30',
                status: 'pendente',
                observacoes: 'Cirurgia - pré-operatório',
                criadaEm: new Date().toISOString(),
            },
        ],
        // Password reset tokens
        resetTokens: [],
        // Email notification log
        emailLog: [],
        // Current logged in user
        currentUser: null,
        // Notification settings
        notificationSettings: {
            enabled: true,
            send7Days: true,
            send1Day: true,
            send1Hour: true,
        },
    };

    function getFutureDate(daysOffset) {
        const d = new Date();
        d.setDate(d.getDate() + daysOffset);
        return d.toISOString().split('T')[0];
    }

    // ── Load / Save ──
    function load() {
        try {
            const raw = localStorage.getItem(STORAGE_KEY);
            if (raw) {
                const data = JSON.parse(raw);
                // Merge with defaults to ensure all keys exist
                return { ...deepClone(defaultData), ...data };
            }
        } catch (e) {
            console.warn('Failed to load data, using defaults', e);
        }
        return deepClone(defaultData);
    }

    function save(data) {
        try {
            localStorage.setItem(STORAGE_KEY, JSON.stringify(data));
        } catch (e) {
            console.error('Failed to save data', e);
        }
    }

    function deepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }

    // ── Public API ──
    return {
        // ── Auth ──
        login(email, senha) {
            const data = load();
            let user = data.administradores.find(a => a.email === email && a.senha === senha);
            let tipo = 'admin';
            if (!user) {
                user = data.medicos.find(m => m.email === email && m.senha === senha);
                tipo = 'medico';
            }
            if (!user) {
                user = data.pacientes.find(p => p.email === email && p.senha === senha);
                tipo = 'paciente';
            }
            if (user) {
                data.currentUser = { ...user, tipo };
                save(data);
                return { success: true, user: data.currentUser };
            }
            return { success: false, error: 'E-mail ou senha inválidos' };
        },

        logout() {
            const data = load();
            data.currentUser = null;
            save(data);
        },

        getCurrentUser() {
            const data = load();
            return data.currentUser || null;
        },

        isLoggedIn() {
            return !!this.getCurrentUser();
        },

        isMedico() {
            const u = this.getCurrentUser();
            return u && u.tipo === 'medico';
        },

        isPaciente() {
            const u = this.getCurrentUser();
            return u && u.tipo === 'paciente';
        },

        isAdmin() {
            const u = this.getCurrentUser();
            return u && u.tipo === 'admin';
        },

        // ── Administradores ──
        getAdministradores() {
            return load().administradores;
        },

        // ── Médicos ──
        getMedicos() {
            return load().medicos;
        },

        getMedicoById(id) {
            return load().medicos.find(m => m.id === id) || null;
        },

        updateMedico(id, updates) {
            const data = load();
            const idx = data.medicos.findIndex(m => m.id === id);
            if (idx === -1) return false;
            data.medicos[idx] = { ...data.medicos[idx], ...updates };
            // Update current user if it's the same
            if (data.currentUser && data.currentUser.id === id) {
                data.currentUser = { ...data.currentUser, ...updates };
            }
            save(data);
            return true;
        },

        // ── Pacientes ──
        getPacientes() {
            return load().pacientes;
        },

        getPacienteById(id) {
            return load().pacientes.find(p => p.id === id) || null;
        },

        updatePaciente(id, updates) {
            const data = load();
            const idx = data.pacientes.findIndex(p => p.id === id);
            if (idx === -1) return false;
            data.pacientes[idx] = { ...data.pacientes[idx], ...updates };
            if (data.currentUser && data.currentUser.id === id) {
                data.currentUser = { ...data.currentUser, ...updates };
            }
            save(data);
            return true;
        },

        searchPacientes(query) {
            const q = query.toLowerCase().trim();
            if (!q) return load().pacientes;
            return load().pacientes.filter(p =>
                p.nome.toLowerCase().includes(q) ||
                p.email.toLowerCase().includes(q) ||
                p.cpf.includes(q)
            );
        },

        // ── Consultas ──
        getConsultas() {
            return load().consultas;
        },

        getConsultaById(id) {
            return load().consultas.find(c => c.id === id) || null;
        },

        getConsultasByMedico(medicoId) {
            return load().consultas.filter(c => c.medicoId === medicoId);
        },

        getConsultasByPaciente(pacienteId) {
            return load().consultas.filter(c => c.pacienteId === pacienteId);
        },

        getConsultasDoDia(medicoId) {
            const hoje = new Date().toISOString().split('T')[0];
            return load().consultas.filter(c => c.medicoId === medicoId && c.data === hoje);
        },

        getConsultasFuturas(medicoId) {
            const hoje = new Date().toISOString().split('T')[0];
            return load().consultas.filter(c => c.medicoId === medicoId && c.data >= hoje);
        },

        getConsultasConcluidas(medicoId) {
            return load().consultas.filter(c => c.medicoId === medicoId && c.status === 'concluida');
        },

        getConsultasCanceladas(medicoId) {
            return load().consultas.filter(c => c.medicoId === medicoId && c.status === 'cancelada');
        },

        getConsultasPendentes(medicoId) {
            return load().consultas.filter(c => c.medicoId === medicoId && c.status === 'pendente');
        },

        // Patient-specific
        getConsultasFuturasPaciente(pacienteId) {
            const hoje = new Date().toISOString().split('T')[0];
            return load().consultas.filter(c => c.pacienteId === pacienteId && c.data >= hoje && c.status !== 'cancelada');
        },

        getHistoricoPaciente(pacienteId) {
            const hoje = new Date().toISOString().split('T')[0];
            return load().consultas.filter(c =>
                c.pacienteId === pacienteId &&
                (c.data < hoje || c.status === 'concluida' || c.status === 'cancelada')
            );
        },

        // ── Create / Update / Cancel Consulta ──
        criarConsulta(pacienteId, medicoId, data, hora, observacoes = '') {
            const dataLoad = load();

            // Validate conflict
            const conflito = dataLoad.consultas.some(c =>
                c.medicoId === medicoId &&
                c.data === data &&
                c.hora === hora &&
                c.status !== 'cancelada'
            );
            if (conflito) {
                return { success: false, error: 'Já existe uma consulta agendada neste horário.' };
            }

            const nova = {
                id: 'cons-' + Date.now(),
                pacienteId,
                medicoId,
                data,
                hora,
                status: 'pendente',
                observacoes,
                criadaEm: new Date().toISOString(),
            };
            dataLoad.consultas.push(nova);
            save(dataLoad);

            // Schedule notifications
            this.agendarNotificacoes(nova);

            return { success: true, consulta: nova };
        },

        cancelarConsulta(consultaId) {
            const data = load();
            const cons = data.consultas.find(c => c.id === consultaId);
            if (!cons) return { success: false, error: 'Consulta não encontrada.' };
            cons.status = 'cancelada';
            save(data);
            return { success: true };
        },

        concluirConsulta(consultaId) {
            const data = load();
            const cons = data.consultas.find(c => c.id === consultaId);
            if (!cons) return { success: false, error: 'Consulta não encontrada.' };
            cons.status = 'concluida';
            save(data);
            return { success: true };
        },

        reagendarConsulta(consultaId, novaData, novaHora) {
            const data = load();
            const cons = data.consultas.find(c => c.id === consultaId);
            if (!cons) return { success: false, error: 'Consulta não encontrada.' };

            // Check conflict
            const conflito = data.consultas.some(c =>
                c.id !== consultaId &&
                c.medicoId === cons.medicoId &&
                c.data === novaData &&
                c.hora === novaHora &&
                c.status !== 'cancelada'
            );
            if (conflito) {
                return { success: false, error: 'Já existe uma consulta neste horário.' };
            }

            cons.data = novaData;
            cons.hora = novaHora;
            cons.status = 'pendente';
            save(data);
            return { success: true };
        },

        // ── Stats ──
        getStatsMedico(medicoId) {
            const todas = this.getConsultasByMedico(medicoId);
            const hoje = new Date().toISOString().split('T')[0];
            return {
                consultasHoje: todas.filter(c => c.data === hoje).length,
                pendentes: todas.filter(c => c.status === 'pendente').length,
                concluidas: todas.filter(c => c.status === 'concluida').length,
                canceladas: todas.filter(c => c.status === 'cancelada').length,
                totalPacientes: new Set(todas.map(c => c.pacienteId)).size,
                totalConsultas: todas.length,
            };
        },

        getStatsPaciente(pacienteId) {
            const todas = this.getConsultasByPaciente(pacienteId);
            const hoje = new Date().toISOString().split('T')[0];
            const futuras = todas.filter(c => c.data >= hoje && c.status !== 'cancelada');
            const proxima = futuras.sort((a, b) => a.data.localeCompare(b.data) || a.hora.localeCompare(b.hora))[0];
            return {
                total: todas.length,
                agendadas: todas.filter(c => c.status === 'pendente' || c.status === 'confirmada').length,
                concluidas: todas.filter(c => c.status === 'concluida').length,
                canceladas: todas.filter(c => c.status === 'cancelada').length,
                proxima: proxima || null,
            };
        },

        // ── Admin Stats ──
        getStatsAdmin() {
            const data = load();
            const hoje = new Date().toISOString().split('T')[0];
            const todasConsultas = data.consultas || [];
            return {
                totalMedicos: data.medicos.length,
                totalPacientes: data.pacientes.length,
                totalConsultas: todasConsultas.length,
                consultasHoje: todasConsultas.filter(c => c.data === hoje).length,
                pendentes: todasConsultas.filter(c => c.status === 'pendente').length,
                concluidas: todasConsultas.filter(c => c.status === 'concluida').length,
                canceladas: todasConsultas.filter(c => c.status === 'cancelada').length,
            };
        },

        // ── Admin: User Management ──
        criarMedico(dados) {
            const data = load();
            const novo = {
                id: 'med-' + Date.now(),
                nome: dados.nome,
                email: dados.email,
                senha: dados.senha || '123',
                crm: dados.crm,
                especialidade: dados.especialidade,
                telefone: dados.telefone || '',
                foto: '',
            };
            data.medicos.push(novo);
            save(data);
            return { success: true, user: novo };
        },

        criarPaciente(dados) {
            const data = load();
            const novo = {
                id: 'pac-' + Date.now(),
                nome: dados.nome,
                email: dados.email,
                senha: dados.senha || '123',
                cpf: dados.cpf,
                telefone: dados.telefone || '',
                foto: '',
            };
            data.pacientes.push(novo);
            save(data);
            return { success: true, user: novo };
        },

        desativarMedico(id) {
            const data = load();
            const idx = data.medicos.findIndex(m => m.id === id);
            if (idx === -1) return { success: false, error: 'Médico não encontrado.' };
            // Mark as inactive by removing or flagging
            data.medicos[idx].ativo = false;
            save(data);
            return { success: true };
        },

        desativarPaciente(id) {
            const data = load();
            const idx = data.pacientes.findIndex(p => p.id === id);
            if (idx === -1) return { success: false, error: 'Paciente não encontrado.' };
            data.pacientes[idx].ativo = false;
            save(data);
            return { success: true };
        },

        // ── Chart Data ──
        getConsultasPorMes(medicoId) {
            const todas = this.getConsultasByMedico(medicoId);
            const meses = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];
            const data = new Array(12).fill(0);
            todas.forEach(c => {
                const month = new Date(c.data + 'T12:00:00').getMonth();
                data[month]++;
            });
            return { labels: meses, data };
        },

        getConsultasPorSemana(medicoId) {
            const todas = this.getConsultasByMedico(medicoId);
            const dias = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'];
            const data = new Array(7).fill(0);
            todas.forEach(c => {
                const day = new Date(c.data + 'T12:00:00').getDay();
                data[day]++;
            });
            return { labels: dias, data };
        },

        getConsultasPorStatus(medicoId) {
            const todas = this.getConsultasByMedico(medicoId);
            return {
                labels: ['Concluídas', 'Pendentes', 'Canceladas', 'Confirmadas'],
                data: [
                    todas.filter(c => c.status === 'concluida').length,
                    todas.filter(c => c.status === 'pendente').length,
                    todas.filter(c => c.status === 'cancelada').length,
                    todas.filter(c => c.status === 'confirmada').length,
                ],
                colors: ['#22C55E', '#F59E0B', '#EF4444', '#3B82F6'],
            };
        },

        // ── Password Reset ──
        solicitarResetSenha(email) {
            const data = load();
            const user = data.medicos.find(m => m.email === email) || data.pacientes.find(p => p.email === email);

            // Security: Always return success to not reveal which emails exist
            if (!user) {
                return {
                    success: true,
                    message: 'Se o e-mail existir no sistema, você receberá um link para redefinir sua senha.',
                };
            }

            const token = Math.random().toString(36).substring(2, 15) + Date.now().toString(36) + Math.random().toString(36).substring(2, 8);
            const expires = new Date(Date.now() + 60 * 60 * 1000).toISOString(); // 1 hour

            data.resetTokens.push({
                email,
                token,
                expires,
                usado: false,
                criadoEm: new Date().toISOString(),
            });
            save(data);

            // Build reset link
            const baseUrl = window.location.origin + window.location.pathname.replace(/\/[^/]*$/, '');
            const resetPath = baseUrl.replace('/pages/EsqueciSenha', '/pages/RedefinirSenha/RedefinirSenha.html');
            const resetLink = `${resetPath}?token=${token}`;

            // Send email via EmailJS service
            const userName = user.nome.split(' ')[0];
            const { html, text } = MedCloudEmail.buildPasswordResetEmail(resetLink, userName);

            // Fire and forget - send asynchronously
            MedCloudEmail.sendEmail(email, 'MedCloud — Redefinição de Senha', html, text)
                .then(result => {
                    if (result.success) {
                        console.log(`[EMAIL] Password reset email sent to ${email}`);
                    } else {
                        console.error(`[EMAIL] Failed to send password reset to ${email}:`, result.error);
                    }
                })
                .catch(err => {
                    console.error(`[EMAIL] Error sending password reset to ${email}:`, err);
                });

            console.log(`[EMAIL] Token de reset para ${email}: ${token}`);

            return {
                success: true,
                message: 'Se o e-mail existir no sistema, você receberá um link para redefinir sua senha.',
                // In dev mode, return token for testing
                _devToken: token,
            };
        },

        validarToken(token) {
            const data = load();
            const entry = data.resetTokens.find(t => t.token === token && !t.usado);
            if (!entry) return { success: false, error: 'Token inválido ou expirado.' };
            if (new Date(entry.expires) < new Date()) {
                return { success: false, error: 'Token expirado. Solicite um novo.' };
            }
            return { success: true, email: entry.email };
        },

        redefinirSenha(token, novaSenha) {
            if (!token) return { success: false, error: 'Token de redefinição não fornecido.' };
            if (!novaSenha || novaSenha.length < 3) {
                return { success: false, error: 'A senha deve ter pelo menos 3 caracteres.' };
            }

            const data = load();
            const entry = data.resetTokens.find(t => t.token === token && !t.usado);
            if (!entry) return { success: false, error: 'Token inválido.' };
            if (new Date(entry.expires) < new Date()) {
                return { success: false, error: 'Token expirado. Solicite um novo.' };
            }

            // Update user password
            let updated = false;
            const medIdx = data.medicos.findIndex(m => m.email === entry.email);
            if (medIdx !== -1) {
                data.medicos[medIdx].senha = novaSenha;
                updated = true;
            }
            const pacIdx = data.pacientes.findIndex(p => p.email === entry.email);
            if (pacIdx !== -1) {
                data.pacientes[pacIdx].senha = novaSenha;
                updated = true;
            }

            if (!updated) return { success: false, error: 'Usuário não encontrado.' };

            entry.usado = true;
            save(data);
            return { success: true, message: 'Senha redefinida com sucesso!' };
        },

        // ── Email Notifications ──
        agendarNotificacoes(consulta) {
            const data = load();
            if (!data.notificationSettings.enabled) return;

            const paciente = this.getPacienteById(consulta.pacienteId);
            const medico = this.getMedicoById(consulta.medicoId);
            if (!paciente || !medico) return;

            const consultaDate = new Date(consulta.data + 'T' + consulta.hora + ':00');

            const notifications = [
                { dias: 7, label: '7 dias antes' },
                { dias: 1, label: '1 dia antes' },
                { dias: 0, horas: 1, label: '1 hora antes' },
            ];

            notifications.forEach(notif => {
                // Calculate when this notification should be sent
                const notifDate = new Date(consultaDate);
                if (notif.dias) {
                    notifDate.setDate(notifDate.getDate() - notif.dias);
                }
                if (notif.horas) {
                    notifDate.setHours(notifDate.getHours() - notif.horas);
                }

                const logEntry = {
                    id: 'notif-' + Date.now() + '-' + Math.random().toString(36).substr(2, 5),
                    consultaId: consulta.id,
                    pacienteNome: paciente.nome,
                    pacienteEmail: paciente.email,
                    medicoNome: medico.nome,
                    dataConsulta: consulta.data,
                    horaConsulta: consulta.hora,
                    tipo: notif.label,
                    agendadoPara: notifDate.toISOString(),
                    criadoEm: new Date().toISOString(),
                    enviadoEm: null,
                    status: 'pendente',
                    erro: null,
                };
                data.emailLog.push(logEntry);
            });

            save(data);

            // Try to send immediately if the notification time has already passed
            this.processarNotificacoesPendentes();
        },

        // ── Process pending notifications ──
        processarNotificacoesPendentes() {
            const data = load();
            const now = new Date();
            let modified = false;

            data.emailLog.forEach(entry => {
                // Only process pending notifications that are due
                if (entry.status !== 'pendente') return;
                if (!entry.agendadoPara) return;

                const agendadoPara = new Date(entry.agendadoPara);
                if (agendadoPara > now) return;

                // Find the consulta and send the reminder
                const consulta = data.consultas.find(c => c.id === entry.consultaId);
                if (!consulta) {
                    entry.status = 'erro';
                    entry.erro = 'Consulta não encontrada';
                    entry.enviadoEm = new Date().toISOString();
                    modified = true;
                    return;
                }

                const paciente = this.getPacienteById(consulta.pacienteId);
                const medico = this.getMedicoById(consulta.medicoId);
                if (!paciente || !medico) {
                    entry.status = 'erro';
                    entry.erro = 'Paciente ou médico não encontrado';
                    entry.enviadoEm = new Date().toISOString();
                    modified = true;
                    return;
                }

                // Send the email asynchronously
                const { html, text } = MedCloudEmail.buildAppointmentReminderEmail(consulta, paciente, medico, entry.tipo);
                MedCloudEmail.sendEmail(
                    paciente.email,
                    `MedCloud — Lembrete de Consulta (${entry.tipo})`,
                    html,
                    text
                ).then(result => {
                    const currentData = load();
                    const logEntry = currentData.emailLog.find(e => e.id === entry.id);
                    if (logEntry) {
                        logEntry.status = result.success ? 'enviado' : 'erro';
                        logEntry.erro = result.success ? null : result.error;
                        logEntry.enviadoEm = new Date().toISOString();
                        save(currentData);
                    }
                }).catch(err => {
                    const currentData = load();
                    const logEntry = currentData.emailLog.find(e => e.id === entry.id);
                    if (logEntry) {
                        logEntry.status = 'erro';
                        logEntry.erro = err.message;
                        logEntry.enviadoEm = new Date().toISOString();
                        save(currentData);
                    }
                });

                entry.status = 'enviando';
                entry.enviadoEm = new Date().toISOString();
                modified = true;
            });

            if (modified) save(data);
        },

        // ── Register email send (called by email-service.js) ──
        registrarEnvioEmail(details) {
            const data = load();
            data.emailLog.push({
                id: 'email-' + Date.now() + '-' + Math.random().toString(36).substr(2, 5),
                ...details,
                registradoEm: new Date().toISOString(),
            });
            save(data);
        },

        getEmailLog() {
            return load().emailLog;
        },

        // ── Notification Settings ──
        getNotificationSettings() {
            return load().notificationSettings;
        },

        updateNotificationSettings(settings) {
            const data = load();
            data.notificationSettings = { ...data.notificationSettings, ...settings };
            save(data);
        },

        // ── Data Management ──
        resetData() {
            localStorage.removeItem(STORAGE_KEY);
            return deepClone(defaultData);
        },

        getRawData() {
            return load();
        },
    };
})();
