
class Api {
    constructor() {
        this.host = "/api/v1.0";
    }

    async _request(path, options = {}) {
        const url = this.host + path;

        const headers = (options.contentType != undefined) ? {
            'Content-Type': options.contentType,
            ...options.headers
        } : {
            ...options.headers
        };

        const token = localStorage.getItem('token');
        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }
        try {
            const response = await fetch(url, { ...options, headers });
            if (response.status === 401) {
                window.location.replace("/login/noauth.html");
                return null;
            }
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            if (response.status === 204) {
                return null;
            }
            
            const contentLength = response.headers.get("content-length");
            if (contentLength === "0") {
                return null;
            }
            return await response.json();
        } catch (error) {
            console.log(`API Error on ${path}:`, error);
            html.showError(error);
            throw error; 
        }
    }

    async get(path) {
        return await this._request(path, { method: 'GET' });
    }

    async post(path, body) {
        return await this._request(path, {
            method: 'POST',
            contentType: 'application/json',
            body: JSON.stringify(body) 
        });
    }

    async postForm(path, form) {
        return await this._request(path, {
            method: 'POST',
            body: form
        });
    }
}


function objectToFormData(obj, formData = new FormData(), parentKey = '') {

    if (obj === null || obj === undefined) {
        return formData;
    }

    if (obj instanceof File || obj instanceof Blob) {
        formData.append(parentKey, obj);
    } else if (Array.isArray(obj)) {
        obj.forEach((element, index) => {
            objectToFormData(element, formData, `${parentKey}[${index}]`);
        });
    } else if (typeof obj === 'object' && !(obj instanceof Date)) {
        Object.keys(obj).forEach(key => {
            const fullKey = parentKey ? `${parentKey}.${key}` : key;
            objectToFormData(obj[key], formData, fullKey);
        });
    } else {
        formData.append(parentKey, obj);
    }

    return formData;
}



