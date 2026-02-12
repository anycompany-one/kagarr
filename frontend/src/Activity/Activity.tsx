import { useState, useEffect, useRef } from 'react';
import { getQueue } from '../api';
import { QueueResource } from '../types';
import { formatSize } from '../utils';
import './Activity.css';

function statusClass(status: string): string {
  const s = status.toLowerCase();
  if (s.includes('download')) return 'aq-status-downloading';
  if (s.includes('pause')) return 'aq-status-paused';
  if (s.includes('complete')) return 'aq-status-completed';
  if (s.includes('queue') || s.includes('wait')) return 'aq-status-queued';
  if (s.includes('fail') || s.includes('error')) return 'aq-status-failed';
  return 'aq-status-queued';
}

function Activity() {
  const [queue, setQueue] = useState<QueueResource[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  useEffect(() => {
    let mounted = true;

    async function poll() {
      try {
        const data = await getQueue();
        if (mounted) {
          setQueue(data);
          setError(false);
        }
      } catch {
        if (mounted) setError(true);
      } finally {
        if (mounted) setLoading(false);
      }
    }

    poll();
    intervalRef.current = setInterval(poll, 5000);

    return () => {
      mounted = false;
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, []);

  if (loading) {
    return (
      <div className="page-loading">
        <div className="spinner" />
      </div>
    );
  }

  return (
    <div>
      <div className="aq-header">
        <h1 className="aq-title">Activity</h1>
        <div className="aq-subtitle">
          <span className="aq-live-dot" />
          Download queue
          {queue.length > 0 && <span className="aq-count">{queue.length}</span>}
        </div>
      </div>

      {error && (
        <div className="aq-error">
          Failed to load queue &mdash; retrying&hellip;
        </div>
      )}

      {!error && queue.length === 0 && (
        <div className="aq-empty">
          <div className="aq-empty-icon">&#9660;</div>
          <div className="aq-empty-title">No active downloads</div>
          <div className="aq-empty-hint">
            Grab some releases from a game&apos;s detail page to see them here
          </div>
        </div>
      )}

      {queue.length > 0 && (
        <div className="aq-table-wrap">
          <table className="aq-table">
            <thead>
              <tr>
                <th>Title</th>
                <th>Status</th>
                <th>Progress</th>
                <th>Size</th>
                <th>Client</th>
              </tr>
            </thead>
            <tbody>
              {queue.map((item) => {
                const downloaded = item.totalSize - item.remainingSize;
                const pct =
                  item.totalSize > 0
                    ? Math.round((downloaded / item.totalSize) * 100)
                    : 0;
                const isActive = item.status.toLowerCase().includes('download');
                const isComplete = pct >= 100;

                return (
                  <tr key={item.downloadId}>
                    <td className="aq-col-title" title={item.title}>
                      {item.title}
                    </td>
                    <td>
                      <span className={`aq-status-badge ${statusClass(item.status)}`}>
                        {item.status}
                      </span>
                    </td>
                    <td className="aq-progress-cell">
                      <div className="aq-progress-wrap">
                        <div className="aq-progress-bar">
                          <div
                            className={`aq-progress-fill ${
                              isComplete
                                ? 'aq-progress-fill--complete'
                                : isActive
                                ? 'aq-progress-fill--active'
                                : ''
                            }`}
                            style={{ width: `${pct}%` }}
                          />
                        </div>
                        <span className="aq-progress-pct">{pct}%</span>
                      </div>
                    </td>
                    <td className="aq-col-size">
                      {formatSize(downloaded)} / {formatSize(item.totalSize)}
                    </td>
                    <td className="aq-col-client">{item.downloadClient}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default Activity;
