
class TodosView {
	constructor(store, titleContainer, listContainer) {
		this.store = store;
		this.titleContainer = titleContainer;
		this.listContainer = listContainer;
		store.subscribe(() => this.render());
	}

	render() {
		this.titleContainer.innerText = this.store.categories.selected?.title ?? "[Unsorted]";
		this.listContainer.innerHTML = "";
		const items = this.store.todos.items;
		for (var i = 0; i < items.length; ++i) {
			this.listContainer.append(this._createNode(items[i]));
		}
	}

	_createNode(item) {
		const menu = this._createMenu(item);
		const title = this._createTitle(item.title, menu);

		const node = document.createElement("div");
		node.dataset.id = item.id;
		node.draggable = true;
		node.className = "accordion todo-node w3-bar-item";
		node.append(title);
		if (item.description != null) {
			const content = this._createDescription(item.description, true);
			node.append(content);
		}
		return node;
	}

	_createDescription(description, isHidden) {
		const node = document.createElement("div");
		node.className = "accordion-content w3-bar-item w3-theme-d2 w3-padding";
		if (isHidden) node.classList.add("w3-hide");
		node.innerText = description;
		return node;
	}

	_createMenu(item) {
		const menu = html.createMenu(true);
		menu.append(html.createMenuItem("Complete", "todo-complete", item.id));
		menu.append(html.createMenuItem("Edit", "todo-edit", item.id));
		menu.append(html.createMenuItem("Delete", "todo-delete", item.id));
		return menu;
	}

	_createTitle(text, menu) {
		const iconBtn = document.createElement("div");
		iconBtn.className = "w3-padding w3-theme-d4 w3-dropdown-click fa fa-ellipsis-v";
		iconBtn.append(menu);

		const button = document.createElement("div");
		button.className = "accordion-click w3-btn w3-block w3-theme-d4 w3-left-align w3-padding";
		button.innerText = text;

		const node = document.createElement("div");
		node.className = "w3-flex";
		node.append(button);
		node.append(iconBtn);

		return node;
	}

}


/*
class Todos {
	constructor(titleId, listId) {
		this.titleId = titleId;
		this.listId = listId;
		this.category = null;
	}

	setCategory(category) {
		this.category = category;
		this.update();
	}

	update() {
		const categoryId = this.category?.id ?? null;
		api.get(`/todos/list?category_id=${categoryId}`)
			.then(resp => {
				todos.setItems(resp);
			}).catch(error => {
				html.showError(error.message);
			});
	}


	setItems(items) {
		document.getElementById(this.titleId).innerText = this.category?.title ?? "Unsorted";
		const list = document.getElementById(this.listId);
		list.innerHTML = "";
		for (var i = 0; i < items.length; ++i) {
			list.append(this._createNode(items[i]));
		}
	}

	showCreateForm() {
		html.showModal("form-todo-add", this.category);
	}

	showEditForm(item) {
		html.setValue("form-todo-edit-title", item.title);
		html.setValue("form-todo-edit-description", item.description);
		html.showModal("form-todo-edit", item);
	}

	showDeleteForm(item) {
		html.showModal("form-todo-delete", item);
	}

	createItem(form) {
		const elTitle = document.getElementById("form-todo-add-title");
		const elDesc = document.getElementById("form-todo-add-description");
		const category = html.hideModal(form);
		api.post("/todos/create", {
			category_id: category?.id ?? null,
			title: elTitle.value,
			description: elDesc.value
		}).then(resp => {
			categories.update();
			this.update();
		}).catch(error => {
			html.showError(error.message);
		});
		elTitle.value = "";
		elDesc.value = "";
	}

	saveItem(form) {
		const elTitle = document.getElementById("form-todo-edit-title");
		const elDesc = document.getElementById("form-todo-edit-description");
		const todo = html.hideModal(form);
		api.post("/todos/update", {
			id: todo.id,
			title: elTitle.value,
			description: elDesc.value
		}).then(resp => {
			this.update();
		}).catch(error => {
			html.showError(error.message);
		});
	}

	deleteItem(form) {
		var todo = html.hideModal(form);
		api.post("/todos/delete", {
			id: todo.id
		}).then(resp => {
			this.update();
		}).catch(error => {
			html.showError(error.message);
		});
	}

	setState(item, state) {
		api.post("/todos/update", {
			id: item.id,
			state: state
		}).then(resp => {
			this.update();
			categories.update();
		}).catch(error => {
			html.showError(error.message);
		});
	}

	moveToCategory(todoId, categoryId) {
		api.post("/todos/update", {
			id: todoId,
			category_id: categoryId
		}).then(resp => {
			this.update();
			categories.update();
		}).catch(error => {
			html.showError(error.message);
		});
	}

	_createNode(item) {
		const menu = this._createMenu(item);
		const title = this._createTitle(item.title, menu);

		const node = document.createElement("div");
		node.dataset.id = item.id;
		node.draggable = true;
		node.className = "accordion todo-node w3-bar-item";
		node.append(title);
		if (item.description != null) {
			const content = this._createDescription(item.description, true);
			node.append(content);
		}
		return node;
	}

	_createDescription(description, isHidden) {
		const node = document.createElement("div");
		node.className = "accordion-content w3-bar-item w3-theme-d2 w3-padding";
		if (isHidden) node.classList.add("w3-hide");
		node.innerText = description;
		return node;
	}

	_createMenu(item) {
		const menu = html.createMenu(true);
		menu.append(html.createMenuItem("Complete", function () { todos.setState(item, 2); }));
		menu.append(html.createMenuItem("Edit", function () { todos.showEditForm(item); }));
		menu.append(html.createMenuItem("Delete", function () { todos.showDeleteForm(item); }));
		return menu;
	}

	_createTitle(text, menu) {
		const iconBtn = document.createElement("div");
		iconBtn.className = "w3-padding w3-theme-d4 w3-dropdown-click fa fa-ellipsis-v";
		iconBtn.append(menu);

		const button = document.createElement("div");
		button.className = "accordion-click w3-btn w3-block w3-theme-d4 w3-left-align w3-padding";
		button.innerText = text;

		const node = document.createElement("div");
		node.className = "w3-flex";
		node.append(button);
		node.append(iconBtn);
		
		return node;
	}
}
const todos = new Todos("todos-title", "todos-list");
*/

