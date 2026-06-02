
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
		const content = this._createContent(item);
		if (content != null)
			node.append(content);
		return node;
	}

	_createContent(item) {
		if ((item.description == null) && (item.check_items.length == 0)) return null;
		const node = document.createElement("div");
		node.className = "accordion-content w3-bar-item w3-theme-d2 w3-padding w3-hide";
		if (item.description != null) {
			const nodeDesc = document.createElement("div");
			nodeDesc.innerText = item.description;
			node.append(nodeDesc);
		}
		for (var i = 0; i < item.check_items.length; ++i) {
			node.append(this._createCheckItem(item.check_items[i]));
		}
		return node;

	}

	_createMenu(item) {
		const menu = html.createMenu(true);
		menu.append(html.createMenuItem("Complete", {id: item.id}, "todo-complete"));
		menu.append(html.createMenuItem("Edit", { modal: "form-todo-edit", id: item.id }));
		menu.append(html.createMenuItem("Delete", { modal: "form-todo-delete", id: item.id }));
		menu.append(html.createMenuItem("Add list item", { modal: "form-check-edit", todoId: item.id }));
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

	_createCheckItem(item) {
		const check = document.createElement("i");
		check.className = (item.is_completed) ? "check-mark fa fa-toggle-on z-clickable" : "check-mark fa fa-toggle-off z-clickable";
		check.dataset.id = item.id;

		const text = document.createElement("span");
		text.className = "w3-margin-left";
		text.innerText = item.title;

		const edit = document.createElement("i");
		edit.className = "fa fa-edit w3-right z-clickable";
		edit.dataset.id = item.id;
		edit.dataset.modal = "form-check-edit";

		const node = document.createElement("div");
		node.className = "check-item";
		node.dataset.id = item.id;
		node.append(check);
		node.append(text);
		node.append(edit);
		return node;
	}

}



