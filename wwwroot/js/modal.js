

class AbstractModalDialog {
    constructor(store, formId) {
        this.container = document.getElementById(formId);
        this.store = store;

        const btnCancel = this.container.querySelectorAll('.btn-cancel');
        for (var i = 0; i < btnCancel.length; ++i)
            btnCancel[i].addEventListener('click', () => this.hide());
    }

    show(data) {
        this.onShow(data);
        this.container.classList.add('w3-show');
    }

    hide() {
        this.container.classList.remove('w3-show');
    }
}

class CategoryEditModalDialog extends AbstractModalDialog {
    constructor(store, formId) {
        super(store, formId);
        this._parentId = null;
        this._id = null;
        this._title = this.container.querySelector('.value-title');
        this._btnCreate = this.container.querySelector('.btn-create');
        this._btnSave = this.container.querySelector('.btn-save');

        this._btnCreate.addEventListener('click', () => {
            this.store.categories.create({ parent_id: this._parentId, title: this._title.value });
            this.hide();
        });

        this._btnSave.addEventListener('click', () => {
            this.store.categories.save({ id: this._id, title: this._title.value });
            this.hide();
        });
    }

    onShow(data) {
        this._parentId = data.parentId ?? null;
        this._id = data.id ?? null;
        this._title.value = this.store.categories.get(data.id)?.title ?? '';
        html.setVisible(this._btnCreate, this._id == null);
        html.setVisible(this._btnSave, this._id != null);
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
        this._btnStop = this.container.querySelector('.btn-stop');
        this._btnComplete = this.container.querySelector('.btn-complete');
        this._btnDelete = this.container.querySelector('.btn-delete');
        this._btnCheckListAdd = this.container.querySelector('.btn-checklist-add');
        this._btnCheckListFromText = this.container.querySelector('.btn-checklist-fromtext');

        this._btnCreate.addEventListener('click', () => {
            const item = this._getItem();
            this.store.todos.create(item);
            this.hide();
        });

        this._btnStart.addEventListener('click', () => {
            const item = this._getItem();
            item.state = 1;
            this.store.todos.save(item, true);
            this.hide();
        });

        this._btnStop.addEventListener('click', () => {
            const item = this._getItem();
            item.state = 0;
            this.store.todos.save(item, true);
            this.hide();
        });

        this._btnComplete.addEventListener('click', () => {
            const item = this._getItem();
            item.state = 2;
            this.store.todos.save(item, true);
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
                const text = lines[i].trim();
                if (text === '') continue;
                this._table.addRow({title:text});
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
        this._categoryId = data.categoryid ?? this.store.todos.filter?.category?.id ?? null;
        this._title.value = todo?.title ?? '';
        this._description.value = todo?.description ?? '';
        this._updatePriority(todo?.priority ?? 0);
        html.setVisible(this._btnCreate, this._id == null);
        html.setVisible(this._btnSave, this._id != null);
        html.setVisible(this._btnStart, this._id != null && todo.state < 1);
        html.setVisible(this._btnStop, this._id != null && todo.state > 0);
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
            category_id: this._categoryId,
            priority: this._priority,
            check_items: this._table.read()
        }
    }

}

class ImageModalDialog extends AbstractModalDialog {
    constructor(store, formId) {
        super(store, formId);
        this._image = this.container.querySelector(".content")
    }

    onShow(data) {
        this._image.src = data.src;
    }
}
class Modal {

    constructor(store) {
        this.categoryEdit = new CategoryEditModalDialog(store, 'form-cat-edit');
        this.categoryDelete = new CategoryDeleteModalDialog(store, 'form-cat-delete');
        this.todoEdit = new TodoEditModalDialog(store, 'form-todo-edit');
        this.image = new ImageModalDialog(store, "form-image");
    }

}