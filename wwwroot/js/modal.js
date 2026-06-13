

class AbstractModalDialog {
    constructor(store, formId) {
        this.container = document.getElementById(formId);
        this.store = store;

        const btnCancel = this.container.querySelectorAll('.btn-cancel');
        for (var i = 0; i < btnCancel.length; ++i)
            btnCancel[i].addEventListener('click', () => this.hide());

        this.store.subscribe(() => this.update());
    }

    update() {
        const target = this.store.modal;

        if (target && target.modal === this.container.id) {
            this.onShow(target);
            this.container.classList.add('w3-show');
        } else {
            this.container.classList.remove('w3-show');
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
        this._title = this.container.querySelector('.value-title');
        this._btnCreate = this.container.querySelector('.btn-create');
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
        this._title = this.container.querySelector('.value-title');
        this._btnSave = this.container.querySelector('.btn-save');
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
        this._deleteBtn = this.container.querySelector('.btn-delete');
        this._deleteBtn.addEventListener('click', () => {
            this.store.categories.delete({ id: this._id });
            this.hide();
        });
    }

    onShow(data) {
        this._id = data.id;
    }
}

class TodoEditModalDialog extends AbstractModalDialog {
    constructor(store, formId) {
        super(store, formId);
        this._id = null;
        this._categoryId = null;
        this._table = new Table('table-todo-checkitems', [
            { id: 'id', type: 'hidden' },
            { id: 'is_completed', type: 'input-checkbox' },
            { id: 'title', type: 'input-text' },
            { id: 'trash', type: 'button' }
        ], true);

        this._title = this.container.querySelector('.value-title');
        this._description = this.container.querySelector('.value-description');
        this._btnCreate = this.container.querySelector('.btn-create');
        this._btnSave = this.container.querySelector('.btn-save');
        this._btnStart = this.container.querySelector('.btn-start');
        this._btnComplete = this.container.querySelector('.btn-complete');
        this._btnDelete = this.container.querySelector('.btn-delete');
        this._btnCheckListAdd = this.container.querySelector('.btn-checklist-add');
        this._btnCheckListFromText = this.container.querySelector('.btn-checklist-fromtext');

        this._btnCreate.addEventListener('click', () => {
            this.store.todos.create({
                category_id: this._categoryId,
                title: this._title.value,
                description: this._description.value,
                priority: this._priority
            });
            this.hide();
        });

        this._btnStart.addEventListener('click', () => {
            this.store.todos.save({
                id: this._id,
                state: 1
            }, true);
            this.hide();
        });

        this._btnComplete.addEventListener('click', () => {
            this.store.todos.save({
                id: this._id,
                state: 2
            }, true);
            this.hide();
        });

        this._btnDelete.addEventListener('click', () => {
            this.store.todos.delete({
                id: this._id
            });
            this.hide();
        });

        this._btnSave.addEventListener('click', () => {
            const data = this._getItem();
            this.store.todos.save(data, false);
            this.hide();
        });

        this._btnCheckListFromText.addEventListener('click', () => {
            const lines = this._description.value.split('\n');
            for (var i = 0; i < lines.length; ++i) {
                this._table.addRow({title:lines[i].trim()});
            }
            this._description.value = '';
        });

        this.container.querySelector('.btn-priority-0').addEventListener('click', () => {
            this._updatePriority(0);
        });

        this.container.querySelector('.btn-priority-1').addEventListener('click', () => {
            this._updatePriority(1);
        });

        this.container.querySelector('.btn-priority-2').addEventListener('click', () => {
            this._updatePriority(2);
        });

        this.container.querySelector('.btn-priority-3').addEventListener('click', () => {
            this._updatePriority(3);
        });

        this.container.querySelector('.btn-priority-4').addEventListener('click', () => {
            this._updatePriority(4);
        });

        this._btnCheckListAdd.addEventListener('click', () => {
            this._table.addRow();
        });

        this._table.addClickListener((row, id) => {
            if (id == 'trash')
                row.remove();
        });

        

    }

    onShow(data) {
        const todo = this.store.todos.get(data.id);
        this._id = data.id ?? null;
        this._categoryId = data.categoryid ?? null;
        this._title.value = todo?.title ?? '';
        this._description.value = todo?.description ?? '';
        this._updatePriority(todo?.priority ?? 0);

        html.setVisible(this._btnCreate, data.iscreate == true);
        html.setVisible(this._btnSave, this._id != null);
        html.setVisible(this._btnStart, this._id != null && todo.state < 1);
        html.setVisible(this._btnComplete, this._id != null && todo.state < 2);
        html.setVisible(this._btnDelete, this._id != null);

        if (todo != null)
            this._table.rebuild(todo.check_items, false);
        else
            this._table.clear();
    }

    _updatePriority(priority) {
        this._priority = priority;
        for (var i = 0; i < 5; ++i) {
            const elem = this.container.querySelector(`.btn-priority-${i}`);
            html.setClass(elem, i <= priority, 'w3-text-yellow');
        }
    }

    _getItem() {
        return {
            id: this._id,
            title: this._title.value,
            description: this._description.value,
            priority: this._priority,
            check_items: this._table.read()
        }
    }

}

class CheckEditModalDialog extends AbstractModalDialog {
    constructor(store, formId) {
        super(store, formId);
        this._id = null;
        this._todoId = null;
        this._title = this.container.querySelector('.value-title');
        this._btnCreate = this.container.querySelector('.btn-create');
        this._btnSave = this.container.querySelector('.btn-save');
        this._btnDelete = this.container.querySelector('.btn-delete');

        this._btnCreate.addEventListener('click', () => {
            this.store.todos.checkItemCreate(this._todoId, this._title.value);
            this.hide();
        });

        this._btnSave.addEventListener('click', () => {
            this.store.todos.checkItemSave(this._id, this._title.value);
            this.hide();
        });

        this._btnDelete.addEventListener('click', () => {
            this.store.todos.checkItemDelete(this._id);
            this.hide();
        });
    }

    onShow(data) {
        this._id = data.id ?? null;
        this._todoId = data.todoid ?? null;
        this._title.value = (this._id != null) ? this.store.todos.getCheckItem(this._id).title : '';
        html.setVisible(this._btnCreate, this._todoId != null);
        html.setVisible(this._btnSave, this._id != null);
        html.setVisible(this._btnDelete, this._id != null);
    }
}