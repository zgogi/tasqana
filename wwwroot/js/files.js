
class ImageTileManager {
    constructor(containerSelector, inputId='input-file') {
        this._container = document.querySelector(containerSelector);
        this._files = [];
        this._inputElement = document.getElementById(inputId);
        this._inputElement.addEventListener('change', (e) => this.handleFileSelection(e));
    }

    setFiles(files = []) {
        this._files.forEach(f => URL.revokeObjectURL(f.previewUrl));
        this._files = files.map((item) => {
            return {
                id: item.id,
                localId: crypto.randomUUID(),
                previewUrl: item.url,
                isDeleted: false,
                rawFile: null
            };
        });
        this._render();
    }

    getFiles() {
        return this._files.map((item) => {
            return {
                id: item.id,
                is_deleted: item.isDeleted,
                content: item.rawFile
            };
        });
    }

    handleFileSelection(event) {
        const selectedFiles = Array.from(event.target.files);

        selectedFiles.forEach(file => {
            const previewUrl = URL.createObjectURL(file);

            this._files.push({
                id: null,
                localId: crypto.randomUUID(), 
                rawFile: file,
                previewUrl: previewUrl,
                isDeleted: false
            });
        });

        event.target.value = '';
        this._render();
    }

    _removeFile(localId) {
        const fileObj = this._files.find(f => f.localId === localId);
        if (fileObj) {
            URL.revokeObjectURL(fileObj.previewUrl); // Освобождаем память
        }

        if (fileObj.id != null)
            fileObj.isDeleted = true;
        else
            this._files = this._files.filter(f => f.localId !== localId);

        this._render();
    }

    clear() {
        this._files.forEach(f => URL.revokeObjectURL(f.previewUrl));
        this._files = [];
        this._render();
    }

    _render() {
        this._container.innerHTML = '';

        this._files.forEach(fileObj => {
            if (fileObj.isDeleted) return;
            const tile = document.createElement('div');
            tile.className = 'image-tile';
            tile.style.backgroundImage = `url('${fileObj.previewUrl}')`;

            const deleteBtn = document.createElement('button');
            deleteBtn.className = 'tile-delete-btn';
            deleteBtn.innerHTML = '&times;'; 
            deleteBtn.type = 'button';

            deleteBtn.addEventListener('click', (e) => {
                e.stopPropagation(); 
                this._removeFile(fileObj.localId);
            });

            tile.appendChild(deleteBtn);
            this._container.appendChild(tile);
        });

        const plusTile = document.createElement('div');
        plusTile.className = 'image-tile plus-tile';
        plusTile.innerHTML = '<span class="plus-icon">+</span>';

        plusTile.addEventListener('click', () => this._inputElement.click());

        this._container.appendChild(plusTile);
    }
}