




class UserStore {

    constructor(api) {
        this.api = api;
        this._read();
    }

    update(onSuccess = null) {
        this.api.post('/users/token/update', {})
            .then(data => {
                this._setUser(data);
                if (onSuccess != null) onSuccess();
            });
    }

    autoUpdate() {
        const day = 1000 * 3600 * 24;
        if (this._getTimeBeforeExpired() < day)
            this.update();
        window.setTimeout( () => {
            if (this._getTimeBeforeExpired() < day)
                this.update();
        }, day);
    }


    setToken(token) {
        localStorage.setItem('token', token);
    }

    logout() {
        this._remove();
        window.location.replace("/login/noauth.html");
    }

    _getTimeBeforeExpired() {
        const ndate = Date.now();
        const edate = this.expired_at.getTime();
        return edate - ndate;
    }

    _setUser(user) {
        localStorage.setItem('token', user.token);
        localStorage.setItem("expired_at", user.expired_at);
        localStorage.setItem('username', user.name);
        localStorage.setItem("is_admin", (user.is_admin) ? "1" : "0");
        this._read();
    }

    _remove() {
        localStorage.removeItem("token");
        localStorage.removeItem("expired_at");
        localStorage.removeItem("username");
        localStorage.removeItem("is_admin");
    }

    _read() {
        this.name = localStorage.getItem('username');
        this.is_admin = (localStorage.getItem("is_admin") === "1");
        this.expired_at = new Date(localStorage.getItem("expired_at"));
    }
}

class AppStore {

    constructor(api) {
        this.api = api;
        this.categories = new CategoriesStore(this, api); 
        this.todos = new TodosStore(this, api);
        this.user = new UserStore(api);
        this.modal = null; // Editing now
        this._listeners = [];

        
    }

    subscribe(listener) {
        this._listeners.push(listener);
    }

    notify() {
        this._listeners.forEach(listener => listener());
    }

    update() {
        this.categories.update();
        this.categories.select({});
        //this.todos.setFilter({});
    }

    startEdit(target) {
        this.modal = target;
        this.notify();
    }

    




}