import { useState, useCallback, useRef } from 'react';
import { GameResource } from '../types';
import { searchGames, addGame } from '../api';
import GameCard from './GameCard';

function AddGame() {
  const [term, setTerm] = useState('');
  const [results, setResults] = useState<GameResource[]>([]);
  const [searching, setSearching] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [addedIds, setAddedIds] = useState<Set<number>>(new Set());
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const doSearch = useCallback(async (searchTerm: string) => {
    if (searchTerm.trim().length < 2) {
      setResults([]);
      return;
    }

    try {
      setSearching(true);
      setError(null);
      const data = await searchGames(searchTerm.trim());
      setResults(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Search failed');
      setResults([]);
    } finally {
      setSearching(false);
    }
  }, []);

  const handleInput = (value: string) => {
    setTerm(value);
    if (debounceRef.current) {
      clearTimeout(debounceRef.current);
    }
    debounceRef.current = setTimeout(() => doSearch(value), 500);
  };

  const handleAdd = async (game: GameResource) => {
    try {
      setError(null);
      await addGame({
        title: game.title,
        igdbId: game.igdbId,
        year: game.year,
        overview: game.overview,
        platform: game.platform,
        genres: game.genres,
        developer: game.developer,
        publisher: game.publisher,
        releaseDate: game.releaseDate,
        images: game.images,
        remoteCover: game.remoteCover,
        monitored: true,
      });
      setAddedIds((prev) => new Set(prev).add(game.igdbId));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to add game');
    }
  };

  return (
    <div className="add-game">
      <div className="add-game-header">
        <h1>Add Game</h1>
        <p className="subtitle">Search IGDB to find games and add them to your library.</p>
      </div>

      <div className="search-bar">
        <input
          type="text"
          className="search-input search-input-lg"
          placeholder="Search for a game..."
          value={term}
          onChange={(e) => handleInput(e.target.value)}
          autoFocus
        />
        {searching && <div className="search-spinner" />}
      </div>

      {error && <div className="error-banner">{error}</div>}

      {results.length > 0 && (
        <div className="search-results-count">
          {results.length} result{results.length !== 1 ? 's' : ''}
        </div>
      )}

      <div className="game-grid">
        {results.map((game) => (
          <GameCard
            key={game.igdbId}
            game={game}
            isSearchResult
            onAdd={addedIds.has(game.igdbId) ? undefined : handleAdd}
          />
        ))}
      </div>

      {term.length >= 2 && !searching && results.length === 0 && (
        <div className="empty-state">
          <p>No results found for "{term}"</p>
          <p className="subtitle">Make sure your IGDB credentials are configured.</p>
        </div>
      )}
    </div>
  );
}

export default AddGame;
