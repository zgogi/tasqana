
class DragItem {
    constructor(id, type, node) {
        this.id = id;
        this._type = type;
        this.node = node;
    }

    isCategory() { return this._type === 'category'; }
    isTodo() { return this._type === 'todo'; }

    show(onNode, className) {
        const node = this.node.querySelector(onNode);
        if (node == null) return;
        node.classList.add(className);
    }

    hide() {
        const node1 = this.node.querySelector('.drop-target');
        if (node1 != null)
            node1.classList.remove('drop-target');
        const node2 = this.node.querySelector('.drop-before');
        if (node2 != null)
            node2.classList.remove('drop-before');
    }
}


class DragNDrop {
    constructor(store) {
        this._store = store;
        this._source = null;
        this._target = null;
    }

    onStart(node) {
        this._source = this._createDragItem(node);
    }

    onEnter(node) {
        if (this._source == null) return false;
        const target = this._createDragItem(node);
        if (target == null) return false;

        if (this._source.isTodo() && target.isCategory()) {
            this._target = target;
            this._target.show('.category-block', 'drop-target');
            return true;
        } else if (this._source.isCategory() && target.isCategory()) {
            this._target = target;
            this._target.show('.category-before', 'drop-target');
            return true;
        } else if (this._source.isTodo() && target.isTodo()) {
            this._target = target;
            this._target.show('.todo-item', 'drop-before');
            return true;
        }
        return false;
    }

    onLeave(node) {
        const item = this._createDragItem(node);
        if ((this._target != null) && (this._target.node == item.node)) {
            this._target.hide();
            this._target = null;
        }
    }

    onEnd(target) {
        if ((this._source == null) || (this._target == null)) return;
        if (this._source.isTodo() && this._target.isCategory()) {
            this._store.todos.moveToCategory(this._source.id, this._target.id);
        } else if (this._source.isCategory() && this._target.isCategory()) {
            this._store.categories.moveBefore(this._source.id, this._target.id);
        } else if (this._source.isTodo() && this._target.isTodo()) {
            this._store.todos.move(this._source.id, this._target.id);
        }
        this._source = null;
        this._target = null;
    }

    _createDragItem(node) {
        const nodeTodo = node?.closest('.todo-node');
        if (nodeTodo != null) 
            return new DragItem(nodeTodo.dataset.id, 'todo', nodeTodo);

        const nodeCat = node?.closest('.category-node');
        if (nodeCat != null) 
            return new DragItem(nodeCat.dataset.id, 'category', nodeCat);

        return null;
        
    }

}

