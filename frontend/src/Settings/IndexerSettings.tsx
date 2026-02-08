import { useState, useEffect, useCallback } from 'react';

function getAuthHeaders(): Record<string, string> {
  const headers: Record<string, string> = { 'Content-Type': 'application/json' };
  const key = localStorage.getItem('kagarr_api_key');
  if (key) headers['X-Api-Key'] = key;
  return headers;
}

interface IndexerResource {
  id: number;
  name: string;
  implementation: string;
  settings: string;
  enableRss: boolean;
  enableSearch: boolean;
  priority: number;
}

interface IndexerForm {
  name: string;
  implementation: string;
  baseUrl: string;
  apiKey: string;
  categories: string;
  enableRss: boolean;
  enableSearch: boolean;
}

const EMPTY_FORM: IndexerForm = {
  name: '',
  implementation: 'torznab',
  baseUrl: '',
  apiKey: '',
  categories: '4000',
  enableRss: true,
  enableSearch: true,
};

function IndexerSettings() {
  const [indexers, setIndexers] = useState<IndexerResource[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState<IndexerForm>(EMPTY_FORM);
  const [editId, setEditId] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  const fetchIndexers = useCallback(async () => {
    try {
      const res = await fetch('/api/v1/indexer', { headers: getAuthHeaders() });
      setIndexers(await res.json());
    } catch {
      setError('Failed to load indexers');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchIndexers(); }, [fetchIndexers]);

  const handleSave = async () => {
    const settings = JSON.stringify({
      baseUrl: form.baseUrl,
      apiKey: form.apiKey,
      categories: form.categories,
    });

    const body = {
      name: form.name,
      implementation: form.implementation,
      settings,
      enableRss: form.enableRss,
      enableSearch: form.enableSearch,
      priority: 25,
    };

    try {
      const method = editId ? 'PUT' : 'POST';
      const url = editId ? `/api/v1/indexer/${editId}` : '/api/v1/indexer';
      await fetch(url, {
        method,
        headers: getAuthHeaders(),
        body: JSON.stringify(body),
      });
      setShowForm(false);
      setForm(EMPTY_FORM);
      setEditId(null);
      fetchIndexers();
    } catch {
      setError('Failed to save indexer');
    }
  };

  const handleEdit = (indexer: IndexerResource) => {
    const parsed = JSON.parse(indexer.settings || '{}');
    setForm({
      name: indexer.name,
      implementation: indexer.implementation,
      baseUrl: parsed.baseUrl || '',
      apiKey: parsed.apiKey || '',
      categories: parsed.categories || '4000',
      enableRss: indexer.enableRss,
      enableSearch: indexer.enableSearch,
    });
    setEditId(indexer.id);
    setShowForm(true);
  };

  const handleDelete = async (id: number) => {
    await fetch(`/api/v1/indexer/${id}`, { method: 'DELETE', headers: getAuthHeaders() });
    fetchIndexers();
  };

  if (loading) return <div className="page-loading"><div className="spinner" /><p>Loading...</p></div>;

  return (
    <div>
      <div className="settings-section-header">
        <h2>Indexers</h2>
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
            <label>Type</label>
            <select className="search-input" value={form.implementation} onChange={e => setForm({ ...form, implementation: e.target.value })}>
              <option value="torznab">Torznab</option>
              <option value="newznab">Newznab</option>
            </select>
          </div>
          <div className="form-group">
            <label>URL</label>
            <input className="search-input" value={form.baseUrl} onChange={e => setForm({ ...form, baseUrl: e.target.value })} placeholder="http://localhost:9696" />
          </div>
          <div className="form-group">
            <label>API Key</label>
            <input className="search-input" value={form.apiKey} onChange={e => setForm({ ...form, apiKey: e.target.value })} />
          </div>
          <div className="form-group">
            <label>Categories</label>
            <input className="search-input" value={form.categories} onChange={e => setForm({ ...form, categories: e.target.value })} />
          </div>
          <div className="form-actions">
            <button className="btn btn-primary btn-sm" onClick={handleSave}>Save</button>
            <button className="btn btn-sm" style={{ background: 'var(--bg-tertiary)', color: 'var(--text-primary)' }} onClick={() => { setShowForm(false); setEditId(null); }}>Cancel</button>
          </div>
        </div>
      )}

      <div className="settings-list">
        {indexers.map(idx => (
          <div key={idx.id} className="settings-item">
            <div className="settings-item-info">
              <strong>{idx.name}</strong>
              <span className="badge">{idx.implementation}</span>
              {idx.enableSearch && <span className="badge" style={{ background: 'var(--accent)', color: '#fff' }}>Search</span>}
            </div>
            <div className="settings-item-actions">
              <button className="btn btn-sm" style={{ background: 'var(--bg-tertiary)', color: 'var(--text-primary)' }} onClick={() => handleEdit(idx)}>Edit</button>
              <button className="btn btn-sm btn-danger" onClick={() => handleDelete(idx.id)}>Delete</button>
            </div>
          </div>
        ))}
        {indexers.length === 0 && !showForm && <p className="subtitle">No indexers configured. Add one to start searching.</p>}
      </div>
    </div>
  );
}

export default IndexerSettings;
