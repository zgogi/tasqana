

class AbstractModalDialog {
    constructor(store, formId) {
        this.container = document.getElementById(formId);
        this.store = store;
        this._btnCancel = this.container.querySelector(".btn-cancel");
        this._btnCancel.addEventListener("click", () => this.hide());

        this.store.subscribe(() => this.update());
    }

    update() {
        const target = this.store.modal;

        if (target && target.modal === this.container.id) {
            this.onShow(target);
            this.container.classList.add('w3-show');
        } else {
            this.container.classList.remove("w3-show");
        }
    }

    hide() {
        this.store.modal = null;
        this.store.notify();
    }
}

class CategoryCreateModalDialog extends AbstractModalDialog {
    constructor(store, formId) {
        super(store, formId);
        this._parentId = null;
        this._title = this.container.querySelector(".value-title");
        this._btnCreate = this.container.querySelector(".btn-create");
        this._btnCreate.addEventListener('click', () => {
            this.store.categories.create({ parent_id: this._parentId, title: this._title.value });
            this.hide();
        });
    }

    onShow(data) {
        this._parentId = data.id;
        this._title.value = "";
    }
}

class CategoryEditModalDialog extends AbstractModalDialog {
    constructor(store, formId) {
        super(store, formId);
        this._id = null;
        this._title = this.container.querySelector(".value-title");
        this._btnSave = this.container.querySelector(".btn-save");
        this._btnSave.addEventListener('click', () => {
            this.store.categories.save({ id: this._id, title: this._title.value });
            this.hide();
        });
    }

    onShow(data) {
        this._id = data.id;
        this._title.value = this.store.categories.get(data.id).title;
    }
}

class CategoryDeleteModalDialog extends AbstractModalDialog {
    constructor(store, formId) {
        super(store, formId);
        this._id = null;
        this._deleteBtn = this.container.querySelector(".btn-delete");
        this._deleteBtn.addEventListener('click', () => {
            this.store.categories.delete({ id: this._id });
            this.hide();
        });
    }

    onShow(data) {
        this._id = data.id;
    }
}

class TodoCreateModalDialog extends AbstractModalDialog {
    constructor(store, formId) {
        super(store, formId);
        this._categoryId = null;
        this._title = this.container.querySelector(".value-title");
        this._description = this.container.querySelector(".value-description");
        this._btnCreate = this.container.querySelector(".btn-create");
        this._btnCreate.addEventListener('click', () => {
            this.store.todos.create({ category_id: this._categoryId, title: this._title.value, description: this._description.value });
            this.hide();
        });
    }

    onShow(data) {
        this._categoryId = data.id;
        this._title.value = "";
        this._description.value = "";
    }
}

class TodoEditModalDialog extends AbstractModalDialog {
    constructor(store, formId) {
        super(store, formId);
        this._id = null;
        this._title = this.container.querySelector(".value-title");
        this._description = this.container.querySelector(".value-description");
        this._btnSave = this.container.querySelector(".btn-save");
        this._btnSave.addEventListener('click', () => {
            this.store.todos.save({ id: this._id, title: this._title.value, description: this._description.value });
            this.hide();
        });
    }

    onShow(data) {
        const todo = this.store.todos.get(data.id);
        this._id = data.id;
        this._title.value = todo.title;
        this._description.value = todo.description;
    }

}

class TodoDeleteModalDialog extends AbstractModalDialog {
    constructor(store, formId) {
        super(store, formId);
        this._id = null;
        this._deleteBtn = this.container.querySelector(".btn-delete");
        this._deleteBtn.addEventListener('click', () => {
            this.store.todos.delete({ id: this._id });
            this.hide();
        });
    }

    onShow(data) {
        this._id = data.id;
    }

}

class CheckEditModalDialog extends AbstractModalDialog {
    constructor(store, formId) {
        super(store, formId);
        this._id = null;
        this._todoId = null;
        this._title = this.container.querySelector(".value-title");
        this._btnCreate = this.container.querySelector(".btn-create");
        this._btnSave = this.container.querySelector(".btn-save");
        this._btnDelete = this.container.querySelector(".btn-delete");

        this._btnCreate.addEventListener("click", () => {
            this.store.todos.checkItemCreate(this._todoId, this._title.value);
            this.hide();
        });

        this._btnSave.addEventListener("click", () => {
            this.store.todos.checkItemSave(this._id, this._title.value);
            this.hide();
        });

        this._btnDelete.addEventListener("click", () => {
            this.store.todos.checkItemDelete(this._id);
            this.hide();
        });
    }

    onShow(data) {
        this._id = data.id ?? null;
        this._todoId = data.todoId ?? null;
        this._title.value = (this._id != null) ? this.store.todos.getCheckItem(this._id).title : "";
        html.setVisible(this._btnCreate, this._todoId != null);
        html.setVisible(this._btnSave, this._id != null);
        html.setVisible(this._btnDelete, this._id != null);
    }
}