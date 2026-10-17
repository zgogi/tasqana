class AppLoginDialog {
    constructor(api, container, button) {
        this.api = api;
        this.container = container;
        this.button = button;
        this.qr = container.querySelector('.app-login-qr');
        this.status = container.querySelector('.app-login-status');
        this.closeButton = container.querySelector('.btn-cancel');
        this.requestId = 0;
        this.isOpen = false;
        button.addEventListener('click', () => this.show());
        this.closeButton.addEventListener('click', () => this.hide());
        container.addEventListener('click', event => {
            if (event.target === container) this.hide();
        });
        container.addEventListener('keydown', event => {
            if (event.key === 'Escape') this.hide();
            if (event.key === 'Tab') {
                event.preventDefault();
                this.closeButton.focus();
            }
        });
        document.addEventListener('visibilitychange', () => {
            if (this.isOpen && performance.now() >= this.deadline) this.hide();
        });
    }

    async show() {
        if (this.isOpen) return;
        const requestId = ++this.requestId;
        const startedAt = performance.now();
        this.deadline = startedAt + 60000;
        this.isOpen = true;
        this.button.disabled = true;
        this.container.classList.add('w3-show');
        this.container.setAttribute('aria-hidden', 'false');
        this.status.textContent = 'Creating QR code…';
        this.closeButton.focus();
        this.timeout = window.setTimeout(() => this.hide(), 60000);

        try {
            const data = await this.api.post('/users/token/app', {});
            if (!this.isOpen || requestId !== this.requestId) return;
            if (!data) {
                this.hide();
                return;
            }
            if (!data.token || !Number.isFinite(data.expires_in) || data.expires_in <= 0) {
                throw new Error('Invalid app login response');
            }
            this.deadline = startedAt + Math.min(data.expires_in * 1000, 60000);
            if (performance.now() >= this.deadline) {
                this.hide();
                return;
            }
            new QRCode(this.qr, {
                text: data.token,
                width: 240,
                height: 240,
                correctLevel: QRCode.CorrectLevel.M
            });
            this.qr.removeAttribute('title');
            this.qr.hidden = false;
            const updateCountdown = () => {
                const remaining = Math.ceil((this.deadline - performance.now()) / 1000);
                if (remaining <= 0) this.hide();
                else this.status.textContent = `Code expires in ${remaining} seconds.`;
            };
            updateCountdown();
            this.interval = window.setInterval(updateCountdown, 250);
        } catch {
            if (this.isOpen && requestId === this.requestId) {
                this.hide();
                html.showError('Could not create the app login QR code. Please try again.');
            }
        }
    }

    hide() {
        if (!this.isOpen) return;
        this.isOpen = false;
        ++this.requestId;
        window.clearTimeout(this.timeout);
        window.clearInterval(this.interval);
        this.qr.replaceChildren();
        this.qr.removeAttribute('title');
        this.qr.hidden = true;
        this.status.textContent = '';
        this.container.classList.remove('w3-show');
        this.container.setAttribute('aria-hidden', 'true');
        this.button.disabled = false;
        this.button.focus();
    }
}
