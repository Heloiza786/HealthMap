/* ============================================
   🏥 MedCloud — Shared Application Logic
   ============================================ */

// ── Sidebar Toggle ──
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');
    if (sidebar) sidebar.classList.toggle('open');
    if (overlay) overlay.classList.toggle('active');
}

// ── Theme ──
function toggleTheme() {
    const html = document.documentElement;
    const current = html.getAttribute('data-theme');
    html.setAttribute('data-theme', current === 'dark' ? 'light' : 'dark');
    localStorage.setItem('theme', html.getAttribute('data-theme'));
}

function loadTheme() {
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme) {
        document.documentElement.setAttribute('data-theme', savedTheme);
    }
}

// ── Auth Guard ──
function requireAuth(tipo) {
    const user = MedCloud.getCurrentUser();
    if (!user) {
        window.location.href = '../LoginInicial/LoginInicial.html';
        return null;
    }
    if (tipo && user.tipo !== tipo) {
        // Redirect to appropriate dashboard
        if (user.tipo === 'admin') {
            window.location.href = '../DashboardAdmin/DashboardAdmin.html';
        } else if (user.tipo === 'medico') {
            window.location.href = '../Dashboard/Dashboard.html';
        } else {
            window.location.href = '../DashboardPac/DashboardPac.html';
        }
        return null;
    }
    return user;
}

// ── Toast Notifications ──
function showToast(message, type = 'success') {
    const container = document.getElementById('toastContainer');
    if (!container) {
        // Create container if not exists
        const div = document.createElement('div');
        div.id = 'toastContainer';
        div.className = 'toast-container';
        document.body.appendChild(div);
    }

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `
        <span class="toast-icon">${type === 'success' ? '✅' : type === 'error' ? '❌' : type === 'warning' ? '⚠️' : 'ℹ️'}</span>
        <span class="toast-message">${message}</span>
    `;

    const container2 = document.getElementById('toastContainer');
    container2.appendChild(toast);

    setTimeout(() => {
        toast.classList.add('toast-exit');
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}

// ── Format Helpers ──
function formatDate(dateStr) {
    if (!dateStr) return '';
    const [y, m, d] = dateStr.split('-');
    return `${d}/${m}/${y}`;
}

function formatDateLong(dateStr) {
    if (!dateStr) return '';
    const d = new Date(dateStr + 'T12:00:00');
    return d.toLocaleDateString('pt-BR', {
        day: 'numeric',
        month: 'long',
        year: 'numeric',
    });
}

function getStatusLabel(status) {
    const map = {
        pendente: 'Pendente',
        confirmada: 'Confirmada',
        concluida: 'Concluída',
        cancelada: 'Cancelada',
        reagendada: 'Reagendada',
    };
    return map[status] || status;
}

function getStatusBadgeClass(status) {
    const map = {
        pendente: 'badge-warning',
        confirmada: 'badge-success',
        concluida: 'badge-info',
        cancelada: 'badge-error',
        reagendada: 'badge-info',
    };
    return map[status] || 'badge-info';
}

// ── Modal ──
function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.add('open');
        modal.querySelector('.modal-backdrop')?.classList.add('active');
    }
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.remove('open');
        modal.querySelector('.modal-backdrop')?.classList.remove('active');
    }
}

// ── Loading State ──
function showLoading(containerId, message = 'Carregando...') {
    const container = document.getElementById(containerId);
    if (!container) return;
    container.innerHTML = `
        <div class="loading-state">
            <div class="spinner"></div>
            <p class="text-muted">${message}</p>
        </div>
    `;
}

// ── Init on page load ──
document.addEventListener('DOMContentLoaded', () => {
    loadTheme();

    // Set current year in footers
    document.querySelectorAll('.current-year').forEach(el => {
        el.textContent = new Date().getFullYear();
    });
});
