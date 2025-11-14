/* cuidabem.js
   Implementa autenticação local, persistência por usuário e gerenciamento de lembretes/perfil.
   ATENÇÃO: demo apenas. Não usar localStorage para autenticação real em produção.
*/

(function(){
  // Keys
  const usersKey = 'cuidabem_users';
  const sessionKey = 'cuidabem_session';

  // --- Utilities ---
  function getUsers(){
    try{ return JSON.parse(localStorage.getItem(usersKey) || '{}'); }
    catch(e){ console.warn('getUsers parse error', e); return {}; }
  }
  function saveUsers(u){ localStorage.setItem(usersKey, JSON.stringify(u)); }

  function escapeHtml(s){
    return (s+'').replace(/[&<>"']/g, c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":"&#39;"}[c]));
  }

  // --- Auth UI helpers ---
  window.showRegister = function(){
    const loginForm = document.getElementById('loginForm');
    const regForm = document.getElementById('regForm');
    if(loginForm) loginForm.style.display = 'none';
    if(regForm) regForm.style.display = 'block';
  };
  window.showLogin = function(){
    const loginForm = document.getElementById('loginForm');
    const regForm = document.getElementById('regForm');
    if(loginForm) loginForm.style.display = 'block';
    if(regForm) regForm.style.display = 'none';
  };

  // --- Auth actions ---
  window.register = function(){
    const username = (document.getElementById('regUser')||{}).value?.trim();
    const pass = (document.getElementById('regPass')||{}).value || '';
    const name = (document.getElementById('regName')||{}).value?.trim() || username;

    if(!username || !pass){ alert('Preencha usuário e senha'); return; }

    const users = getUsers();
    if(users[username]){ alert('Usuário já existe'); return; }

    // Store minimal demo record
    users[username] = { pass: pass, name: name, created: Date.now() };
    saveUsers(users);
    alert('Conta criada. Faça login.');
    showLogin();
  };

  window.login = function(){
    const username = (document.getElementById('loginUser')||{}).value?.trim();
    const pass = (document.getElementById('loginPass')||{}).value || '';
    const users = getUsers();
    if(!username || !pass || !users[username] || users[username].pass !== pass){
      alert('Usuário ou senha inválidos');
      return;
    }
    localStorage.setItem(sessionKey, username);
    openAppFor(username);
  };

  window.logout = function(){
    localStorage.removeItem(sessionKey);
    // simple page reset
    location.reload();
  };

  function getCurrentUser(){ return localStorage.getItem(sessionKey); }

  // --- Per-user reminders persistence ---
  function remindersKeyFor(user){ return `cuidabem_reminders_${user}`; }
  function loadReminders(){
    const user = getCurrentUser(); if(!user) return [];
    try{ return JSON.parse(localStorage.getItem(remindersKeyFor(user)) || '[]'); }
    catch(e){ console.warn('loadReminders parse', e); return []; }
  }
  function saveReminders(list){
    const user = getCurrentUser(); if(!user) return;
    localStorage.setItem(remindersKeyFor(user), JSON.stringify(list));
    renderReminders();
  }

  window.addReminder = function(){
    const medEl = document.getElementById('med');
    const timeEl = document.getElementById('time');
    const med = medEl?.value?.trim();
    const time = timeEl?.value?.trim();
    if(!med || !time){ alert('Preencha os campos'); return; }
    const list = loadReminders();
    list.push({ med: med, time: time, created: Date.now() });
    saveReminders(list);
    medEl.value = ''; timeEl.value = '';
  };

  window.removeReminder = function(i){
    const list = loadReminders();
    if(i < 0 || i >= list.length) return;
    list.splice(i, 1);
    saveReminders(list);
  };

  window.clearReminders = function(){
    const user = getCurrentUser();
    if(!user) return;
    if(confirm('Limpar todos os lembretes?')){
      localStorage.removeItem(remindersKeyFor(user));
      renderReminders();
    }
  };

  window.confirmTaken = function(i){
    const list = loadReminders();
    const r = list[i];
    if(!r) return;
    alert(`Medicação confirmada: ${r.med} às ${r.time}`);
    // could move to history: future enhancement
  };

  function renderReminders(){
    const items = loadReminders();
    const remList = document.getElementById('remList');
    const quickList = document.getElementById('quickList');
    if(!remList) return;
    remList.innerHTML = '';
    if(!items || items.length === 0){
      remList.innerHTML = '<div class="muted">Nenhum lembrete. Adicione um acima.</div>';
      if(quickList) quickList.textContent = 'Nenhum lembrete salvo.';
      return;
    }
    if(quickList) quickList.textContent = items.slice(-3).map(r => `${r.med} @ ${r.time}`).join(' | ');
    items.forEach((r, idx) => {
      const el = document.createElement('div');
      el.className = 'reminder';
      el.innerHTML = `<div><strong>${escapeHtml(r.med)}</strong><div class="muted">${escapeHtml(r.time)}</div></div>
                      <div>
                        <button class="ghost" onclick="confirmTaken(${idx})" aria-label="Confirmar ${escapeHtml(r.med)}">Confirmar</button>
                        <button class="ghost" onclick="removeReminder(${idx})" aria-label="Remover ${escapeHtml(r.med)}">Remover</button>
                      </div>`;
      remList.appendChild(el);
    });
  }

  // --- Profile management (per user) ---
  function profileKeyFor(user){ return `cuidabem_profile_${user}`; }

  window.saveProfile = function(){
    const user = getCurrentUser();
    if(!user) return alert('Nenhum usuário autenticado');
    const profile = {
      name: (document.getElementById('pName')||{}).value?.trim() || '',
      age: (document.getElementById('pAge')||{}).value?.trim() || '',
      conditions: (document.getElementById('pConditions')||{}).value?.trim() || '',
      caregiver: (document.getElementById('pCaregiver')||{}).value?.trim() || '',
      updated: Date.now()
    };
    localStorage.setItem(profileKeyFor(user), JSON.stringify(profile));
    const summaryText = formatProfile(profile);
    const summaryEl = document.getElementById('profileSummaryText');
    if(summaryEl) summaryEl.textContent = summaryText;
    const summaryWrap = document.getElementById('profileSummary');
    if(summaryWrap) summaryWrap.style.display = 'block';

    // update user display name
    const users = getUsers();
    if(users[user]){ users[user].name = profile.name || users[user].name; saveUsers(users); }
    const profileNameEl = document.getElementById('profileName');
    if(profileNameEl) profileNameEl.textContent = users[user]?.name || profile.name || 'Usuário';

    alert('Perfil salvo localmente.');
  };

  function loadProfileIntoForm(){
    const user = getCurrentUser();
    if(!user) return;
    try{
      const raw = localStorage.getItem(profileKeyFor(user));
      if(!raw) { resetProfileForm(); return; }
      const p = JSON.parse(raw);
      document.getElementById('pName').value = p.name || '';
      document.getElementById('pAge').value = p.age || '';
      document.getElementById('pConditions').value = p.conditions || '';
      document.getElementById('pCaregiver').value = p.caregiver || '';
      const summaryEl = document.getElementById('profileSummaryText');
      if(summaryEl) summaryEl.textContent = formatProfile(p);
      const summaryWrap = document.getElementById('profileSummary');
      if(summaryWrap) summaryWrap.style.display = 'block';
    }catch(e){
      console.warn('Erro ao carregar perfil', e);
    }
  }

  window.resetProfileForm = function(){
    const ids = ['pName','pAge','pConditions','pCaregiver'];
    ids.forEach(id => {
      const el = document.getElementById(id);
      if(el) el.value = '';
    });
    const summaryWrap = document.getElementById('profileSummary');
    if(summaryWrap) summaryWrap.style.display = 'none';
  };

  function formatProfile(p){
    const parts = [];
    if(p.name) parts.push(p.name);
    if(p.age) parts.push(`${p.age} anos`);
    if(p.conditions) parts.push('Condições: ' + p.conditions);
    if(p.caregiver) parts.push('Cuidador: ' + p.caregiver);
    return parts.join(' • ');
  }

  // --- App open / session management ---
  function openAppFor(username){
    const users = getUsers();
    const u = users[username] || { name: username };
    const profileNameEl = document.getElementById('profileName');
    const profileUserEl = document.getElementById('profileUser');
    if(profileNameEl) profileNameEl.textContent = u.name || 'Usuário';
    if(profileUserEl) profileUserEl.textContent = '@' + username;

    loadProfileIntoForm();

    const authCard = document.getElementById('authCard');
    const app = document.getElementById('app');
    if(authCard) authCard.style.display = 'none';
    if(app) app.style.display = 'block';

    renderReminders();
  }

  // --- Auto-login if session present ---
  document.addEventListener('DOMContentLoaded', function(){
    const session = getCurrentUser();
    if(session){
      // small timeout to ensure elements are parsed
      setTimeout(()=>{ openAppFor(session); }, 50);
    }
  });

  // expose some functions (already attached to window for handlers)
  // window.* functions already attached above
  

  
})();
