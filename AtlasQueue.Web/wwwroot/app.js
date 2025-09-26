const API = (path) => `http://localhost:5000/${path.replace(/^\/+/,'')}`; // assume API on 5000, web on 5002

let page = 1, pageSize = 10;

async function fetchJSON(url, opts) {
  const res = await fetch(url, opts);
  if (!res.ok) throw new Error(await res.text());
  return res.json();
}

async function loadStats() {
  try {
    const s = await fetchJSON(API('/api/stats'));
    document.getElementById('stats').textContent = JSON.stringify(s, null, 2);
  } catch (e) {
    document.getElementById('stats').textContent = `Error: ${e}`;
  }
}

async function loadList() {
  const q = document.getElementById('q').value.trim();
  const status = document.getElementById('status').value || '';
  const u = new URL(API('/api/work-items'));
  u.searchParams.set('page', page);
  u.searchParams.set('pageSize', pageSize);
  if (q) u.searchParams.set('q', q);
  if (status) u.searchParams.set('status', status);

  const data = await fetchJSON(u);
  const tbody = document.getElementById('list');
  tbody.innerHTML = '';
  for (const w of data.items) {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td><code>${w.id.slice(0,8)}…</code></td>
      <td>${w.externalRef}</td>
      <td>${w.status}</td>
      <td>${w.priority}</td>
      <td>${w.createdUtc?.replace('T',' ').replace('Z','')}</td>
      <td>${w.processedUtc ? w.processedUtc.replace('T',' ').replace('Z','') : ''}</td>
      <td>${w.status === 'Failed' ? `<button data-id="${w.id}" class="retry">Retry</button>` : ''}</td>
    `;
    tbody.appendChild(tr);
  }
  document.getElementById('pageinfo').textContent = `Page ${data.page} of ${Math.ceil(data.total / data.pageSize)}`;

  // Bind retry buttons
  for (const btn of document.querySelectorAll('button.retry')) {
    btn.addEventListener('click', async (e) => {
      const id = e.target.getAttribute('data-id');
      await fetch(API(`/api/work-items/${id}/retry`), { method: 'POST' });
      await loadStats();
      await loadList();
    });
  }
}

document.getElementById('refresh').addEventListener('click', () => { page = 1; loadList(); loadStats(); });
document.getElementById('prev').addEventListener('click', () => { if (page > 1) { page--; loadList(); } });
document.getElementById('next').addEventListener('click', () => { page++; loadList(); });

document.getElementById('enqueue').addEventListener('click', async () => {
  const ext = document.getElementById('ext').value || `EXT-${Math.floor(Math.random()*9000)+1000}`;
  const payload = document.getElementById('payload').value || '{}';
  const prio = document.getElementById('prio').value;
  const dto = { externalRef: ext, payloadJson: payload, priority: prio };

  const r = await fetchJSON(API('/api/work-items'), {
    method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(dto)
  });
  document.getElementById('enqueueResult').textContent = `Queued ${r.id}`;
  await loadStats();
  await loadList();
});

(async function init(){
  await loadStats();
  await loadList();
})();
