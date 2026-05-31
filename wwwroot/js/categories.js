

class CategoriesView {
	constructor(store, container) {
		this.store = store;
		this.container = container;
		this.store.subscribe(() => this.render());
	}

	render() {
		this.container.innerHTML = "";
		const items = this.store.categories.items;
		for (var i = 0; i < items.length; ++i) {
			const item = this._createNode(items[i]);
			this.container.append(item);
		}
	}

	showAddForm(parentId = null) {
		html.showModal("form-cat-add", parentId);
	}

	showEditForm(item) {
		html.setValue("form-cat-edit-title", item.title);
		html.showModal("form-cat-edit", item);
	}

	showDeleteForm(item) {
		html.showModal("form-cat-delete", item);
	}

	_createNode(item) {
		const menu = this._createMenu(item);
		const title = this._createTitleBar(item, menu);
		const content = this._createSubNodes(item.sub_categories);

		const node = document.createElement("div");
		node.dataset.id = item.id;
		node.className = "category-item";
		node.append(title);
		node.append(content);
		return node;

	}

	_createTitleBar(item, menu) {
		const titleNode = document.createElement("span");
		titleNode.innerText = item.title;
		titleNode.dataset.id = item.id;
		titleNode.className = "category-title z-clickable w3-padding";
		titleNode.draggable = true;
		if (item.id == this.store.categories.selected?.id) {
			titleNode.classList.add("selected");
		}

		const badge = this._createBadge(item);

		const node = document.createElement("div");
		node.className = "w3-bar";
		node.append(titleNode);
		node.append(menu);
		if (badge) node.append(badge);
		return node;
	}

	_createBadge(item) {
		if (item.todo_count == 0) return null;
		const node = document.createElement("span");
		node.innerText = item.todo_count;
		node.className = "w3-badge w3-brown";
		return node;
	}

	_createSubNodes(items) {
		const node = document.createElement("div");
		node.className = "z-padding-left";
		for (var i = 0; i < items.length; ++i) {
			const subnode = this._createNode(items[i]);
			node.append(subnode);
		}
		return node;
	}



	_createMenu(item) {
		const menu = html.createMenu();
		menu.append(html.createMenuItem("Create", "category-create", item.id));
		menu.append(html.createMenuItem("Edit", "category-edit", item.id));
		menu.append(html.createMenuItem("Delete", "category-delete", item.id));

		const iconNode = document.createElement("div");
		iconNode.className = "w3-dropdown-click w3-padding fa fa-ellipsis-v w3-right";
		iconNode.append(menu);

		return iconNode;
	}

}







/*
class Categories {
	constructor(id) {
		this.id = id;
		this.currentId = null;
	}

	update(onUpdated=null) {
		api.get(`/categories/tree`)
			.then(resp => {
				categories.setTree(resp);
				if (onUpdated) onUpdated();
			}).catch(error => {
				html.showError(error.message);
			});
	}

	setCurrent(categoryId) {
		let sel = document.querySelector(`.category-title[data-id="${this.currentId}"]`);
		if (sel != null)
			sel.classList.remove("selected");

		this.currentId = categoryId;
		sel = document.querySelector(`.category-title[data-id="${this.currentId}"]`);
		if (sel != null)
			sel.classList.add("selected");
	}

	setTree(items) {
		const parent = document.getElementById(this.id);
		parent.innerHTML = "";
		for (var i = 0; i < items.length; ++i) {
			const item = this._createNode(items[i]);
			parent.append(item);
		}
	}

	showAddForm(parentId = null) {
		html.showModal("form-cat-add", parentId);
	}

	_showEditForm(item) {
		html.setValue("form-cat-edit-title", item.title);
		html.showModal("form-cat-edit", item);
	}

	_showDeleteForm(item) {
		html.showModal("form-cat-delete", item);
	}

	addItem(form) {
		const element = document.getElementById("form-cat-add-title");
		const parentId = html.hideModal(form);
		api.post(`/categories/create`, { title: element.value, parent_id: parentId })
			.then(resp => {
				categories.update(function() {
					categories.setCurrent(resp.id);
					todos.setCategory(resp);
				});
			}).catch(error => {
				http.showError(error.message);
			});
		element.value = "";
	}

	saveItem(form) {
		const element = document.getElementById("form-cat-edit-title");
		const category = html.hideModal(form);
		api.post(`/categories/update`, { title: element.value, id: category.id })
			.then(resp => {
				categories.update();
			}).catch(error => {
				http.showError(error.message);
			});
		element.value = "";
	}

	deleteItem(form) {
		const category = html.hideModal(form);
		api.post(`/categories/delete`, { id: category.id })
			.then(resp => {
				categories.update();
			}).catch(error => {
				http.showError(error.message);
			});
	}

	_createNode(item) {
		const menu = this._createMenu(item);
		const title = this._createTitleBar(item, menu);
		const content = this._createSubNodes(item.sub_categories);

		const node = document.createElement("div");
		node.dataset.id = item.id;
		node.className = "category-item";
		node.append(title);
		node.append(content);
		return node;

	}

	_createTitleBar(item, menu) {
		const titleNode = document.createElement("span");
		titleNode.innerText = item.title;
		titleNode.dataset.id = item.id;
		titleNode.className = "category-title z-clickable w3-padding";
		titleNode.onclick = function () {
			categories.setCurrent(item.id);
			todos.setCategory(item);
		};
		titleNode.draggable = true;

		const badge = this._createBadge(item);

		const node = document.createElement("div");
		node.className = "w3-bar";
		node.append(titleNode);
		node.append(menu);
		if (badge) node.append(badge);
		return node;
	}

	_createBadge(item) {
		if (item.todo_count == 0) return null;
		const node = document.createElement("span");
		node.innerText = item.todo_count;
		node.className = "w3-badge w3-brown";
		return node;
	}

	_createSubNodes(items) {
		const node = document.createElement("div");
		node.className = "z-padding-left";
		for (var i = 0; i < items.length; ++i) {
			const subnode = this._createNode(items[i]);
			node.append(subnode);
		}
		return node;
	}

	

	_createMenu(item) {
		const menu = html.createMenu();
		menu.append(html.createMenuItem("Create", function () { categories.showAddForm(item.id); }));
		menu.append(html.createMenuItem("Edit", function () { categories._showEditForm(item); }));
		menu.append(html.createMenuItem("Delete", function () { categories._showDeleteForm(item); }));

		const iconNode = document.createElement("div");
		iconNode.className = "w3-dropdown-click w3-padding fa fa-ellipsis-v w3-right";
		iconNode.append(menu);

		return iconNode;
	}
}
const categories = new Categories("categories-list");*/