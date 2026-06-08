class Table {
    constructor(id, columns) {
        this._table = document.getElementById(id);;
        this._columns = columns;
        this._listeners = [];
    }

    rebuild(data, header=true) {
        this.clear();
        if (header)
            this._table.append(this._createHeader());
        for (var i = 0; i < data.length; ++i) {
            this._table.append(this._createRow(data[i]));
        }
    }

    clear() { this._table.innerHTML = ""; }

    addEventListener(listener) { this._listeners.push(listener); }

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

    _createRow(row) {
        const tr = document.createElement("tr");
        for (var i = 0; i < this._columns.length; ++i) {
            const item = this._columns[i]
            const type = item.type ?? null;
            const value = row[item.id];
            if (type == "hidden") {
                tr.dataset[item.id] = value;
                continue;
            }

            const td = document.createElement('td');
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
                   this._listeners.forEach(listener => listener(tr, item.id));
                });
            } else {
                td.innerText = value;
            }
            tr.append(td);
        }
        return tr;
    }

}