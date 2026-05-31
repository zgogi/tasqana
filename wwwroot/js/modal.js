

class AbstractModalDialog {
    constructor(store, container, command) {
        this.container = container;
        this.store = store;
        this.command = command;
        this._btnSave = container.querySelector(".btn-confirm");
        this._btnCancel = container.querySelector(".btn-cancel");

        this._btnSave.addEventListener("click", () => this.store.confirmEditing(this.formToData()));
        this._btnCancel.addEventListener("click", () => this.store.cancelEditing());

        this.store.subscribe(() => this.update());
    }

    update() {
        const target = this.store.editTarget;

        if (target && target.command === this.command) {
            this.dataToForm(target.id);
            this.container.classList.add('w3-show');
        } else {
            this.container.classList.remove("w3-show");
        }
    }
}

class CategoryCreateModalDialog extends AbstractModalDialog {
    constructor(store, container) {
        super(store, container, "category-create");
        this.title = container.querySelector(".value-title");
        this.parentId = null;
    }

    dataToForm(id) {
        this.parentId = id;
        this.title.value = "";
    }

    formToData() {
        return {
            command: this.command,
            title: this.title.value,
            parent_id: this.parentId
        };
    }
}

class CategoryEditModalDialog extends AbstractModalDialog {
    constructor(store, container) {
        super(store, container, "category-edit");
        this.title = container.querySelector(".value-title");
        this.id = null;
    }

    dataToForm(id) {
        this.id = id;
        this.title.value = this.store.categories.get(id).title;
    }

    formToData() {
        return {
            command: this.command,
            title: this.title.value,
            id: this.id
        };
    }
}

class CategoryDeleteModalDialog extends AbstractModalDialog {
    constructor(store, container) {
        super(store, container, "category-delete");
        this.id = null;
    }

    dataToForm(id) {
        this.id = id;
    }

    formToData() {
        return {
            command: this.command,
            id: this.id
        };
    }
}

class TodoCreateModalDialog extends AbstractModalDialog {
    constructor(store, container) {
        super(store, container, "todo-create");
        this.title = container.querySelector(".value-title");
        this.description = container.querySelector(".value-description");
        this.categoryId = null;
    }

    dataToForm(id) {
        this.categoryId = id;
        this.title.value = "";
        this.description.value = "";
    }

    formToData() {
        return {
            command: this.command,
            title: this.title.value,
            description: this.description.value,
            category_id: this.categoryId
        };
    }
}

class TodoEditModalDialog extends AbstractModalDialog {
    constructor(store, container) {
        super(store, container, "todo-edit");
        this.title = container.querySelector(".value-title");
        this.description = container.querySelector(".value-description");
        this.id = null;
    }

    dataToForm(id) {
        const todo = this.store.todos.get(id);
        this.id = id;
        this.title.value = todo.title;
        this.description.value = todo.description;
    }

    formToData() {
        return {
            command: this.command,
            title: this.title.value,
            description: this.description.value,
            id: this.id
        };
    }
}

class TodoDeleteModalDialog extends AbstractModalDialog {
    constructor(store, container) {
        super(store, container, "todo-delete");
        this.id = null;
    }

    dataToForm(id) {
        this.id = id;
    }

    formToData() {
        return {
            command: this.command,
            id: this.id
        };
    }
}