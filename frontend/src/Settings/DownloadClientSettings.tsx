import { useState, useEffect, useCallback } from 'react';

interface DownloadClientResource {
  id: number;
  name: string;
  implementation: string;
  settings: string;
  protocol: string;
  priority: number;
  enable: boolean;
}

interface ClientForm {
  name: string;
  implementation: string;
  host: string;
  port: string;
  username: string;
  password: string;
  apiKey: string;
  category: string;
  useSsl: boolean;
  enable: boolean;
}

const EMPTY_FORM: ClientForm = {
  name: '',
  implementation: 'qbittorrent',
  host: 'localhost',
  port: '8080',
  username: '',
  password: '',
  apiKey: '',
  category: 'kagarr',
  useSsl: false,
  enable: true,
};

function DownloadClientSettings() {
  const [clients, setClients] = useState<DownloadClientResource[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState<ClientForm>(EMPTY_FORM);
  const [editId, setEditId] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  const fetchClients = useCallback(async () => {
    try {
      const res = await fetch('/api/v1/downloadclient');
      setClients(await res.json());
    } catch {
      setError('Failed to load download clients');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchClients(); }, [fetchClients]);

  const isSabnzbd = form.implementation === 'sabnzbd';

  const handleSave = async () => {
    const settings = isSabnzbd
      ? JSON.stringify({ host: form.host, port: parseInt(form.port, 10), apiKey: form.apiKey, category: form.category, useSsl: form.useSsl })
      : JSON.stringify({ host: form.host, port: parseInt(form.port, 10), username: form.username, password: form.password, category: form.category, useSsl: form.useSsl });

    const body = {
      name: form.name,
      implementation: form.implementation,
      settings,
      protocol: isSabnzbd ? 'usenet' : 'torrent',
      priority: 1,
      enable: form.enable,
    };

    try {
      const method = editId ? 'PUT' : 'POST';
      const url = editId ? `/api/v1/downloadclient/${editId}` : '/api/v1/downloadclient';
      await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
      });
      setShowForm(false);
      setForm(EMPTY_FORM);
      setEditId(null);
      fetchClients();
    } catch {
      setError('Failed to save download client');
    }
  };

  const handleEdit = (client: DownloadClientResource) => {
    const parsed = JSON.parse(client.settings || '{}');
    setForm({
      name: client.name,
      implementation: client.implementation,
      host: parsed.host || 'localhost',
      port: String(parsed.port || 8080),
      username: parsed.username || '',
      password: parsed.password || '',
      apiKey: parsed.apiKey || '',
      category: parsed.category || 'kagarr',
      useSsl: parsed.useSsl || false,
      enable: client.enable,
    });
    setEditId(client.id);
    setShowForm(true);
  };

  const handleDelete = async (id: number) => {
    await fetch(`/api/v1/downloadclient/${id}`, { method: 'DELETE' });
    fetchClients();
  };

  if (loading) return <div className="page-loading"><div className="spinner" /><p>Loading...</p></div>;

  return (
    <div>
      <div className="settings-section-header">
        <h2>Download Clients</h2>
        <button className="btn btn-primary btn-sm" onClick={() => { setShowForm(true); setEditId(null); setForm(EMPTY_FORM); }}>
          + Add
        </button>
      </div>
      {error && <div className="error-banner">{error}</div>}

      {showForm && (
        <div className="settings-form">
          <div className="form-group">
            <label>Name</label>
            <input className="search-input" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} />
          </div>
          <div className="form-group">
            <label>Client</label>
            <select className="search-input" value={form.implementation} onChange={e => setForm({ ...form, implementation: e.target.value })}>
              <option value="qbittorrent">qBittorrent</option>
              <option value="sabnzbd">SABnzbd</option>
            </select>
          </div>
          <div className="form-group">
            <label>Host</label>
            <input className="search-input" value={form.host} onChange={e => setForm({ ...form, host: e.target.value })} />
          </div>
          <div className="form-group">
            <label>Port</label>
            <input className="search-input" type="number" value={form.port} onChange={e => setForm({ ...form, port: e.target.value })} />
          </div>
          {!isSabnzbd && (
            <>
              <div className="form-group">
                <label>Username</label>
                <input className="search-input" value={form.username} onChange={e => setForm({ ...form, username: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Password</label>
                <input className="search-input" type="password" value={form.password} onChange={e => setForm({ ...form, password: e.target.value })} />
              </div>
            </>
          )}
          {isSabnzbd && (
            <div className="form-group">
              <label>API Key</label>
              <input className="search-input" value={form.apiKey} onChange={e => setForm({ ...form, apiKey: e.target.value })} />
            </div>
          )}
          <div className="form-group">
            <label>Category</label>
            <input className="search-input" value={form.category} onChange={e => setForm({ ...form, category: e.target.value })} />
          </div>
          <div className="form-actions">
            <button className="btn btn-primary btn-sm" onClick={handleSave}>Save</button>
            <button className="btn btn-sm" style={{ background: 'var(--bg-tertiary)', color: 'var(--text-primary)' }} onClick={() => { setShowForm(false); setEditId(null); }}>Cancel</button>
          </div>
        </div>
      )}

      <div className="settings-list">
        {clients.map(c => (
          <div key={c.id} className="settings-item">
            <div className="settings-item-info">
              <strong>{c.name}</strong>
              <span className="badge">{c.implementation}</span>
              <span className="badge">{c.protocol}</span>
              {!c.enable && <span className="badge" style={{ background: 'var(--danger)', color: '#fff' }}>Disabled</span>}
            </div>
            <div className="settings-item-actions">
              <button className="btn btn-sm" style={{ background: 'var(--bg-tertiary)', color: 'var(--text-primary)' }} onClick={() => handleEdit(c)}>Edit</button>
              <button className="btn btn-sm btn-danger" onClick={() => handleDelete(c.id)}>Delete</button>
            </div>
          </div>
        ))}
        {clients.length === 0 && !showForm && <p className="subtitle">No download clients configured.</p>}
      </div>
    </div>
  );
}

export default DownloadClientSettings;
