
class TodoFilter {
    constructor(src = {}) {
        this.category = src.category ?? null;
        this.priority = src.priority ?? null;
        this.state = src.state ?? null;
    }

    toQuery() {
        const result = [];
        if (this.category != null) result.push(`category_id=${this.category.id}`);
        if (this.priority != null) result.push(`priority=${this.priority}`);
        if (this.state != null) result.push(`state=${this.state}`);
        return result.join('&');
    }
}


class TodosStore {
    constructor(parent, api) {
        this.parent = parent;
        this.api = api;
        this.filter = new TodoFilter();
        this.items = [];
    }

    setFilter(filter) {
        this.filter = new TodoFilter(filter);
        this.update(true);
    }

    update(rebuild=false) {
        const filter = this.filter.toQuery();
       // console.log(filter);
        this.api.get(`/todos/list?${filter}`)
            .then(resp => {
                this.items = resp;
                this.render(rebuild);
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
                this.update();
                this.parent.categories.update();
            });
    }

    save(data, rebuild) {
        this.api.post(`/todos/update`, data)
            .then(resp => {
                if (rebuild) {
                    this.update(rebuild);
                    this.parent.categories.update(rebuild);
                } else {
                    this._setItem(resp);
                }
                
            });
    }

    delete(data) {
        this.api.post(`/todos/delete`, data)
            .then(resp => {
                this.update(true);
                this.parent.categories.update();
            });
    }

    moveToCategory(itemId, categoryId) {
        this.save({ id: itemId, category_id: categoryId }, true);
        //this.parent.categories.select(categoryId);
    }

    move(itemId, beforeId) {
        this.api.post(`/todos/move`, { id:itemId, before_id: beforeId })
            .then(resp => {
                this.update(true);
            });
    }

    checkItemToggle(checkId) {
        this.api.post(`/todos/checklist/toggle`, { id: checkId })
            .then(resp => { this.update(); });
    }

    checkItemCreate(todoId, title) {
        this.api.post(`/todos/checklist/create`, { todo_id: todoId, title: title })
            .then(resp => { this.update(); });
    }

    checkItemSave(id, title) {
        this.api.post(`/todos/checklist/update`, { id: id, title: title })
            .then(resp => { this.update(); });
    }

    checkItemDelete(id) {
        this.api.post(`/todos/checklist/delete`, { id: id })
            .then(resp => { this.update(); });
    }

    render(clear = false) {
        if (clear)
            document.getElementById('todos-list').innerHTML = '';
        this.items.forEach(todo => this._renderItem(todo));
    }

    _setItem(item) {
        for (var i = 0; i < this.items.length; ++i) {
            if (this.items[i].id != item.id) continue;
            this.items[i] = item;
            break;
        }
        this._renderItem(item);
    }

    _renderItem(item) {
        const container = document.getElementById('todos-list');
        var node = container.querySelector(`.todo-node[data-id="${item.id}"]`);
        const isNew = !node;

        if (isNew) {
            node = document.createElement("div");
            node.dataset.id = item.id;
           // node.draggable = true;
            node.className = "todo-node accordion w3-bar-item";
            node.innerHTML = `
                <div class="todo-item w3-block w3-theme-d4 w3-flex w3-padding" style="align-items:center; gap:8px;" draggable="true">
                    ${this._renderCategory(item)}
                    <div class="todo-started fa fa-toggle-right w3-text-yellow w3-hide"></div>
                    <div class="todo-completed fa fa-check-square w3-text-green w3-hide"></div>
                    <div class="todo-title accordion-click w3-block w3-left-align z-clickable"></div>
                    <div class="btn-todo-edit w3-btn fa fa-edit" data-id="${item.id}"></div>
                </div>
                <div class="accordion-content w3-bar-item w3-theme-d2 w3-padding w3-hide">
                    <div class="todo-description"></div>
                    <div class="todo-media"></div>
                    <div class="todo-checkitems"></div>
                </div>
                `;
        }

        const checkItems = item.check_items.reduce((acc, curr) => acc + `
        <div class="w3-flex" style="align-items:center;">
            <div class="check-mark z-clickable ${this._checkBox(curr.is_completed)}" data-id="${curr.id}"></div>
            <div class="w3-block w3-margin-left">${curr.title}</div>
        </div>`, '');

        const media = item.media.reduce((acc, curr) => acc + `
        <div>
            <img src="${curr.url}" class="w3-image w3-round-xlarge">
        </div>`, '');

        const title = node.querySelector('.todo-title');

        title.innerText = item.title;
        node.querySelector('.todo-description').innerText = item.description;
        node.querySelector('.todo-checkitems').innerHTML = checkItems;
        node.querySelector('.todo-media').innerHTML = media;

        html.setClass(title, item.priority == 0, 'priority-0');
        html.setClass(title, item.priority == 1, 'priority-1');
        html.setClass(title, item.priority == 2, 'priority-2');
        html.setClass(title, item.priority == 3, 'priority-3');
        html.setClass(title, item.priority == 4, 'priority-4');

        html.setVisible(node.querySelector('.todo-started'), item.state == 1);
        html.setVisible(node.querySelector('.todo-completed'), item.state == 2);

        if (isNew) {
            container.append(node);
        }
    }

    _checkBox(value) {
        if (value)
            return 'fa fa-check-square-o';
        else
            return 'fa fa-square-o';
    }

    _renderCategory(item) {
        if (this.filter.category == null) {
            const cat = this.parent.categories.get(item.category_id);
            if (cat == null) return '';
            return `<div class="todo-category">${cat.title}</div>`;
        } else {
            return '';
        }
    }

}



