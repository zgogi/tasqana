
class DragNDrop {
    constructor(store) {
        this.store = store;
        this.dragSource = null;
        this.dropTarget = null;
    }

    onStart(target) {
        this.dragSource = this._getNodeInfo(target);
    }

    onEnter(target) {
        const srcTodo = this.dragSource.target.closest(".todo-node");
        const dstCategory = target.closest(".category-title");

        if ((srcTodo != null) && (dstCategory != null)) {
            this.dropTarget = this._getNodeInfo(target);
            this.dropTarget.target.classList.add("drop-target");
            return true;
        }
        return false;
    }

    onLeave(target) {
        if (this.dropTarget?.target === target) {
            this.dropTarget.target.classList.remove("drop-target");
            this.dropTarget = null;
        }
    }

    onEnd(target) {
        if (this.dragSource && this.dropTarget) {
            this.dropTarget.target.classList.remove("drop-target");
            if ((this.dragSource.type == "todo") && (this.dropTarget.type == "category")) {
                this.store.todos.moveToCategory(this.dragSource.id, this.dropTarget.id);
            }
        }
        this.dragSource = null;
        this.dropTarget = null;
    }

    _getNodeInfo(target) {
        const category = target.closest(".category-title");
        const todo = target.closest(".todo-node");
        if (category != null) {
            return {
                target: target,
                node: category,
                id: category.dataset.id,
                type: "category"
            };
        } else if (todo != null) {
            return {
                target: target,
                node: todo,
                id: todo.dataset.id,
                type: "todo"
            };
        }
            
    }

}

//const dragndrop = new DragNDrop();