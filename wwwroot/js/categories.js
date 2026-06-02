

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
		menu.append(html.createMenuItem("Create", { modal: "form-cat-create", id: item.id }));
		menu.append(html.createMenuItem("Edit", { modal: "form-cat-edit", id: item.id }));
		menu.append(html.createMenuItem("Delete", { modal: "form-cat-delete", id: item.id }));

		const iconNode = document.createElement("div");
		iconNode.className = "w3-dropdown-click w3-padding fa fa-ellipsis-v w3-right";
		iconNode.append(menu);

		return iconNode;
	}

}


