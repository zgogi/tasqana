class Table {
    constructor(id, columns) {
        this.id = id;
        this.columns = columns;
    }

    rebuild(data) {
        const table = document.getElementById(this.id);
        table.innerHTML = "";
        table.append(this._createHeader());
        for (var i = 0; i < data.length; ++i) {
            table.append(this._createRow(data[i]));
        }
    }

    _createHeader() {
        const row = document.createElement("tr");
        row.className = "w3-theme-d4";
        for (var i = 0; i < this.columns.length; ++i) {
            const cell = document.createElement('th');
            cell.innerText = this.columns[i].title;
            row.append(cell);
        }
        return row;
    }

    _createRow(row) {
        const result = document.createElement("tr");
        for (var i = 0; i < this.columns.length; ++i) {
            const cell = document.createElement('td');
            const value = row[this.columns[i].id];
            cell.innerText = value;
            result.append(cell);
        }
        return result;
    }

}