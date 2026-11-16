/* ============================================
   🖥️ MedCloud — Email Backend Server
   Provides SMTP email sending via REST API
   ============================================ */

const express = require('express');
const nodemailer = require('nodemailer');
const cors = require('cors');
const path = require('path');
require('dotenv').config({ path: path.join(__dirname, '..', '.env') });

const app = express();
const PORT = process.env.BACKEND_PORT || 3000;

// ── Middleware ──
app.use(cors());
app.use(express.json({ limit: '1mb' }));

// ── Request logging ──
app.use((req, res, next) => {
    const start = Date.now();
    res.on('finish', () => {
        const duration = Date.now() - start;
        console.log(`[${new Date().toISOString()}] ${req.method} ${req.path} ${res.statusCode} (${duration}ms)`);
    });
    next();
});

// ── Health Check ──
app.get('/api/health', (req, res) => {
    res.json({
        status: 'ok',
        timestamp: new Date().toISOString(),
        smtpConfigured: !!(process.env.SMTP_HOST && process.env.SMTP_USER && process.env.SMTP_PASS),
    });
});

// ── Send Email ──
app.post('/api/send-email', async (req, res) => {
    try {
        const { to, subject, html, text, from, smtp } = req.body;

        // Validate required fields
        if (!to || !subject) {
            return res.status(400).json({ error: 'Campos obrigatórios: to, subject' });
        }

        // Use provided SMTP config or fallback to env vars
        const smtpConfig = smtp || {};
        const host = smtpConfig.host || process.env.SMTP_HOST;
        const port = parseInt(smtpConfig.port || process.env.SMTP_PORT || '587');
        const user = smtpConfig.user || process.env.SMTP_USER;
        const pass = smtpConfig.pass || process.env.SMTP_PASS;
        const secure = smtpConfig.secure !== undefined ? smtpConfig.secure : (process.env.SMTP_SECURE === 'true');
        const fromEmail = from || process.env.SMTP_FROM || 'noreply@medcloud.com';

        if (!host || !user || !pass) {
            return res.status(400).json({
                error: 'SMTP não configurado. Configure SMTP_HOST, SMTP_USER e SMTP_PASS no .env ou envie smtp config no body.',
            });
        }

        // Create transporter
        const transporter = nodemailer.createTransport({
            host,
            port,
            secure,
            auth: { user, pass },
            tls: {
                rejectUnauthorized: false, // Allow self-signed certs for testing
            },
        });

        // Verify connection
        await transporter.verify();

        // Send mail
        const info = await transporter.sendMail({
            from: `"MedCloud" <${fromEmail}>`,
            to,
            subject: `[MedCloud] ${subject}`,
            text: text || '',
            html: html || text || '',
        });

        console.log(`[EMAIL] Sent to ${to}: ${info.messageId}`);

        res.json({
            success: true,
            messageId: info.messageId,
            accepted: info.accepted,
            rejected: info.rejected,
        });
    } catch (error) {
        console.error(`[EMAIL] Error:`, error.message);
        res.status(500).json({
            success: false,
            error: error.message,
        });
    }
});

// ── SendGrid Proxy ──
app.post('/api/send-sendgrid', async (req, res) => {
    try {
        const { to, subject, html, text, from } = req.body;
        const apiKey = process.env.SENDGRID_API_KEY || req.body.apiKey;

        if (!apiKey) {
            return res.status(400).json({ error: 'SendGrid API Key não configurada.' });
        }

        const response = await fetch('https://api.sendgrid.com/v3/mail/send', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${apiKey}`,
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                personalizations: [{ to: [{ email: to }] }],
                from: { email: from || process.env.SMTP_FROM || 'noreply@medcloud.com', name: 'MedCloud' },
                subject: `[MedCloud] ${subject}`,
                content: [
                    { type: 'text/plain', value: text || html.replace(/<[^>]*>/g, '') },
                    { type: 'text/html', value: html },
                ],
            }),
        });

        if (!response.ok) {
            const err = await response.text();
            throw new Error(`SendGrid error: ${err}`);
        }

        res.json({ success: true, provider: 'sendgrid' });
    } catch (error) {
        res.status(500).json({ success: false, error: error.message });
    }
});

// ── Error Handler ──
app.use((err, req, res, next) => {
    console.error('[SERVER] Unhandled error:', err);
    res.status(500).json({ error: 'Erro interno do servidor' });
});

// ── Start Server ──
app.listen(PORT, '0.0.0.0', () => {
    console.log(`
╔══════════════════════════════════════════╗
║     🏥 MedCloud — Email Server          ║
║──────────────────────────────────────────║
║  Status:  Running                        ║
║  Port:    ${String(PORT).padEnd(33)}║
║  API:     http://localhost:${PORT}/api     ║
║  Health:  http://localhost:${PORT}/api/health ║
╚══════════════════════════════════════════╝
    `);
});
