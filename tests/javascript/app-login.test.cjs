const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

function setup(post) {
    let now = 0;
    const timers = new Map();
    let timerId = 0;
    const rendered = [];
    const errors = [];
    function element() {
        const classes = new Set();
        return {
            hidden: true, textContent: '', disabled: false, children: [], attributes: {}, listeners: {},
            classList: { add: value => classes.add(value), remove: value => classes.delete(value) },
            addEventListener(name, callback) { this.listeners[name] = callback; },
            setAttribute(name, value) { this.attributes[name] = value; },
            removeAttribute(name) { delete this.attributes[name]; },
            replaceChildren() { this.children = []; },
            focus() { this.focused = true; }
        };
    }
    const qr = element(), status = element(), close = element(), container = element(), button = element();
    container.querySelector = selector => ({ '.app-login-qr': qr, '.app-login-status': status, '.btn-cancel': close })[selector];
    function QRCode(target, options) {
        rendered.push(options.text);
        target.children.push(options.text);
        target.setAttribute('title', options.text);
    }
    QRCode.CorrectLevel = { M: 0 };
    const context = vm.createContext({
        QRCode, performance: { now: () => now },
        html: { showError: message => errors.push(message) },
        document: { addEventListener() {} },
        window: {
            setTimeout(callback, delay) { const id = ++timerId; timers.set(id, { callback, delay }); return id; },
            setInterval(callback, delay) { const id = ++timerId; timers.set(id, { callback, delay }); return id; },
            clearTimeout: id => timers.delete(id), clearInterval: id => timers.delete(id)
        }
    });
    vm.runInContext(fs.readFileSync(path.join(__dirname, '../../wwwroot/js/app-login.js'), 'utf8'), context);
    const Dialog = vm.runInContext('AppLoginDialog', context);
    const dialog = new Dialog({ post }, container, button);
    return { dialog, qr, status, button, container, rendered, timers, errors, setTime: value => now = value };
}

test('requests a separate token, renders it, and clears the modal after one minute', async () => {
    const state = setup(async endpoint => {
        assert.equal(endpoint, '/users/token/app');
        return { token: 'test-token', expires_in: 60 };
    });
    await state.dialog.show();
    assert.deepEqual(state.rendered, ['test-token']);
    assert.equal(state.qr.hidden, false);
    assert.equal(state.qr.attributes.title, undefined);
    assert.equal(state.status.textContent, 'Code expires in 60 seconds.');
    const timeout = [...state.timers.values()].find(timer => timer.delay === 60000);
    state.setTime(60000);
    timeout.callback();
    assert.equal(state.dialog.isOpen, false);
    assert.equal(state.qr.hidden, true);
    assert.deepEqual(state.qr.children, []);
    assert.equal(state.timers.size, 0);
    assert.equal(state.button.disabled, false);
});

test('network delay does not extend the token display lifetime', async () => {
    let resolve;
    const state = setup(() => new Promise(done => resolve = done));
    const pending = state.dialog.show();
    state.setTime(15000);
    resolve({ token: 'test-token', expires_in: 60 });
    await pending;
    assert.equal(state.status.textContent, 'Code expires in 45 seconds.');
});

test('expired responses never display a QR code', async () => {
    let resolve;
    const state = setup(() => new Promise(done => resolve = done));
    const pending = state.dialog.show();
    state.setTime(61000);
    resolve({ token: 'expired-token', expires_in: 60 });
    await pending;
    assert.deepEqual(state.rendered, []);
    assert.equal(state.dialog.isOpen, false);
});

test('closing and reopening ignores the older pending request', async () => {
    const responses = [];
    const state = setup(() => new Promise(done => responses.push(done)));
    const first = state.dialog.show();
    state.dialog.hide();
    const second = state.dialog.show();
    responses[0]({ token: 'old-token', expires_in: 60 });
    await first;
    assert.deepEqual(state.rendered, []);
    responses[1]({ token: 'new-token', expires_in: 60 });
    await second;
    assert.deepEqual(state.rendered, ['new-token']);
});

test('repeated clicks do not create extra sessions', async () => {
    let calls = 0;
    const state = setup(async () => { calls++; return { token: 'token', expires_in: 60 }; });
    await Promise.all([state.dialog.show(), state.dialog.show()]);
    assert.equal(calls, 1);
});

test('failed request closes the modal and allows retry', async () => {
    const state = setup(async () => { throw new Error('Network failure'); });
    await state.dialog.show();
    assert.equal(state.dialog.isOpen, false);
    assert.equal(state.button.disabled, false);
    assert.equal(state.timers.size, 0);
    assert.equal(state.errors.length, 1);
});
