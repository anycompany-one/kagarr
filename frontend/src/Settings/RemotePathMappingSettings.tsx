import { useState, useEffect, useCallback } from 'react';
import {
  getRemotePathMappings,
  addRemotePathMapping,
  deleteRemotePathMapping,
} from '../api';
import { RemotePathMappingResource } from '../types';

function RemotePathMappingSettings() {
  const [mappings, setMappings] = useState<RemotePathMappingResource[]>([]);
  const [loading, setLoading] = useState(true);
  const [host, setHost] = useState('');
  const [remotePath, setRemotePath] = useState('');
  const [localPath, setLocalPath] = useState('');
  const [saving, setSaving] = useState(false);

  const fetchMappings = useCallback(async () => {
    try {
      const data = await getRemotePathMappings();
      setMappings(data);
    } catch {
      // API may not be ready
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchMappings();
  }, [fetchMappings]);

  const handleAdd = async () => {
    if (!host.trim() || !remotePath.trim() || !localPath.trim()) return;

    setSaving(true);
    try {
      await addRemotePathMapping({
        host: host.trim(),
        remotePath: remotePath.trim(),
        localPath: localPath.trim(),
      });
      setHost('');
      setRemotePath('');
      setLocalPath('');
      await fetchMappings();
    } catch (err) {
      console.error('Failed to add mapping:', err);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: number) => {
    try {
      await deleteRemotePathMapping(id);
      await fetchMappings();
    } catch (err) {
      console.error('Failed to delete mapping:', err);
    }
  };

  if (loading) {
    return <div style={{ padding: '1.5rem', color: '#888' }}>Loading...</div>;
  }

  return (
    <div style={{ padding: '1.5rem' }}>
      <h2 style={{ marginTop: 0, marginBottom: '0.5rem' }}>Remote Path Mappings</h2>
      <p style={{ color: '#888', fontSize: '0.85rem', marginBottom: '1.5rem' }}>
        Map download client paths to local paths when Kagarr and your download client
        see different filesystem layouts (e.g., different Docker containers).
      </p>

      {mappings.length > 0 && (
        <table style={{ width: '100%', borderCollapse: 'collapse', marginBottom: '1.5rem' }}>
          <thead>
            <tr style={{ borderBottom: '1px solid #333' }}>
              <th style={thStyle}>Host</th>
              <th style={thStyle}>Remote Path</th>
              <th style={thStyle}>Local Path</th>
              <th style={{ ...thStyle, width: 60 }}></th>
            </tr>
          </thead>
          <tbody>
            {mappings.map((m) => (
              <tr key={m.id} style={{ borderBottom: '1px solid #2a2a3e' }}>
                <td style={tdStyle}>{m.host}</td>
                <td style={tdStyle}>
                  <code style={{ fontSize: '0.85rem' }}>{m.remotePath}</code>
                </td>
                <td style={tdStyle}>
                  <code style={{ fontSize: '0.85rem' }}>{m.localPath}</code>
                </td>
                <td style={tdStyle}>
                  <button
                    onClick={() => handleDelete(m.id)}
                    style={{
                      background: '#c0392b',
                      color: '#fff',
                      border: 'none',
                      padding: '4px 10px',
                      borderRadius: 4,
                      cursor: 'pointer',
                      fontSize: '0.8rem',
                    }}
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {mappings.length === 0 && (
        <div
          style={{
            color: '#666',
            padding: '1rem',
            textAlign: 'center',
            border: '1px dashed #333',
            borderRadius: 6,
            marginBottom: '1.5rem',
          }}
        >
          No remote path mappings configured. Add one below if your download client
          reports paths that differ from what Kagarr sees on disk.
        </div>
      )}

      <div
        style={{
          display: 'grid',
          gridTemplateColumns: '1fr 1fr 1fr auto',
          gap: '0.75rem',
          alignItems: 'end',
        }}
      >
        <label style={labelStyle}>
          <span>Host</span>
          <input
            type="text"
            value={host}
            onChange={(e) => setHost(e.target.value)}
            placeholder="e.g. 192.168.1.50"
            style={inputStyle}
          />
        </label>
        <label style={labelStyle}>
          <span>Remote Path</span>
          <input
            type="text"
            value={remotePath}
            onChange={(e) => setRemotePath(e.target.value)}
            placeholder="e.g. /downloads/"
            style={inputStyle}
          />
        </label>
        <label style={labelStyle}>
          <span>Local Path</span>
          <input
            type="text"
            value={localPath}
            onChange={(e) => setLocalPath(e.target.value)}
            placeholder="e.g. /mnt/data/downloads/"
            style={inputStyle}
          />
        </label>
        <button
          onClick={handleAdd}
          disabled={saving || !host.trim() || !remotePath.trim() || !localPath.trim()}
          style={{
            background: '#2ecc71',
            color: '#fff',
            border: 'none',
            padding: '8px 16px',
            borderRadius: 4,
            cursor: 'pointer',
            fontWeight: 600,
            opacity:
              saving || !host.trim() || !remotePath.trim() || !localPath.trim() ? 0.5 : 1,
          }}
        >
          {saving ? 'Adding...' : 'Add'}
        </button>
      </div>
    </div>
  );
}

const thStyle: React.CSSProperties = {
  textAlign: 'left',
  padding: '0.5rem 0.75rem',
  color: '#aaa',
  fontSize: '0.8rem',
  textTransform: 'uppercase',
  letterSpacing: '0.05em',
};

const tdStyle: React.CSSProperties = {
  padding: '0.5rem 0.75rem',
  color: '#ddd',
};

const labelStyle: React.CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: '0.25rem',
  color: '#aaa',
  fontSize: '0.85rem',
};

const inputStyle: React.CSSProperties = {
  background: '#1a1a2e',
  border: '1px solid #333',
  borderRadius: 4,
  padding: '8px 10px',
  color: '#eee',
  fontSize: '0.9rem',
};

export default RemotePathMappingSettings;
