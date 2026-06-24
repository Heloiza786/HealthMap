/* ============================================
   📧 MedCloud — Email Service
   Client-side email sending via EmailJS
   Supports: EmailJS, SMTP (via backend), SendGrid, Mailgun, Resend
   ============================================ */

const MedCloudEmail = (function () {
    // ── Configuration ──
    // EmailJS defaults (user must replace with their own keys)
    const CONFIG = {
        // EmailJS
        emailjsServiceId: localStorage.getItem('EMAILJS_SERVICE_ID') || '',
        emailjsTemplateId: localStorage.getItem('EMAILJS_TEMPLATE_ID') || '',
        emailjsPublicKey: localStorage.getItem('EMAILJS_PUBLIC_KEY') || '',

        // SMTP / General
        smtpHost: localStorage.getItem('SMTP_HOST') || '',
        smtpPort: localStorage.getItem('SMTP_PORT') || '587',
        smtpUser: localStorage.getItem('SMTP_USER') || '',
        smtpPass: localStorage.getItem('SMTP_PASS') || '',
        smtpFrom: localStorage.getItem('SMTP_FROM') || 'noreply@medcloud.com',
        smtpSecure: localStorage.getItem('SMTP_SECURE') === 'true',

        // Provider selection
        emailProvider: localStorage.getItem('EMAIL_PROVIDER') || 'emailjs', // emailjs | smtp | sendgrid | mailgun | resend

        // SendGrid
        sendgridApiKey: localStorage.getItem('SENDGRID_API_KEY') || '',

        // Mailgun
        mailgunApiKey: localStorage.getItem('MAILGUN_API_KEY') || '',
        mailgunDomain: localStorage.getItem('MAILGUN_DOMAIN') || '',

        // Resend
        resendApiKey: localStorage.getItem('RESEND_API_KEY') || '',
    };

    // ── EmailJS SDK Loader ──
    function loadEmailJSSDK() {
        return new Promise((resolve, reject) => {
            if (window.emailjs) {
                resolve(window.emailjs);
                return;
            }
            const script = document.createElement('script');
            script.src = 'https://cdn.jsdelivr.net/npm/@emailjs/browser@4/dist/email.min.js';
            script.onload = () => {
                if (window.emailjs) {
                    window.emailjs.init(CONFIG.emailjsPublicKey);
                    resolve(window.emailjs);
                } else {
                    reject(new Error('EmailJS SDK failed to initialize'));
                }
            };
            script.onerror = () => reject(new Error('Failed to load EmailJS SDK'));
            document.head.appendChild(script);
        });
    }

    // ── Send via EmailJS ──
    async function sendViaEmailJS(to, subject, htmlBody, textBody) {
        if (!CONFIG.emailjsServiceId || !CONFIG.emailjsTemplateId || !CONFIG.emailjsPublicKey) {
            throw new Error('EmailJS não configurado. Configure as chaves no painel de configurações.');
        }

        const emailjs = await loadEmailJSSDK();

        const templateParams = {
            to_email: to,
            to_name: to.split('@')[0],
            subject: subject,
            message: htmlBody,
            html_message: htmlBody,
            text_message: textBody || htmlBody.replace(/<[^>]*>/g, ''),
            from_name: 'MedCloud',
            from_email: CONFIG.smtpFrom,
        };

        const response = await emailjs.send(
            CONFIG.emailjsServiceId,
            CONFIG.emailjsTemplateId,
            templateParams
        );

        return {
            success: true,
            provider: 'emailjs',
            messageId: response?.text || 'unknown',
        };
    }

    // ── Send via Backend SMTP API ──
    async function sendViaBackend(to, subject, htmlBody, textBody) {
        const backendUrl = localStorage.getItem('BACKEND_URL') || 'http://localhost:3000';

        const response = await fetch(`${backendUrl}/api/send-email`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                to,
                subject,
                html: htmlBody,
                text: textBody || htmlBody.replace(/<[^>]*>/g, ''),
                from: CONFIG.smtpFrom,
                smtp: {
                    host: CONFIG.smtpHost,
                    port: parseInt(CONFIG.smtpPort),
                    user: CONFIG.smtpUser,
                    pass: CONFIG.smtpPass,
                    secure: CONFIG.smtpSecure,
                },
            }),
        });

        if (!response.ok) {
            const err = await response.json().catch(() => ({}));
            throw new Error(err.error || `HTTP ${response.status}: Falha ao enviar e-mail`);
        }

        return await response.json();
    }

    // ── Send via SendGrid (direct API) ──
    async function sendViaSendGrid(to, subject, htmlBody, textBody) {
        if (!CONFIG.sendgridApiKey) {
            throw new Error('SendGrid não configurado. Configure a chave SENDGRID_API_KEY.');
        }

        const response = await fetch('https://api.sendgrid.com/v3/mail/send', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${CONFIG.sendgridApiKey}`,
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                personalizations: [{ to: [{ email: to }] }],
                from: { email: CONFIG.smtpFrom, name: 'MedCloud' },
                subject,
                content: [
                    { type: 'text/plain', value: textBody || htmlBody.replace(/<[^>]*>/g, '') },
                    { type: 'text/html', value: htmlBody },
                ],
            }),
        });

        if (!response.ok) {
            const err = await response.text().catch(() => 'Unknown error');
            throw new Error(`SendGrid error: ${err}`);
        }

        return { success: true, provider: 'sendgrid' };
    }

    // ── Send via Mailgun ──
    async function sendViaMailgun(to, subject, htmlBody, textBody) {
        if (!CONFIG.mailgunApiKey || !CONFIG.mailgunDomain) {
            throw new Error('Mailgun não configurado. Configure MAILGUN_API_KEY e MAILGUN_DOMAIN.');
        }

        const formData = new URLSearchParams();
        formData.append('from', `MedCloud <${CONFIG.smtpFrom}>`);
        formData.append('to', to);
        formData.append('subject', subject);
        formData.append('text', textBody || htmlBody.replace(/<[^>]*>/g, ''));
        formData.append('html', htmlBody);

        const response = await fetch(
            `https://api.mailgun.net/v3/${CONFIG.mailgunDomain}/messages`,
            {
                method: 'POST',
                headers: {
                    'Authorization': 'Basic ' + btoa('api:' + CONFIG.mailgunApiKey),
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: formData.toString(),
            }
        );

        if (!response.ok) {
            const err = await response.text().catch(() => 'Unknown error');
            throw new Error(`Mailgun error: ${err}`);
        }

        return { success: true, provider: 'mailgun' };
    }

    // ── Send via Resend ──
    async function sendViaResend(to, subject, htmlBody, textBody) {
        if (!CONFIG.resendApiKey) {
            throw new Error('Resend não configurado. Configure RESEND_API_KEY.');
        }

        const response = await fetch('https://api.resend.com/emails', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${CONFIG.resendApiKey}`,
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                from: `MedCloud <${CONFIG.smtpFrom}>`,
                to: [to],
                subject,
                html: htmlBody,
                text: textBody || htmlBody.replace(/<[^>]*>/g, ''),
            }),
        });

        if (!response.ok) {
            const err = await response.json().catch(() => ({}));
            throw new Error(err.message || `Resend error: HTTP ${response.status}`);
        }

        return { success: true, provider: 'resend' };
    }

    // ── Public: Send Email (auto-selects provider) ──
    async function sendEmail(to, subject, htmlBody, textBody) {
        const provider = CONFIG.emailProvider;
        let result;

        try {
            switch (provider) {
                case 'emailjs':
                    result = await sendViaEmailJS(to, subject, htmlBody, textBody);
                    break;
                case 'smtp':
                    result = await sendViaBackend(to, subject, htmlBody, textBody);
                    break;
                case 'sendgrid':
                    result = await sendViaSendGrid(to, subject, htmlBody, textBody);
                    break;
                case 'mailgun':
                    result = await sendViaMailgun(to, subject, htmlBody, textBody);
                    break;
                case 'resend':
                    result = await sendViaResend(to, subject, htmlBody, textBody);
                    break;
                default:
                    // Try EmailJS first, fallback to backend
                    try {
                        result = await sendViaEmailJS(to, subject, htmlBody, textBody);
                    } catch (e) {
                        console.warn('[EMAIL] EmailJS failed, trying backend SMTP:', e.message);
                        result = await sendViaBackend(to, subject, htmlBody, textBody);
                    }
            }

            // Log successful send
            MedCloud.registrarEnvioEmail({
                to,
                subject,
                provider: result.provider || provider,
                messageId: result.messageId,
                status: 'enviado',
                enviadoEm: new Date().toISOString(),
            });

            console.log(`[EMAIL] Sent to ${to} via ${result.provider || provider}: ${subject}`);
            return { success: true, ...result };
        } catch (error) {
            // Log failed send
            MedCloud.registrarEnvioEmail({
                to,
                subject,
                provider,
                status: 'erro',
                erro: error.message,
                enviadoEm: new Date().toISOString(),
            });

            console.error(`[EMAIL] Failed to send to ${to}:`, error.message);
            return { success: false, error: error.message };
        }
    }

    // ── Email Template Builders ──

    function buildPasswordResetEmail(resetLink, userName) {
        const html = `
            <!DOCTYPE html>
            <html>
            <head><meta charset="utf-8"></head>
            <body style="font-family: 'Inter', Arial, sans-serif; background: #f4f7fc; margin: 0; padding: 0;">
                <table width="100%" cellpadding="0" cellspacing="0" style="background: #f4f7fc; padding: 40px 20px;">
                    <tr>
                        <td align="center">
                            <table width="600" cellpadding="0" cellspacing="0" style="background: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 24px rgba(0,0,0,0.08);">
                                <!-- Header -->
                                <tr>
                                    <td style="background: linear-gradient(135deg, #2563EB, #1D4ED8); padding: 32px; text-align: center;">
                                        <h1 style="color: #ffffff; font-size: 24px; margin: 0;">🏥 MedCloud</h1>
                                        <p style="color: rgba(255,255,255,0.85); font-size: 14px; margin: 8px 0 0 0;">Redefinição de Senha</p>
                                    </td>
                                </tr>
                                <!-- Body -->
                                <tr>
                                    <td style="padding: 32px;">
                                        <h2 style="color: #1e293b; font-size: 20px; margin: 0 0 16px 0;">Olá, ${userName}!</h2>
                                        <p style="color: #475569; font-size: 15px; line-height: 1.6; margin: 0 0 24px 0;">
                                            Recebemos uma solicitação para redefinir a senha da sua conta no MedCloud.
                                            Clique no botão abaixo para criar uma nova senha:
                                        </p>
                                        <table width="100%" cellpadding="0" cellspacing="0">
                                            <tr>
                                                <td align="center" style="padding: 0 0 24px 0;">
                                                    <a href="${resetLink}" style="display: inline-block; background: #2563EB; color: #ffffff; text-decoration: none; padding: 14px 32px; border-radius: 8px; font-size: 16px; font-weight: 600;">Redefinir Senha</a>
                                                </td>
                                            </tr>
                                        </table>
                                        <p style="color: #94a3b8; font-size: 13px; line-height: 1.5; margin: 0 0 8px 0;">
                                            Se você não solicitou esta redefinição, ignore este e-mail.
                                        </p>
                                        <p style="color: #94a3b8; font-size: 13px; line-height: 1.5; margin: 0;">
                                            Este link expira em <strong>1 hora</strong>.
                                        </p>
                                        <hr style="border: none; border-top: 1px solid #e2e8f0; margin: 24px 0;">
                                        <p style="color: #94a3b8; font-size: 12px; margin: 0;">
                                            Link direto: <a href="${resetLink}" style="color: #2563EB;">${resetLink}</a>
                                        </p>
                                    </td>
                                </tr>
                                <!-- Footer -->
                                <tr>
                                    <td style="background: #f8fafc; padding: 20px 32px; text-align: center;">
                                        <p style="color: #94a3b8; font-size: 12px; margin: 0;">
                                            © ${new Date().getFullYear()} MedCloud. Todos os direitos reservados.
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
        `;
        const text = `Olá, ${userName}!\n\nRecebemos uma solicitação para redefinir a senha da sua conta no MedCloud.\n\nAcesse o link abaixo para criar uma nova senha:\n${resetLink}\n\nSe você não solicitou esta redefinição, ignore este e-mail.\nEste link expira em 1 hora.\n\n© ${new Date().getFullYear()} MedCloud.`;
        return { html, text };
    }

    function buildAppointmentReminderEmail(consulta, paciente, medico, tipo) {
        const dataFormatada = new Date(consulta.data + 'T12:00:00').toLocaleDateString('pt-BR', {
            day: 'numeric', month: 'long', year: 'numeric',
        });

        let reminderText = '';
        let urgencyColor = '#2563EB';
        if (tipo === '7 dias antes') {
            reminderText = 'Lembrete: Sua consulta está agendada para daqui a 7 dias.';
            urgencyColor = '#2563EB';
        } else if (tipo === '1 dia antes') {
            reminderText = 'Lembrete: Sua consulta é AMANHÃ!';
            urgencyColor = '#F59E0B';
        } else if (tipo === '1 hora antes') {
            reminderText = '⚠️ Lembrete: Sua consulta é EM 1 HORA!';
            urgencyColor = '#EF4444';
        }

        const html = `
            <!DOCTYPE html>
            <html>
            <head><meta charset="utf-8"></head>
            <body style="font-family: 'Inter', Arial, sans-serif; background: #f4f7fc; margin: 0; padding: 0;">
                <table width="100%" cellpadding="0" cellspacing="0" style="background: #f4f7fc; padding: 40px 20px;">
                    <tr>
                        <td align="center">
                            <table width="600" cellpadding="0" cellspacing="0" style="background: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 24px rgba(0,0,0,0.08);">
                                <tr>
                                    <td style="background: linear-gradient(135deg, ${urgencyColor}, ${urgencyColor}dd); padding: 32px; text-align: center;">
                                        <h1 style="color: #ffffff; font-size: 24px; margin: 0;">🏥 MedCloud</h1>
                                        <p style="color: rgba(255,255,255,0.85); font-size: 14px; margin: 8px 0 0 0;">Lembrete de Consulta</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding: 32px;">
                                        <h2 style="color: #1e293b; font-size: 20px; margin: 0 0 16px 0;">Olá, ${paciente.nome}!</h2>
                                        <p style="color: #475569; font-size: 15px; line-height: 1.6; margin: 0 0 24px 0;">
                                            ${reminderText}
                                        </p>
                                        <table width="100%" cellpadding="12" cellspacing="0" style="background: #f8fafc; border-radius: 12px; margin-bottom: 24px;">
                                            <tr>
                                                <td style="padding: 8px 16px; color: #64748b; font-size: 13px; width: 100px;">Médico</td>
                                                <td style="padding: 8px 16px; color: #1e293b; font-size: 14px; font-weight: 600;">${medico.nome} (${medico.especialidade || 'Clínico Geral'})</td>
                                            </tr>
                                            <tr>
                                                <td style="padding: 8px 16px; color: #64748b; font-size: 13px;">Data</td>
                                                <td style="padding: 8px 16px; color: #1e293b; font-size: 14px; font-weight: 600;">${dataFormatada}</td>
                                            </tr>
                                            <tr>
                                                <td style="padding: 8px 16px; color: #64748b; font-size: 13px;">Horário</td>
                                                <td style="padding: 8px 16px; color: #1e293b; font-size: 14px; font-weight: 600;">${consulta.hora}</td>
                                            </tr>
                                            <tr>
                                                <td style="padding: 8px 16px; color: #64748b; font-size: 13px;">Observações</td>
                                                <td style="padding: 8px 16px; color: #1e293b; font-size: 14px;">${consulta.observacoes || '—'}</td>
                                            </tr>
                                        </table>
                                        <p style="color: #94a3b8; font-size: 13px; margin: 0;">
                                            Por favor, chegue com 15 minutos de antecedência.
                                            Em caso de imprevistos, cancele ou reagende pelo sistema.
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="background: #f8fafc; padding: 20px 32px; text-align: center;">
                                        <p style="color: #94a3b8; font-size: 12px; margin: 0;">
                                            © ${new Date().getFullYear()} MedCloud. Todos os direitos reservados.
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
        `;
        const text = `Olá, ${paciente.nome}!\n\n${reminderText}\n\nMédico: ${medico.nome} (${medico.especialidade || 'Clínico Geral'})\nData: ${dataFormatada}\nHorário: ${consulta.hora}\nObservações: ${consulta.observacoes || '—'}\n\nPor favor, chegue com 15 minutos de antecedência.`;
        return { html, text };
    }

    // ── Appointment Confirmation Email ──
    function buildAppointmentConfirmationEmail(consulta, paciente, medico) {
        const dataFormatada = new Date(consulta.data + 'T12:00:00').toLocaleDateString('pt-BR', {
            day: 'numeric', month: 'long', year: 'numeric',
        });

        const protocolo = consulta.id || ('PROTO-' + Date.now().toString(36).toUpperCase());

        const html = `
            <!DOCTYPE html>
            <html>
            <head><meta charset="utf-8"></head>
            <body style="font-family: 'Inter', Arial, sans-serif; background: #f4f7fc; margin: 0; padding: 0;">
                <table width="100%" cellpadding="0" cellspacing="0" style="background: #f4f7fc; padding: 40px 20px;">
                    <tr>
                        <td align="center">
                            <table width="600" cellpadding="0" cellspacing="0" style="background: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 24px rgba(0,0,0,0.08);">
                                <!-- Header -->
                                <tr>
                                    <td style="background: linear-gradient(135deg, #0D9488, #0F766E); padding: 32px; text-align: center;">
                                        <h1 style="color: #ffffff; font-size: 24px; margin: 0;">🏥 MedCloud</h1>
                                        <p style="color: rgba(255,255,255,0.85); font-size: 14px; margin: 8px 0 0 0;">Confirmação de Consulta Agendada</p>
                                    </td>
                                </tr>
                                <!-- Body -->
                                <tr>
                                    <td style="padding: 32px;">
                                        <h2 style="color: #1e293b; font-size: 20px; margin: 0 0 16px 0;">Olá!</h2>
                                        <p style="color: #475569; font-size: 15px; line-height: 1.6; margin: 0 0 24px 0;">
                                            Sua consulta foi agendada com sucesso.
                                        </p>
                                        <table width="100%" cellpadding="12" cellspacing="0" style="background: #f8fafc; border-radius: 12px; margin-bottom: 24px;">
                                            <tr>
                                                <td style="padding: 8px 16px; color: #64748b; font-size: 13px; width: 140px;">Paciente</td>
                                                <td style="padding: 8px 16px; color: #1e293b; font-size: 14px; font-weight: 600;">${paciente.nome}</td>
                                            </tr>
                                            <tr>
                                                <td style="padding: 8px 16px; color: #64748b; font-size: 13px;">Médico</td>
                                                <td style="padding: 8px 16px; color: #1e293b; font-size: 14px; font-weight: 600;">${medico.nome}</td>
                                            </tr>
                                            <tr>
                                                <td style="padding: 8px 16px; color: #64748b; font-size: 13px;">Especialidade</td>
                                                <td style="padding: 8px 16px; color: #1e293b; font-size: 14px; font-weight: 600;">${medico.especialidade || 'Clínico Geral'}</td>
                                            </tr>
                                            <tr>
                                                <td style="padding: 8px 16px; color: #64748b; font-size: 13px;">Data</td>
                                                <td style="padding: 8px 16px; color: #1e293b; font-size: 14px; font-weight: 600;">${dataFormatada}</td>
                                            </tr>
                                            <tr>
                                                <td style="padding: 8px 16px; color: #64748b; font-size: 13px;">Horário</td>
                                                <td style="padding: 8px 16px; color: #1e293b; font-size: 14px; font-weight: 600;">${consulta.hora}</td>
                                            </tr>
                                            <tr>
                                                <td style="padding: 8px 16px; color: #64748b; font-size: 13px;">Protocolo</td>
                                                <td style="padding: 8px 16px; color: #1e293b; font-size: 14px; font-weight: 600;">${protocolo}</td>
                                            </tr>
                                        </table>
                                        <p style="color: #94a3b8; font-size: 13px; line-height: 1.5; margin: 0 0 16px 0;">
                                            Em caso de dúvidas, entre em contato com a clínica.
                                        </p>
                                        <p style="color: #475569; font-size: 14px; margin: 0;">
                                            Atenciosamente,<br>
                                            <strong>Equipe de Agendamento Médico</strong>
                                        </p>
                                    </td>
                                </tr>
                                <!-- Footer -->
                                <tr>
                                    <td style="background: #f8fafc; padding: 20px 32px; text-align: center;">
                                        <p style="color: #94a3b8; font-size: 12px; margin: 0;">
                                            © ${new Date().getFullYear()} MedCloud. Todos os direitos reservados.
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
        `;
        const text = `Olá!\n\nSua consulta foi agendada com sucesso.\n\nDetalhes da consulta:\n\n* Paciente: ${paciente.nome}\n* Médico: ${medico.nome}\n* Especialidade: ${medico.especialidade || 'Clínico Geral'}\n* Data: ${dataFormatada}\n* Horário: ${consulta.hora}\n\nProtocolo: ${protocolo}\n\nEm caso de dúvidas, entre em contato com a clínica.\n\nAtenciosamente,\nEquipe de Agendamento Médico`;
        return { html, text };
    }

    // ── Public API ──
    return {
        sendEmail,
        buildPasswordResetEmail,
        buildAppointmentReminderEmail,
        buildAppointmentConfirmationEmail,
        getConfig() { return { ...CONFIG }; },

        // Update config at runtime
        updateConfig(updates) {
            Object.keys(updates).forEach(key => {
                if (key in CONFIG) {
                    CONFIG[key] = updates[key];
                    localStorage.setItem(key.toUpperCase(), String(updates[key]));
                }
            });
        },

        // Test email configuration
        async testConfig(to) {
            const testHtml = `
                <h2>🔧 Teste de Configuração MedCloud</h2>
                <p>Este é um e-mail de teste para verificar a configuração de envio.</p>
                <p>Provedor: <strong>${CONFIG.emailProvider}</strong></p>
                <p>Enviado em: ${new Date().toLocaleString('pt-BR')}</p>
            `;
            return await this.sendEmail(to, '[MedCloud] Teste de Configuração', testHtml);
        },
    };
})();
