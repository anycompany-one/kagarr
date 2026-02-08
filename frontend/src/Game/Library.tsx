import { useState, useEffect, useCallback } from 'react';
import { GameResource } from '../types';
import { getGames, deleteGame } from '../api';
import GameCard from './GameCard';

function Library() {
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
      setError(err instanceof Error ? err.message : 'Failed to load games');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchGames();
  }, [fetchGames]);

  const handleDelete = async (id: number) => {
    try {
      await deleteGame(id);
      setGames((prev) => prev.filter((g) => g.id !== id));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete game');
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
        <p>Loading library...</p>
      </div>
    );
  }

  return (
    <div className="library">
      <div className="library-header">
        <h1>Library</h1>
        <div className="library-stats">
          {games.length} {games.length === 1 ? 'game' : 'games'}
        </div>
      </div>

      {games.length > 0 && (
        <div className="library-toolbar">
          <input
            type="text"
            className="search-input"
            placeholder="Filter library..."
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
              <h2>Your library is empty</h2>
              <p>Add some games to get started.</p>
              <a href="/add" className="btn btn-primary">
                Add Game
              </a>
            </>
          ) : (
            <>
              <p>No games match your filter.</p>
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
