const Api = {
    getToken() {
        return localStorage.getItem('accessToken');
    },

    setAuth(data) {
        localStorage.setItem('accessToken', data.accessToken);
        localStorage.setItem('refreshToken', data.refreshToken);
        localStorage.setItem('user', JSON.stringify(data));
    },

    clearAuth() {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
    },

    getUser() {
        const u = localStorage.getItem('user');
        return u ? JSON.parse(u) : null;
    },

    async request(url, options = {}) {
        const headers = { 'Content-Type': 'application/json', ...options.headers };
        const token = this.getToken();
        if (token) headers['Authorization'] = `Bearer ${token}`;

        const response = await fetch(url, { ...options, headers });
        const json = await response.json().catch(() => ({}));

        if (!response.ok) {
            const msg = json.message || json.title || 'Request failed';
            throw new Error(msg);
        }

        return json.data !== undefined ? json.data : json;
    },

    login(email, password) {
        return this.request('/api/auth/login', {
            method: 'POST',
            body: JSON.stringify({ email, password })
        });
    },

    register(dto) {
        return this.request('/api/auth/register', {
            method: 'POST',
            body: JSON.stringify(dto)
        });
    },

    logout() {
        return this.request('/api/auth/logout', { method: 'POST' });
    },

    getStatistics() {
        return this.request('/api/progress/statistics');
    },

    getSubjects() {
        return this.request('/api/subjects?pageSize=50');
    },

    createSubject(dto) {
        return this.request('/api/subjects', { method: 'POST', body: JSON.stringify(dto) });
    },

    deleteSubject(id) {
        return this.request(`/api/subjects/${id}`, { method: 'DELETE' });
    },

    getTasks() {
        return this.request('/api/tasks?pageSize=50&sortBy=priority&sortDirection=1');
    },

    createTask(dto) {
        return this.request('/api/tasks', { method: 'POST', body: JSON.stringify(dto) });
    },

    completeTask(id) {
        return this.request(`/api/tasks/${id}/complete`, { method: 'POST' });
    },

    getGoals() {
        return this.request('/api/goals?pageSize=50');
    },

    createGoal(dto) {
        return this.request('/api/goals', { method: 'POST', body: JSON.stringify(dto) });
    },

    addGoalHours(id, hoursStudied) {
        return this.request(`/api/goals/${id}/add-hours`, {
            method: 'POST',
            body: JSON.stringify({ hoursStudied: parseFloat(hoursStudied) })
        });
    },

    evaluateAchievements() {
        return this.request('/api/achievements/evaluate', { method: 'POST' });
    },

    getSessions() {
        return this.request('/api/studysessions?pageSize=20');
    },

    createSession(dto) {
        return this.request('/api/studysessions', { method: 'POST', body: JSON.stringify(dto) });
    },

    getAchievements() {
        return this.request('/api/achievements/user');
    }
};
