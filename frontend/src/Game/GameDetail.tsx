import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import {
  getGame,
  deleteGame,
  searchReleases,
  grabRelease,
  scanForImport,
  importFiles,
} from '../api';
import { GameResource, ReleaseResource, ImportResultResource } from '../types';
import './GameDetail.css';

function formatSize(bytes: number): string {
  if (bytes === 0) return '0 B';
  const units = ['B', 'KB', 'MB', 'GB', 'TB'];
  const i = Math.floor(Math.log(bytes) / Math.log(1024));
  return `${(bytes / Math.pow(1024, i)).toFixed(i > 1 ? 1 : 0)} ${units[i]}`;
}

function formatAge(dateStr: string): string {
  const diff = Date.now() - new Date(dateStr).getTime();
  const days = Math.floor(diff / 86400000);
  if (days < 1) return 'Today';
  if (days === 1) return '1d';
  if (days < 30) return `${days}d`;
  if (days < 365) return `${Math.floor(days / 30)}mo`;
  return `${Math.floor(days / 365)}y`;
}

function seederClass(s: number): string {
  if (s >= 10) return 'gd-seeders-good';
  if (s >= 3) return 'gd-seeders-ok';
  return 'gd-seeders-low';
}

function GameDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [game, setGame] = useState<GameResource | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Release search state
  const [releases, setReleases] = useState<ReleaseResource[]>([]);
  const [searchLoading, setSearchLoading] = useState(false);
  const [searchDone, setSearchDone] = useState(false);
  const [grabbedGuids, setGrabbedGuids] = useState<Set<string>>(new Set());
  const [grabbing, setGrabbing] = useState<Set<string>>(new Set());

  // Import state
  const [importPath, setImportPath] = useState('/downloads');
  const [scanning, setScanning] = useState(false);
  const [foundFiles, setFoundFiles] = useState<string[]>([]);
  const [selectedFiles, setSelectedFiles] = useState<Set<string>>(new Set());
  const [importing, setImporting] = useState(false);
  const [importResults, setImportResults] = useState<ImportResultResource[]>([]);

  useEffect(() => {
    if (!id) return;
    setLoading(true);
    getGame(Number(id))
      .then(setGame)
      .catch(() => setError('Failed to load game'))
      .finally(() => setLoading(false));
  }, [id]);

  const handleSearch = useCallback(async () => {
    if (!game) return;
    setSearchLoading(true);
    setSearchDone(false);
    setReleases([]);
    try {
      const results = await searchReleases(game.title);
      setReleases(results);
    } catch {
      setError('Release search failed');
    } finally {
      setSearchLoading(false);
      setSearchDone(true);
    }
  }, [game]);

  const handleGrab = useCallback(async (release: ReleaseResource) => {
    setGrabbing((prev) => new Set(prev).add(release.guid));
    try {
      await grabRelease(release, game?.id, game?.title);
      setGrabbedGuids((prev) => new Set(prev).add(release.guid));
    } catch {
      setError('Grab failed — check your download client settings');
    } finally {
      setGrabbing((prev) => {
        const next = new Set(prev);
        next.delete(release.guid);
        return next;
      });
    }
  }, [game]);

  const handleDelete = useCallback(async () => {
    if (!game) return;
    if (!window.confirm(`Remove "${game.title}" from your library?`)) return;
    try {
      await deleteGame(game.id);
      navigate('/');
    } catch {
      setError('Failed to delete game');
    }
  }, [game, navigate]);

  const handleScan = useCallback(async () => {
    setScanning(true);
    setFoundFiles([]);
    setSelectedFiles(new Set());
    setImportResults([]);
    try {
      const files = await scanForImport(importPath);
      setFoundFiles(files);
      setSelectedFiles(new Set(files));
    } catch {
      setError('Scan failed — check the path');
    } finally {
      setScanning(false);
    }
  }, [importPath]);

  const handleImport = useCallback(async () => {
    if (!game || selectedFiles.size === 0) return;
    setImporting(true);
    setImportResults([]);
    try {
      const results = await importFiles({
        path: importPath,
        gameId: game.id,
        files: Array.from(selectedFiles),
      });
      setImportResults(results);
    } catch {
      setError('Import failed');
    } finally {
      setImporting(false);
    }
  }, [game, importPath, selectedFiles]);

  const toggleFile = (file: string) => {
    setSelectedFiles((prev) => {
      const next = new Set(prev);
      if (next.has(file)) next.delete(file);
      else next.add(file);
      return next;
    });
  };

  if (loading) {
    return (
      <div className="page-loading">
        <div className="spinner" />
      </div>
    );
  }

  if (!game) {
    return (
      <div className="gd-empty">
        <p>Game not found</p>
        <Link to="/" className="gd-back">
          <span className="gd-back-arrow">&larr;</span> Back to Library
        </Link>
      </div>
    );
  }

  const coverImage = game.images?.find((i) => i.coverType === 'Cover');
  const coverUrl = coverImage?.remoteUrl || coverImage?.url || game.remoteCover;

  return (
    <div>
      {/* Back link */}
      <Link to="/" className="gd-back">
        <span className="gd-back-arrow">&larr;</span> Library
      </Link>

      {/* Error banner */}
      {error && (
        <div className="error-banner" style={{ marginBottom: '1rem' }}>
          {error}
          <button
            onClick={() => setError('')}
            style={{
              marginLeft: '1rem',
              background: 'none',
              border: 'none',
              color: 'inherit',
              cursor: 'pointer',
              fontWeight: 'bold',
            }}
          >
            &times;
          </button>
        </div>
      )}

      {/* Hero header */}
      <div className="gd-hero">
        <div className="gd-cover">
          {coverUrl ? (
            <img src={coverUrl} alt={game.title} />
          ) : (
            <div className="gd-cover-empty">&#127918;</div>
          )}
        </div>

        <div className="gd-info">
          <div className="gd-title-row">
            <h1 className="gd-title">{game.title}</h1>
            {game.year > 0 && <span className="gd-year">({game.year})</span>}
          </div>

          <div className="gd-meta">
            {game.platform && game.platform !== 'Unknown' && (
              <>
                <span className="gd-meta-label">Platform</span>
                <span className="gd-meta-value">
                  {game.platform.replace('_', ' ')}
                </span>
                <span className="gd-meta-divider" />
              </>
            )}
            {game.developer && (
              <>
                <span className="gd-meta-label">Developer</span>
                <span className="gd-meta-value">{game.developer}</span>
                <span className="gd-meta-divider" />
              </>
            )}
            {game.publisher && (
              <>
                <span className="gd-meta-label">Publisher</span>
                <span className="gd-meta-value">{game.publisher}</span>
              </>
            )}
          </div>

          {game.genres && game.genres.length > 0 && (
            <div className="gd-genres">
              {game.genres.map((g) => (
                <span key={g} className="gd-genre-tag">
                  {g}
                </span>
              ))}
            </div>
          )}

          {game.overview && <p className="gd-overview">{game.overview}</p>}

          <div className="gd-actions">
            <button
              className="gd-btn-search"
              onClick={handleSearch}
              disabled={searchLoading}
            >
              {searchLoading ? (
                <>
                  <span className="spinner" style={{ width: 14, height: 14, margin: 0 }} />
                  Searching&hellip;
                </>
              ) : (
                <>&#128269; Search Releases</>
              )}
            </button>
            <button className="gd-btn-delete" onClick={handleDelete}>
              Delete Game
            </button>
          </div>
        </div>
      </div>

      {/* Release search results */}
      {(searchLoading || searchDone) && (
        <div className="gd-releases">
          <div className="gd-section-header">
            <h2>
              Releases
              {releases.length > 0 && (
                <span className="gd-release-count">
                  {' '}
                  ({releases.length})
                </span>
              )}
            </h2>
          </div>

          {searchLoading && (
            <div className="gd-release-loading">
              <div className="spinner" />
              Searching indexers&hellip;
            </div>
          )}

          {searchDone && !searchLoading && releases.length === 0 && (
            <div className="gd-empty">No releases found</div>
          )}

          {releases.length > 0 && (
            <div className="gd-table-wrap">
              <table className="gd-table">
                <thead>
                  <tr>
                    <th>Title</th>
                    <th>Indexer</th>
                    <th>Size</th>
                    <th>Seeders</th>
                    <th>Protocol</th>
                    <th>Age</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {releases.map((r) => (
                    <tr key={r.guid}>
                      <td className="gd-col-title" title={r.title}>
                        {r.title}
                      </td>
                      <td className="gd-col-indexer">{r.indexer}</td>
                      <td className="gd-col-size">{formatSize(r.size)}</td>
                      <td className="gd-col-seeders">
                        {r.downloadProtocol === 'Torrent' ? (
                          <span className={seederClass(r.seeders)}>
                            {r.seeders}
                          </span>
                        ) : (
                          <span style={{ color: 'var(--text-muted)' }}>&mdash;</span>
                        )}
                      </td>
                      <td>
                        <span
                          className={`gd-protocol-badge ${
                            r.downloadProtocol === 'Torrent'
                              ? 'gd-protocol-torrent'
                              : 'gd-protocol-usenet'
                          }`}
                        >
                          {r.downloadProtocol === 'Torrent' ? 'Torrent' : 'NZB'}
                        </span>
                      </td>
                      <td className="gd-col-age">{formatAge(r.publishDate)}</td>
                      <td>
                        <button
                          className={`gd-grab-btn ${
                            grabbedGuids.has(r.guid) ? 'gd-grab-btn--grabbed' : ''
                          }`}
                          onClick={() => handleGrab(r)}
                          disabled={
                            grabbing.has(r.guid) || grabbedGuids.has(r.guid)
                          }
                        >
                          {grabbedGuids.has(r.guid)
                            ? '\u2713 Grabbed'
                            : grabbing.has(r.guid)
                            ? 'Sending\u2026'
                            : 'Grab'}
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* Manual import */}
      <div className="gd-import">
        <div className="gd-section-header">
          <h2>Manual Import</h2>
        </div>

        <div className="gd-import-row">
          <input
            className="gd-import-path"
            type="text"
            value={importPath}
            onChange={(e) => setImportPath(e.target.value)}
            placeholder="/path/to/downloads"
          />
          <button
            className="gd-btn-search"
            onClick={handleScan}
            disabled={scanning || !importPath}
          >
            {scanning ? 'Scanning\u2026' : 'Scan'}
          </button>
        </div>

        {foundFiles.length > 0 && (
          <>
            <div className="gd-import-files">
              {foundFiles.map((file) => (
                <label
                  key={file}
                  className={`gd-import-file ${
                    selectedFiles.has(file) ? 'gd-import-file--selected' : ''
                  }`}
                >
                  <input
                    type="checkbox"
                    checked={selectedFiles.has(file)}
                    onChange={() => toggleFile(file)}
                  />
                  <span className="gd-import-file-path">{file}</span>
                </label>
              ))}
            </div>

            <div className="gd-import-actions">
              <button
                className="gd-btn-search"
                onClick={handleImport}
                disabled={importing || selectedFiles.size === 0}
              >
                {importing
                  ? 'Importing\u2026'
                  : `Import Selected (${selectedFiles.size})`}
              </button>
            </div>
          </>
        )}

        {importResults.length > 0 && (
          <div className="gd-import-results">
            {importResults.map((r, i) => (
              <div
                key={i}
                className={`gd-import-result ${
                  r.success
                    ? 'gd-import-result--success'
                    : 'gd-import-result--fail'
                }`}
              >
                <span className="gd-import-result-icon">
                  {r.success ? '\u2713' : '\u2717'}
                </span>
                <span className="gd-import-result-path">{r.sourcePath}</span>
                {!r.success && r.errors.length > 0 && (
                  <span> &mdash; {r.errors[0]}</span>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

export default GameDetail;
