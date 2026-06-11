const views = {
    dashboard: { title: 'Dashboard', el: () => document.getElementById('view-dashboard') },
    subjects: { title: 'Subjects', el: () => document.getElementById('view-subjects') },
    tasks: { title: 'Tasks', el: () => document.getElementById('view-tasks') },
    goals: { title: 'Goals', el: () => document.getElementById('view-goals') },
    sessions: { title: 'Study Sessions', el: () => document.getElementById('view-sessions') },
    achievements: { title: 'Achievements', el: () => document.getElementById('view-achievements') }
};

function showError(msg) {
    const el = document.getElementById('global-error');
    el.textContent = msg;
    el.style.color = '';
    el.classList.remove('hidden');
    setTimeout(() => el.classList.add('hidden'), 5000);
}

function showSuccess(msg) {
    const el = document.getElementById('global-error');
    el.textContent = msg;
    el.style.color = '#86efac';
    el.classList.remove('hidden');
    setTimeout(() => { el.classList.add('hidden'); el.style.color = ''; }, 5000);
}

function showAuthError(msg) {
    const el = document.getElementById('auth-error');
    el.textContent = msg;
    el.classList.remove('hidden');
}

function showApp() {
    document.getElementById('auth-screen').classList.add('hidden');
    document.getElementById('app').classList.remove('hidden');
    const user = Api.getUser();
    document.getElementById('user-name').textContent = user ? `${user.firstName} ${user.lastName}` : '';
    navigate('dashboard');
}

function showAuth() {
    document.getElementById('app').classList.add('hidden');
    document.getElementById('auth-screen').classList.remove('hidden');
}

async function navigate(name) {
    document.querySelectorAll('.nav-btn').forEach(b => b.classList.toggle('active', b.dataset.view === name));
    document.querySelectorAll('.view').forEach(v => v.classList.add('hidden'));
    views[name].el().classList.remove('hidden');
    document.getElementById('view-title').textContent = views[name].title;

    try {
        if (name === 'dashboard') await renderDashboard();
        if (name === 'subjects') await renderSubjects();
        if (name === 'tasks') await renderTasks();
        if (name === 'goals') await renderGoals();
        if (name === 'sessions') await renderSessions();
        if (name === 'achievements') await renderAchievements();
    } catch (e) {
        showError(e.message);
    }
}

async function renderDashboard() {
    const stats = await Api.getStatistics();
    const el = views.dashboard.el();
    el.innerHTML = `
        <div class="stats-grid">
            <div class="stat-card"><div class="label">Subjects</div><div class="value">${stats.totalSubjects}</div></div>
            <div class="stat-card"><div class="label">Completed Tasks</div><div class="value">${stats.completedTasks}</div></div>
            <div class="stat-card"><div class="label">Pending Tasks</div><div class="value">${stats.pendingTasks}</div></div>
            <div class="stat-card"><div class="label">Hours Studied</div><div class="value">${stats.hoursStudied}</div></div>
            <div class="stat-card"><div class="label">Goals Done</div><div class="value">${stats.goalsCompleted}</div></div>
            <div class="stat-card"><div class="label">Achievement Pts</div><div class="value">${stats.achievementPoints}</div></div>
        </div>
        <div class="panel">
            <h3>Weekly Activity</h3>
            ${stats.weeklyActivity?.length
                ? `<table><tr><th>Date</th><th>Hours</th></tr>${stats.weeklyActivity.map(w =>
                    `<tr><td>${new Date(w.date).toLocaleDateString()}</td><td>${w.hoursStudied}</td></tr>`).join('')}</table>`
                : '<p class="empty">No activity this week</p>'}
        </div>`;
}

async function renderSubjects() {
    const data = await Api.getSubjects();
    const items = data.items || [];
    const el = views.subjects.el();
    el.innerHTML = `
        <div class="panel">
            <h3>Add Subject</h3>
            <div class="form-row">
                <input type="text" id="sub-name" placeholder="Name" />
                <input type="text" id="sub-color" placeholder="#3498db" value="#3498db" />
                <button type="button" class="btn" id="add-subject-btn">Add</button>
            </div>
        </div>
        <div class="panel">
            <h3>Your Subjects (${data.totalCount})</h3>
            ${items.length ? `<table>
                <tr><th>Name</th><th>Color</th><th>Created</th><th></th></tr>
                ${items.map(s => `<tr>
                    <td>${escapeHtml(s.name)}</td>
                    <td><span style="color:${s.color}">&#9679;</span> ${s.color}</td>
                    <td>${new Date(s.createdOn).toLocaleDateString()}</td>
                    <td><button class="btn-sm danger" data-del-sub="${s.id}">Delete</button></td>
                </tr>`).join('')}
            </table>` : '<p class="empty">No subjects yet</p>'}
        </div>`;

    document.getElementById('add-subject-btn').onclick = async () => {
        const name = document.getElementById('sub-name').value.trim();
        const color = document.getElementById('sub-color').value.trim() || '#3498db';
        if (!name) return;
        await Api.createSubject({ name, color, description: '' });
        await renderSubjects();
    };

    el.querySelectorAll('[data-del-sub]').forEach(btn => {
        btn.onclick = async () => {
            await Api.deleteSubject(btn.dataset.delSub);
            await renderSubjects();
        };
    });
}

