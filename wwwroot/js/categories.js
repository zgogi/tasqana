

class CategoriesStore {
	constructor(parent, api) {
		this.parent = parent;
		this.api = api;
		this.items = [];
		this.selected = null;
	}

	update(clear=false) {
		this.api.get(`/categories/tree`)
			.then(resp => {
				this.items = resp;
				this.render(clear);
			});
	}

	select(filter) {
		html.removeClasses(".selected", "selected");
		if (filter.categoryId != undefined) {
			this.selected = this.get(filter.categoryId);
			this.parent.todos.setFilter({ category: this.selected });
			html.addClasses(`.category-title[data-id="${filter.categoryId}"]`, "selected");
		} else if (filter.priority != undefined) {
			this.selected = null;
			this.parent.todos.setFilter(filter);
			html.addClasses(`#category-priority`, "selected");
		} else if (filter.state != undefined) {
			this.selected = null;
			this.parent.todos.setFilter(filter);
			html.addClasses(`#category-completed`, "selected");
		} else {
			this.selected = null;
			this.parent.todos.setFilter(filter);
			html.addClasses(`#category-unsorted`, "selected");
		}
	}

	create(data) {
		this.api.post(`/categories/create`, data)
			.then(resp => {
				this.update(true);
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
				this.update(true);
				this.select(null);
			});
	}

	moveBefore(id, beforeId) {
		this.api.post(`/categories/move`, {id: id, before_id: beforeId})
			.then(resp => {
				this.items = resp;
				this.render(true);
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

	render(clear = false) {
		if (clear)
			document.getElementById("categories-list").innerHTML = "";
		this.items.forEach(todo => this._renderItem(todo));
	}

	_renderItem(item, container=null) {
		if (container === null) 
			container = document.getElementById("categories-list");

		var node = container.querySelector(`.category-node[data-id="${item.id}"]`);
		const isNew = !node;

		if (isNew) {
			node = document.createElement("div");
			node.dataset.id = item.id;
			//node.draggable = true;
			node.className = "category-node w3-bar-item";
			node.innerHTML = `
				<div class="category-before w3-padding w3-hide"></div>
				<div class="category-block w3-block w3-flex w3-theme-d4" draggable="true">
					<div class="category-click w3-block z-clickable w3-left-align w3-padding">
						<span class="category-title" data-id="${item.id}"></span>
						<span class="category-count w3-badge w3-white w3-text-black"></span>
					</div>
					<div class="w3-padding w3-dropdown-click fa fa-ellipsis-v">
                        <div class="w3-dropdown-content w3-bar-block w3-border" style="right:0;">
							<span class="w3-bar-item w3-button" data-modal="form-cat-create" data-id="${item.id}">Createt</span>
                            <span class="w3-bar-item w3-button" data-modal="form-cat-edit" data-id="${item.id}">Edit</span>
                            <span class="w3-bar-item w3-button" data-modal="form-cat-delete" data-id="${item.id}">Delete</span>
                        </div>
                    </div>
				</div>
				<div class="category-subnodes w3-margin-left"></div>
				
			`;

			var subs = node.querySelector(".category-subnodes");
			item.sub_categories.forEach(it => this._renderItem(it, subs));

		}

		node.querySelector(".category-title").innerText = item.title;

		const todo_count = node.querySelector(".category-count");
		todo_count.innerText = item.todo_count;
		html.setVisible(todo_count, item.todo_count > 0);

		if (isNew) {
			container.append(node);
		}

	}

}


