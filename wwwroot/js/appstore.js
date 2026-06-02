
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
            });
    }

    save(data) {
        this.api.post(`/categories/update`, data)
            .then(resp => {
                this.update();
            });
    }

    delete(data) {
        this.api.post(`/categories/delete`, data)
            .then(resp => {
                this.update();
                this.select(null);
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
            });
    }



    get(id) {
        for (var i = 0; i < this.items.length; ++i) {
            if (this.items[i].id == id)
                return this.items[i];
        }
        return null;
    }

    getCheckItem(id) {
        for (var i = 0; i < this.items.length; ++i) {
            const subitems = this.items[i].check_items;
            for (var j = 0; j < subitems.length; ++j) {
                if (subitems[j].id == id)
                    return subitems[j];
            }
        }
        return null;
    }

    create(data) {
        this.api.post(`/todos/create`, data)
            .then(resp => {
                this.parent.update();
            });
    }

    save(data) {
        this.api.post(`/todos/update`, data)
            .then(resp => {
                this.parent.update();
            });
    }

    delete(data) {
        this.api.post(`/todos/delete`, data)
            .then(resp => {
                this.parent.update();
            });
    }

    moveToCategory(itemId, categoryId) {
        this.save({ id: itemId, category_id: categoryId });
        this.parent.categories.select(categoryId);
    }

    checkItemToggle(checkId) {
        this.api.post(`/todos/checklist/toggle`, { id:checkId })
            .then(resp => {
                this.parent.update();
            });
    }

    checkItemCreate(todoId, title) {
        this.api.post(`/todos/checklist/create`, { todo_id: todoId, title: title })
            .then(resp => {
                this.parent.update();
            });
    }

    checkItemSave(id, title) {
        this.api.post(`/todos/checklist/update`, { id: id, title:title })
            .then(resp => {
                this.parent.update();
            });
    }

    checkItemDelete(id) {
        this.api.post(`/todos/checklist/delete`, { id: id })
            .then(resp => {
                this.parent.update();
            });
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
        this.todos.update();
    }

    startEdit(target) {
        this.modal = target;
        this.notify();
    }

    




}