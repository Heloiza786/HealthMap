/* ============================================
   ⏰ MedCloud — Notification Worker
   Processes pending email notifications
   Runs on page load and at intervals
   ============================================ */

const MedCloudNotificationWorker = (function () {
    let intervalId = null;
    const CHECK_INTERVAL_MS = 60000; // Check every 60 seconds

    // ── Start the worker ──
    function start() {
        if (intervalId) {
            console.log('[WORKER] Notification worker already running');
            return;
        }

        console.log('[WORKER] Starting notification worker...');

        // Process immediately on start
        processPendingNotifications();

        // Then check periodically
        intervalId = setInterval(() => {
            processPendingNotifications();
        }, CHECK_INTERVAL_MS);

        console.log(`[WORKER] Notification worker started (checking every ${CHECK_INTERVAL_MS / 1000}s)`);
    }

    // ── Stop the worker ──
    function stop() {
        if (intervalId) {
            clearInterval(intervalId);
            intervalId = null;
            console.log('[WORKER] Notification worker stopped');
        }
    }

    // ── Process all pending notifications ──
    function processPendingNotifications() {
        try {
            if (typeof MedCloud === 'undefined' || typeof MedCloud.processarNotificacoesPendentes !== 'function') {
                console.warn('[WORKER] MedCloud not available yet');
                return;
            }

            const startTime = Date.now();
            MedCloud.processarNotificacoesPendentes();
            const elapsed = Date.now() - startTime;

            // Log stats
            const data = MedCloud.getRawData ? MedCloud.getRawData() : null;
            if (data && data.emailLog) {
                const pending = data.emailLog.filter(e => e.status === 'pendente').length;
                const sent = data.emailLog.filter(e => e.status === 'enviado').length;
                const errors = data.emailLog.filter(e => e.status === 'erro').length;
                console.log(`[WORKER] Check complete (${elapsed}ms) — ${sent} sent, ${pending} pending, ${errors} errors`);
            }
        } catch (error) {
            console.error('[WORKER] Error processing notifications:', error);
        }
    }

    // ── Get worker status ──
    function getStatus() {
        return {
            running: intervalId !== null,
            intervalMs: CHECK_INTERVAL_MS,
            lastCheck: new Date().toISOString(),
        };
    }

    // ── Auto-start on page load ──
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            // Wait a bit for MedCloud to be fully initialized
            setTimeout(start, 1000);
        });
    } else {
        setTimeout(start, 1000);
    }

    return {
        start,
        stop,
        processPendingNotifications,
        getStatus,
    };
})();