async function renderTasks() {
    const [data, subjectsData] = await Promise.all([Api.getTasks(), Api.getSubjects()]);
    const items = data.items || [];
    const subjects = subjectsData.items || [];
    const statusLabel = s => ['Pending','In Progress','Completed','Overdue'][s] || s;
    const el = views.tasks.el();
    const subjectOptions = subjects.map(s =>
        `<option value="${s.id}">${escapeHtml(s.name)}</option>`).join('');

    const defaultDeadline = new Date();
    defaultDeadline.setDate(defaultDeadline.getDate() + 7);
    const deadlineStr = defaultDeadline.toISOString().slice(0, 16);

    el.innerHTML = `
        <div class="panel">
            <h3>Add Task</h3>
            ${subjects.length ? `<div class="form-row">
                <input type="text" id="task-title" placeholder="Task title" />
                <select id="task-subject">${subjectOptions}</select>
                <input type="datetime-local" id="task-deadline" value="${deadlineStr}" />
                <select id="task-priority">
                    <option value="0">Low</option>
                    <option value="1" selected>Medium</option>
                    <option value="2">High</option>
                    <option value="3">Urgent</option>
                </select>
                <button type="button" class="btn" id="add-task-btn">Add Task</button>
            </div>` : '<p class="empty">Create a subject first before adding tasks.</p>'}
        </div>
        <div class="panel">
            <h3>Your Tasks (${data.totalCount})</h3>
            ${items.length ? `<table>
                <tr><th>Title</th><th>Subject</th><th>Deadline</th><th>Priority</th><th>Status</th><th></th></tr>
                ${items.map(t => `<tr>
                    <td>${escapeHtml(t.title)}</td>
                    <td>${escapeHtml(t.subjectName)}</td>
                    <td>${new Date(t.deadline).toLocaleDateString()}</td>
                    <td>${['Low','Medium','High','Urgent'][t.priority] || t.priority}</td>
                    <td><span class="badge ${t.status === 2 ? 'completed' : 'pending'}">${statusLabel(t.status)}</span></td>
                    <td>${t.status !== 2 ? `<button class="btn-sm success" data-complete="${t.id}">Complete</button>` : ''}</td>
                </tr>`).join('')}
            </table>` : '<p class="empty">No tasks yet</p>'}
        </div>`;

    const addBtn = document.getElementById('add-task-btn');
    if (addBtn) {
        addBtn.onclick = async () => {
            const title = document.getElementById('task-title').value.trim();
            const subjectId = parseInt(document.getElementById('task-subject').value, 10);
            const deadline = document.getElementById('task-deadline').value;
            const priority = parseInt(document.getElementById('task-priority').value, 10);
            if (!title || !deadline) return;
            await Api.createTask({
                title,
                description: '',
                subjectId,
                deadline: new Date(deadline).toISOString(),
                priority
            });
            await renderTasks();
        };
    }

    el.querySelectorAll('[data-complete]').forEach(btn => {
        btn.onclick = async () => {
            await Api.completeTask(btn.dataset.complete);
            await renderTasks();
        };
    });
}

async function renderGoals() {
    const data = await Api.getGoals();
    const items = data.items || [];
    const statusLabel = s => ['Not Started','In Progress','Completed','Expired'][s] || s;
    const el = views.goals.el();

    const defaultDeadline = new Date();
    defaultDeadline.setMonth(defaultDeadline.getMonth() + 1);
    const deadlineStr = defaultDeadline.toISOString().slice(0, 10);

    el.innerHTML = `
        <div class="panel">
            <h3>Add Goal</h3>
            <div class="form-row">
                <input type="text" id="goal-title" placeholder="Goal title" />
                <input type="number" id="goal-target" placeholder="Target hours" min="0.1" step="0.5" value="10" />
                <input type="date" id="goal-deadline" value="${deadlineStr}" />
                <button type="button" class="btn" id="add-goal-btn">Add Goal</button>
            </div>
        </div>
        <div class="panel">
            <h3>Your Goals (${data.totalCount})</h3>
            ${items.length ? items.map(g => {
                const pct = g.targetHours > 0 ? Math.min(100, Math.round(g.currentHours / g.targetHours * 100)) : 0;
                const isDone = g.status === 2;
                return `<div class="goal-card">
                    <div class="goal-header">
                        <strong>${escapeHtml(g.title)}</strong>
                        <span class="badge ${isDone ? 'completed' : 'pending'}">${statusLabel(g.status)}</span>
                    </div>
                    <div class="progress-bar"><div class="progress-fill" style="width:${pct}%"></div></div>
                    <div class="goal-meta">${g.currentHours} / ${g.targetHours} hours &middot; Deadline: ${new Date(g.deadline).toLocaleDateString()}</div>
                    ${!isDone ? `<div class="form-row goal-hours-row">
                        <input type="number" id="goal-hours-${g.id}" placeholder="Hours studied today" min="0.1" max="24" step="0.5" />
                        <button type="button" class="btn-sm success" data-add-hours="${g.id}" data-current="${g.currentHours}" data-target="${g.targetHours}">Add hours</button>
                    </div>` : '<p class="goal-done-msg">Goal completed!</p>'}
                </div>`;
            }).join('') : '<p class="empty">No goals yet</p>'}
        </div>`;

    document.getElementById('add-goal-btn').onclick = async () => {
        const title = document.getElementById('goal-title').value.trim();
        const targetHours = parseFloat(document.getElementById('goal-target').value);
        const deadline = document.getElementById('goal-deadline').value;
        if (!title || !deadline || !targetHours) return;
        const result = await Api.createGoal({
            title,
            description: '',
            targetHours,
            deadline: new Date(deadline).toISOString()
        });
        if (result.status === 2) {
            showError('Goal created and completed!');
        }
        await renderGoals();
    };

    el.querySelectorAll('[data-add-hours]').forEach(btn => {
        btn.onclick = async () => {
            const id = btn.dataset.addHours;
            const input = document.getElementById(`goal-hours-${id}`);
            const hours = parseFloat(input.value);
            if (!hours || hours <= 0) return;
            const result = await Api.addGoalHours(id, hours);
            if (result.status === 2) {
                showSuccess('Congratulations! Goal completed!');
            }
            await renderGoals();
            await Api.evaluateAchievements();
        };
    });
}

