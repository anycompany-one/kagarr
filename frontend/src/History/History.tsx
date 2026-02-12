import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { getHistory } from '../api';
import { HistoryResource } from '../types';
import './History.css';

function formatDate(dateStr: string): string {
  const d = new Date(dateStr);
  const now = new Date();
  const diff = now.getTime() - d.getTime();
  const mins = Math.floor(diff / 60000);
  const hours = Math.floor(diff / 3600000);
  const days = Math.floor(diff / 86400000);

  if (mins < 1) return 'Just now';
  if (mins < 60) return `${mins}m ago`;
  if (hours < 24) return `${hours}h ago`;
  if (days < 7) return `${days}d ago`;

  return d.toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
    year: d.getFullYear() !== now.getFullYear() ? 'numeric' : undefined,
  });
}

function eventBadgeClass(eventType: string): string {
  switch (eventType) {
    case 'Grabbed':
      return 'hy-event-grabbed';
    case 'Imported':
      return 'hy-event-imported';
    case 'ImportFailed':
      return 'hy-event-importfailed';
    case 'Deleted':
      return 'hy-event-deleted';
    default:
      return 'hy-event-deleted';
  }
}

function eventLabel(eventType: string): string {
  switch (eventType) {
    case 'ImportFailed':
      return 'Failed';
    default:
      return eventType;
  }
}

function History() {
  const [records, setRecords] = useState<HistoryResource[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchHistory = useCallback(async () => {
    try {
      const data = await getHistory();
      setRecords(data);
      setError('');
    } catch {
      setError('Failed to load history');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchHistory();
  }, [fetchHistory]);

  if (loading) {
    return (
      <div className="page-loading">
        <div className="spinner" />
      </div>
    );
  }

  return (
    <div>
      <div className="hy-header">
        <h1 className="hy-title">History</h1>
        <div className="hy-subtitle">
          Event log
          {records.length > 0 && (
            <span className="hy-count">{records.length}</span>
          )}
        </div>
      </div>

      {error && <div className="error-banner">{error}</div>}

      {!error && records.length === 0 && (
        <div className="hy-empty">
          <div className="hy-empty-icon">&#128220;</div>
          <div className="hy-empty-title">No history yet</div>
          <div className="hy-empty-hint">
            Grab some releases to get started
          </div>
        </div>
      )}

      {records.length > 0 && (
        <div className="hy-table-wrap">
          <table className="hy-table">
            <thead>
              <tr>
                <th>Date</th>
                <th>Event</th>
                <th>Game</th>
                <th>Source</th>
                <th>Details</th>
              </tr>
            </thead>
            <tbody>
              {records.map((record) => (
                <tr key={record.id}>
                  <td className="hy-col-date" title={new Date(record.date).toLocaleString()}>
                    {formatDate(record.date)}
                  </td>
                  <td>
                    <span className={`hy-event-badge ${eventBadgeClass(record.eventType)}`}>
                      {eventLabel(record.eventType)}
                    </span>
                  </td>
                  <td className="hy-col-game">
                    {record.gameId > 0 ? (
                      <Link to={`/game/${record.gameId}`} className="hy-game-link">
                        {record.gameTitle}
                      </Link>
                    ) : (
                      <span>{record.gameTitle}</span>
                    )}
                  </td>
                  <td className="hy-col-source" title={record.sourceTitle}>
                    {record.sourceTitle || '\u2014'}
                  </td>
                  <td className="hy-col-data" title={record.data}>
                    {record.data || '\u2014'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default History;
