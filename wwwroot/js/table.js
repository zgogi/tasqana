class Table {
    constructor(id, columns, allowReorder=false) {
        this._table = document.getElementById(id);;
        this._columns = columns;
        this._allowReorder = allowReorder;
        this._clickListeners = []; // onclick(row,id)

        if (allowReorder) {
            this._dragSource = null;
            this._dropTarget = null;
            this._table.addEventListener("dragstart", event => this._onDragStart(event));
            this._table.addEventListener("dragover", event => this._onDragOver(event));
            this._table.addEventListener("dragleave", event => this._onDragLeave(event));
            this._table.addEventListener("dragend", event => this._onDragEnd(event));
        }

    }

    rebuild(data, header=true) {
        this.clear();
        if (header)
            this._table.append(this._createHeader());
        for (var i = 0; i < data.length; ++i) {
            this._table.append(this._createRow(data[i]));
        }
    }

    addRow(row=null) {
        this._table.append(this._createRow(row));
    }

    clear() { this._table.innerHTML = ""; }

    addClickListener(listener) { this._clickListeners.push(listener); }
    addDragListener(listener) { this._dragListeners.push(listener); }

    read() {
        const ret = [];
        for (var i = 0; i < this._table.childNodes.length; ++i) {
            const row = this._table.childNodes[i];
            if (row.tagName != "TR") continue;
            ret.push(this._readRow(row));
        }
        return ret;
    }

    _createHeader() {
        const row = document.createElement("tr");
        row.className = "w3-theme-d4";
        for (var i = 0; i < this._columns.length; ++i) {
            const type = this._columns[i].type ?? null;
            if (type != "hidden") {
                const cell = document.createElement('th');
                cell.innerText = this._columns[i].title;
                row.append(cell);
            }
        }
        return row;
    }

    _createRow(row=null) {
        const tr = document.createElement("tr");
        if (this._allowReorder)
            tr.draggable = true;
        for (var i = 0; i < this._columns.length; ++i) {
            const item = this._columns[i];
            const type = item.type ?? null;
            const value = (row != null) ? row[item.id] : "";
            if (type == "hidden") {
                tr.dataset[item.id] = value;
                continue;
            }

            const td = document.createElement('td');
            td.dataset.id = item.id;
            if (type == "input-checkbox") {
                const c = (value) ? "checked" : "";
                td.innerHTML = `<input type="checkbox" id="${item.id}" ${c}>`;
            } else if (type == "input-text") {
                td.innerHTML = `<input type="text" id="${item.id}" value="${value}" style="width:100%;">`;
            } else if (type == "button") {
                const btn = document.createElement("i");
                btn.className = `w3-btn fa fa-${item.id}`;
                td.append(btn);
                btn.addEventListener("click", () => {
                    this._clickListeners.forEach(listener => listener(tr, item.id));
                });
            } else {
                td.innerText = value;
            }
            tr.append(td);
        }
        return tr;
    }

    _readRow(row) {
        const ret = {};
        for (var i = 0; i < this._columns.length; ++i) {
            const item = this._columns[i];
            const type = item.type ?? null;

            if (type == "hidden") {
                const value = row.dataset[item.id];
                ret[item.id] = (value === '') ? null : parseInt(value, 10);
            } else if (type == "input-checkbox") {
                const cell = row.querySelector(`td[data-id="${item.id}"] input`);
                ret[item.id] = cell?.checked;
            } else if (type == "input-text") {
                const cell = row.querySelector(`td[data-id="${item.id}"] input`);
                ret[item.id] = cell?.value;
            } else if (type == "button") {
                continue;
            } else {
                const cell = row.querySelector(`td[data-id=${item.id}]`);
                ret[item.id] = cell?.innerText;
            }
        }
        return ret;
    }

    _onDragStart(event) {
        this._dragSource = event.target.closest("tr");
    }

    _onDragOver(event) {
        if (this._dragSource == null) return;
        const tg = event.target.closest("tr");
        if (this._canDropOn(this._dragSource, tg)) {
            this._dropTarget = tg;
            tg.classList.add("drop-before");
            event.preventDefault();
        }
           
    }

    _onDragLeave(event) {
        if (this._dropTarget == null) return;
        this._dropTarget.classList.remove("drop-before");
        this._dropTarget = null;
    }

    _onDragEnd(event) {
        if (this._canDropOn(this._dragSource, this._dropTarget)) {
            this._dropTarget.classList.remove("drop-before");
            this._moveRow(this._dragSource, this._dropTarget);
        }
        this._dragSource = null;
        this._dropTarget = null;
    }

    _canDropOn(src, dst) {
        if ((src == null) || (dst == null)) return false;
        if (src == dst) return false;
        return true;
    }

    _moveRow(row, before) {
        if (!row) return;
        const parent = row.parentNode;

        if (!parent) {
            console.error("Row is not in this DOM");
            return;
        }

        if (before) {
            if (before.parentNode !== parent) {
                console.error("Trying to move row to another table");
                return;
            }
            parent.insertBefore(row, before);
        } else {
            parent.appendChild(row);
        }
    }

}