async function renderSessions() {
    const data = await Api.getSessions();
    const items = data.items || [];
    const el = views.sessions.el();
    el.innerHTML = `
        <div class="panel">
            <h3>Study Sessions (${data.totalCount})</h3>
            ${items.length ? `<table>
                <tr><th>Title</th><th>Subject</th><th>Start</th><th>Duration</th></tr>
                ${items.map(s => `<tr>
                    <td>${escapeHtml(s.title)}</td>
                    <td>${escapeHtml(s.subjectName)}</td>
                    <td>${new Date(s.startTime).toLocaleString()}</td>
                    <td>${s.duration} min</td>
                </tr>`).join('')}
            </table>` : '<p class="empty">No sessions yet</p>'}
        </div>`;
}

async function renderAchievements() {
    const items = await Api.getAchievements();
    const el = views.achievements.el();
    const unlocked = items.filter(a => a.isUnlocked).length;
    el.innerHTML = `
        <div class="panel">
            <div class="goal-header">
                <h3>Your Achievements (${unlocked}/${items.length} unlocked)</h3>
                <button type="button" class="btn-sm" id="refresh-achievements">Check for new</button>
            </div>
            <p class="demo-hint" style="text-align:left;margin-bottom:1rem">Achievements unlock automatically when you complete tasks, goals, and study hours.</p>
            ${items.length ? `<table>
                <tr><th>Title</th><th>Points</th><th>Status</th><th>Unlocked</th></tr>
                ${items.map(a => `<tr>
                    <td>${escapeHtml(a.title)}</td>
                    <td>${a.points}</td>
                    <td><span class="badge ${a.isUnlocked ? 'completed' : 'pending'}">${a.isUnlocked ? 'Unlocked' : 'Locked'}</span></td>
                    <td>${a.unlockedDate ? new Date(a.unlockedDate).toLocaleDateString() : '-'}</td>
                </tr>`).join('')}
            </table>` : '<p class="empty">No achievements yet</p>'}
        </div>`;

    document.getElementById('refresh-achievements').onclick = async () => {
        await Api.evaluateAchievements();
        await renderAchievements();
    };
}

function escapeHtml(text) {
    const d = document.createElement('div');
    d.textContent = text || '';
    return d.innerHTML;
}

document.querySelectorAll('.tab').forEach(tab => {
    tab.onclick = () => {
        document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
        tab.classList.add('active');
        document.getElementById('login-form').classList.toggle('hidden', tab.dataset.tab !== 'login');
        document.getElementById('register-form').classList.toggle('hidden', tab.dataset.tab !== 'register');
        document.getElementById('auth-error').classList.add('hidden');
    };
});

document.getElementById('login-form').onsubmit = async e => {
    e.preventDefault();
    try {
        const data = await Api.login(
            document.getElementById('login-email').value,
            document.getElementById('login-password').value
        );
        Api.setAuth(data);
        showApp();
    } catch (err) {
        showAuthError(err.message);
    }
};

document.getElementById('register-form').onsubmit = async e => {
    e.preventDefault();
    try {
        const data = await Api.register({
            firstName: document.getElementById('reg-first').value,
            lastName: document.getElementById('reg-last').value,
            email: document.getElementById('reg-email').value,
            password: document.getElementById('reg-password').value,
            confirmPassword: document.getElementById('reg-confirm').value
        });
        Api.setAuth(data);
        showApp();
    } catch (err) {
        showAuthError(err.message);
    }
};

document.getElementById('logout-btn').onclick = async () => {
    try { await Api.logout(); } catch (_) {}
    Api.clearAuth();
    showAuth();
};

document.querySelectorAll('.nav-btn').forEach(btn => {
    btn.onclick = () => navigate(btn.dataset.view);
});

if (Api.getToken()) {
    showApp();
} else {
    showAuth();
}
