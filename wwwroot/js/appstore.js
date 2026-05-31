
class CategoriesStore {
    constructor(parent, api) {
        this.parent = parent;
        this.api = api;
        this.items = [];
        this.selected = null;
    }

    update() {
        this.api.get(`/categories/tree`)
            .then(resp => {
                this.items = resp;
                this.parent.notify();
            }).catch(error => {
                html.showError(error.message);
            });
    }

    select(id) {
        if (id == null) {
            this.selected = null;
            this.parent.todos.update();
            return;
        }

        this.selected = this.get(id);
        this.parent.todos.update();
    }

    create(data) {
        this.api.post(`/categories/create`, data)
            .then(resp => {
                this.update();
            }).catch(error => {
                html.showError(error.message);
            });
    }

    save(data) {
        this.api.post(`/categories/update`, data)
            .then(resp => {
                this.update();
            }).catch(error => {
                html.showError(error.message);
            });
    }

    delete(data) {
        this.api.post(`/categories/delete`, data)
            .then(resp => {
                this.select(null);
            }).catch(error => {
                html.showError(error.message);
            });
    }

    get(id, items = null) {
        const litems = items ?? this.items;
        for (var i = 0; i < litems.length; ++i) {
            const item = litems[i]
            if (item.id == id) {
                return item;
            }
            const ret = this.get(id, item.sub_categories);
            if (ret != null)
                return ret;
        }
        return null;
    }


}

class TodosStore {
    constructor(parent, api) {
        this.parent = parent;
        this.api = api;
        this.items = [];
    }

    update() {
        const categoryId = this.parent.categories.selected?.id ?? null;
        this.api.get(`/todos/list?category_id=${categoryId}`)
            .then(resp => {
                this.items = resp;
                this.parent.notify();
            }).catch(error => {
                html.showError(error.message);
            });
    }

    get(id) {
        for (var i = 0; i < this.items.length; ++i) {
            if (this.items[i].id == id)
                return this.items[i];
        }
        return null;
    }

    create(data) {
        this.api.post(`/todos/create`, data)
            .then(resp => {
                this.parent.update();
            }).catch(error => {
                html.showError(error.message);
            });
    }

    save(data) {
        this.api.post(`/todos/update`, data)
            .then(resp => {
                this.parent.update();
            }).catch(error => {
                html.showError(error.message);
            });
    }

    delete(data) {
        this.api.post(`/todos/delete`, data)
            .then(resp => {
                this.parent.update();
            }).catch(error => {
                html.showError(error.message);
            });
    }

    moveToCategory(itemId, categoryId) {
        this.save({ id: itemId, category_id: categoryId });
        this.parent.categories.select(categoryId);
    }
}

class UserStore {

    constructor(api) {
        this.api = api;
        this._read();
    }

    update(onSuccess = null) {
        this.api.post('/users/token/update', {})
            .then(data => {
                this._setUser(data);
               // console.log("Token update succeded");
                if (onSuccess != null) onSuccess();
            })
            .catch(error => {
                //console.log("Token update failed", error);
                this.logout();
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
        return ndate - edate;
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
        this.editTarget = null; // Editing now
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
        this.todos.update();
    }

    executeCommand(command, id) {
        if (command == "todo-complete") {
            this.todos.save({ id: id, state: 2 });
        } else {
            this.editTarget = { command: command, id: id };
            this.notify();
        }
    }

    confirmEditing(data) {
        if (data != null) {
            if (data.command === "category-create")
                this.categories.create(data);
            else if (data.command === "category-edit")
                this.categories.save(data);
            else if (data.command === "category-delete")
                this.categories.delete(data);
            else if (data.command === "todo-create")
                this.todos.create(data);
            else if (data.command === "todo-edit")
                this.todos.save(data);
            else if (data.command === "todo-delete")
                this.todos.delete(data);
        }
        this.editTarget = null;
        this.notify();
    }

    cancelEditing() {
        this.editTarget = null;
        this.notify();
    }


}