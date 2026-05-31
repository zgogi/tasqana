
class Api {
    constructor() {
        this.host = "/api/v1.0";
    }

    async _request(path, options = {}) {
        const url = this.host + path;

        const headers = {
            'Content-Type': 'application/json',
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
            throw error; 
        }
    }

    async get(path) {
        return await this._request(path, { method: 'GET' });
    }

    async post(path, body) {
        return await this._request(path, {
            method: 'POST',
            body: JSON.stringify(body) 
        });
    }
}

//const api = new Api();

/*
api.get('/path')
	.then(data => {
		console.log(data);
	})
	.catch(error => {
		console.log("Error:", error);
	});
*/


