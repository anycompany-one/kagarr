import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { GameResource } from '../types';
import { getGames, deleteGame } from '../api';
import GameCard from './GameCard';

function Library() {
  const { t } = useTranslation();
  const [games, setGames] = useState<GameResource[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState('');

  const fetchGames = useCallback(async () => {
    try {
      setLoading(true);
      const data = await getGames();
      setGames(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : t('library.failedToLoad'));
    } finally {
      setLoading(false);
    }
  }, [t]);

  useEffect(() => {
    fetchGames();
  }, [fetchGames]);

  const handleDelete = async (id: number) => {
    try {
      await deleteGame(id);
      setGames((prev) => prev.filter((g) => g.id !== id));
    } catch (err) {
      setError(err instanceof Error ? err.message : t('library.failedToDelete'));
    }
  };

  const filtered = games.filter(
    (g) =>
      g.title.toLowerCase().includes(filter.toLowerCase()) ||
      g.developer?.toLowerCase().includes(filter.toLowerCase()) ||
      g.platform?.toLowerCase().includes(filter.toLowerCase()),
  );

  if (loading) {
    return (
      <div className="page-loading">
        <div className="spinner" />
        <p>{t('library.loadingLibrary')}</p>
      </div>
    );
  }

  return (
    <div className="library">
      <div className="library-header">
        <h1>{t('library.title')}</h1>
        <div className="library-stats">
          {games.length} {games.length === 1 ? t('library.game') : t('library.games')}
        </div>
      </div>

      {games.length > 0 && (
        <div className="library-toolbar">
          <input
            type="text"
            className="search-input"
            placeholder={t('library.searchPlaceholder')}
            value={filter}
            onChange={(e) => setFilter(e.target.value)}
          />
        </div>
      )}

      {error && <div className="error-banner">{error}</div>}

      {filtered.length === 0 && !loading && (
        <div className="empty-state">
          {games.length === 0 ? (
            <>
              <div className="empty-icon">&#127918;</div>
              <h2>{t('library.empty')}</h2>
              <p>{t('library.emptyHint')}</p>
              <a href="/add" className="btn btn-primary">
                {t('library.addGame')}
              </a>
            </>
          ) : (
            <>
              <p>{t('library.noMatch')}</p>
            </>
          )}
        </div>
      )}

      <div className="game-grid">
        {filtered.map((game) => (
          <GameCard key={game.id} game={game} onDelete={handleDelete} />
        ))}
      </div>
    </div>
  );
}

export default Library;
