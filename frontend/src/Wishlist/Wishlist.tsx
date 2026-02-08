import { useState, useEffect, useCallback, useRef } from 'react';
import { GameResource, WishlistResource, GameDealResource } from '../types';
import {
  searchGames,
  getWishlist,
  addToWishlist,
  updateWishlistItem,
  removeFromWishlist,
  checkDeals,
  checkAllDeals,
  getDeals,
} from '../api';
import './Wishlist.css';

function timeAgo(dateStr: string | null): string {
  if (!dateStr) return 'never';
  const now = Date.now();
  const then = new Date(dateStr).getTime();
  const diff = now - then;
  const mins = Math.floor(diff / 60000);
  if (mins < 1) return 'just now';
  if (mins < 60) return `${mins}m ago`;
  const hrs = Math.floor(mins / 60);
  if (hrs < 24) return `${hrs}h ago`;
  const days = Math.floor(hrs / 24);
  return `${days}d ago`;
}

function formatPrice(price: number | null | undefined): string {
  if (price === null || price === undefined) return '';
  if (price === 0) return 'FREE';
  return `$${price.toFixed(2)}`;
}

// Inline SVG icons to avoid dependencies
function SearchIcon() {
  return (
    <svg viewBox="0 0 20 20" fill="currentColor">
      <path
        fillRule="evenodd"
        d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z"
        clipRule="evenodd"
      />
    </svg>
  );
}

function RefreshIcon() {
  return (
    <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round">
      <path d="M1.5 8a6.5 6.5 0 0111.48-4.16M14.5 8a6.5 6.5 0 01-11.48 4.16" />
      <path d="M13 1v3.5h-3.5M3 15v-3.5h3.5" />
    </svg>
  );
}

function TrashIcon() {
  return (
    <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round">
      <path d="M2.5 4.5h11M5.5 4.5V3a1 1 0 011-1h3a1 1 0 011 1v1.5M12 4.5l-.5 8.5a1 1 0 01-1 1h-5a1 1 0 01-1-1L4 4.5" />
    </svg>
  );
}

function ChevronIcon({ open }: { open: boolean }) {
  return (
    <svg
      viewBox="0 0 16 16"
      fill="currentColor"
      style={{
        width: 12,
        height: 12,
        transition: 'transform 0.2s',
        transform: open ? 'rotate(90deg)' : 'rotate(0)',
        marginRight: 4,
        opacity: 0.4,
      }}
    >
      <path d="M6 3l5 5-5 5V3z" />
    </svg>
  );
}

function Wishlist() {
  const [items, setItems] = useState<WishlistResource[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Search state
  const [searchTerm, setSearchTerm] = useState('');
  const [searchResults, setSearchResults] = useState<GameResource[]>([]);
  const [searching, setSearching] = useState(false);
  const [addedIgdbIds, setAddedIgdbIds] = useState<Set<number>>(new Set());
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const searchRef = useRef<HTMLDivElement>(null);

  // Expansion state
  const [expandedId, setExpandedId] = useState<number | null>(null);
  const [expandedDeals, setExpandedDeals] = useState<GameDealResource[]>([]);
  const [loadingDeals, setLoadingDeals] = useState(false);

  // Checking state
  const [checkingId, setCheckingId] = useState<number | null>(null);
  const [checkingAll, setCheckingAll] = useState(false);

  // Threshold editing
  const [editingThreshold, setEditingThreshold] = useState<Record<number, string>>({});

  const fetchWishlist = useCallback(async () => {
    try {
      setLoading(true);
      const data = await getWishlist();
      setItems(data);
      setAddedIgdbIds(new Set(data.map((d) => d.igdbId)));
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load wishlist');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchWishlist();
  }, [fetchWishlist]);

  // Close search dropdown on outside click
  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (searchRef.current && !searchRef.current.contains(e.target as Node)) {
        setSearchResults([]);
      }
    }
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, []);

  // Search
  const doSearch = useCallback(async (term: string) => {
    if (term.trim().length < 2) {
      setSearchResults([]);
      return;
    }
    try {
      setSearching(true);
      const data = await searchGames(term.trim());
      setSearchResults(data);
    } catch {
      setSearchResults([]);
    } finally {
      setSearching(false);
    }
  }, []);

  const handleSearchInput = (value: string) => {
    setSearchTerm(value);
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(() => doSearch(value), 400);
  };

  const handleAddToWishlist = async (game: GameResource) => {
    try {
      setError(null);
      await addToWishlist({
        title: game.title,
        igdbId: game.igdbId,
        year: game.year,
        overview: game.overview,
        steamAppId: game.steamAppId,
        platform: game.platform,
        genres: game.genres,
        developer: game.developer,
        publisher: game.publisher,
        releaseDate: game.releaseDate,
        images: game.images,
        remoteCover: game.remoteCover,
        notifyOnAnyDeal: true,
      });
      setAddedIgdbIds((prev) => new Set(prev).add(game.igdbId));
      setSearchResults([]);
      setSearchTerm('');
      await fetchWishlist();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to add to wishlist');
    }
  };

  const handleRemove = async (id: number) => {
    try {
      await removeFromWishlist(id);
      setItems((prev) => prev.filter((i) => i.id !== id));
      if (expandedId === id) setExpandedId(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to remove');
    }
  };

  const handleCheckDeals = async (id: number) => {
    try {
      setCheckingId(id);
      const result = await checkDeals(id);
      setItems((prev) =>
        prev.map((item) =>
          item.id === id
            ? {
                ...item,
                currentLowestPrice: result.lowestPrice,
                currentLowestStore: result.lowestPriceStore,
                lastDealCheck: result.lastChecked,
              }
            : item,
        ),
      );
      if (expandedId === id) {
        setExpandedDeals(result.deals);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Deal check failed');
    } finally {
      setCheckingId(null);
    }
  };

  const handleCheckAll = async () => {
    try {
      setCheckingAll(true);
      setError(null);
      const results = await checkAllDeals();
      const priceMap = new Map(results.map((r) => [r.wishlistItemId, r]));
      setItems((prev) =>
        prev.map((item) => {
          const r = priceMap.get(item.id);
          if (r) {
            return {
              ...item,
              currentLowestPrice: r.lowestPrice,
              currentLowestStore: r.lowestPriceStore,
              lastDealCheck: r.lastChecked,
            };
          }
          return item;
        }),
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to check deals');
    } finally {
      setCheckingAll(false);
    }
  };

  const handleToggleNotify = async (item: WishlistResource) => {
    try {
      const updated = await updateWishlistItem(item.id, {
        ...item,
        notifyOnAnyDeal: !item.notifyOnAnyDeal,
      });
      setItems((prev) => prev.map((i) => (i.id === item.id ? { ...i, notifyOnAnyDeal: updated.notifyOnAnyDeal } : i)));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update');
    }
  };

  const handleThresholdBlur = async (item: WishlistResource) => {
    const raw = editingThreshold[item.id];
    if (raw === undefined) return;

    const parsed = raw === '' ? null : parseFloat(raw);
    if (parsed !== null && isNaN(parsed)) return;

    try {
      await updateWishlistItem(item.id, {
        ...item,
        priceThreshold: parsed,
      });
      setItems((prev) =>
        prev.map((i) => (i.id === item.id ? { ...i, priceThreshold: parsed } : i)),
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update threshold');
    }

    setEditingThreshold((prev) => {
      const next = { ...prev };
      delete next[item.id];
      return next;
    });
  };

  const handleExpandRow = async (id: number) => {
    if (expandedId === id) {
      setExpandedId(null);
      return;
    }
    setExpandedId(id);
    setExpandedDeals([]);
    setLoadingDeals(true);
    try {
      const data = await getDeals(id);
      setExpandedDeals(data.deals);
    } catch {
      setExpandedDeals([]);
    } finally {
      setLoadingDeals(false);
    }
  };

  if (loading) {
    return (
      <div className="page-loading">
        <div className="spinner" />
        <p>Loading wishlist...</p>
      </div>
    );
  }

  return (
    <div className="wl">
      <div className="wl-header">
        <div className="wl-header-left">
          <h1>Wishlist</h1>
          <span className="wl-count">
            {items.length} {items.length === 1 ? 'game' : 'games'}
          </span>
        </div>
        <div className="wl-header-actions">
          {items.length > 0 && (
            <button
              className="wl-btn-check-all"
              onClick={handleCheckAll}
              disabled={checkingAll}
            >
              <RefreshIcon />
              {checkingAll ? 'Checking...' : 'Check All Deals'}
            </button>
          )}
        </div>
      </div>

      <div className="wl-search-section" ref={searchRef}>
        <div className="wl-search-bar">
          <span className="wl-search-icon"><SearchIcon /></span>
          <input
            type="text"
            placeholder="Search IGDB to add a game to your wishlist..."
            value={searchTerm}
            onChange={(e) => handleSearchInput(e.target.value)}
          />
          {searching && <div className="wl-search-spinner" />}
          {searchResults.length > 0 && (
            <div className="wl-search-results">
              {searchResults.map((game) => {
                const cover = game.images?.find((i) => i.coverType === 'Cover');
                const coverUrl = cover?.remoteUrl || cover?.url;
                const alreadyAdded = addedIgdbIds.has(game.igdbId);
                return (
                  <div
                    key={game.igdbId}
                    className="wl-search-result"
                    onClick={() => !alreadyAdded && handleAddToWishlist(game)}
                    style={{ opacity: alreadyAdded ? 0.5 : 1, cursor: alreadyAdded ? 'default' : 'pointer' }}
                  >
                    {coverUrl ? (
                      <img className="wl-search-result-thumb" src={coverUrl} alt="" />
                    ) : (
                      <div className="wl-search-result-thumb" style={{ background: 'var(--bg-tertiary)' }} />
                    )}
                    <div className="wl-search-result-info">
                      <div className="wl-search-result-title">{game.title}</div>
                      <div className="wl-search-result-meta">
                        {game.year > 0 && game.year}
                        {game.developer && ` \u00B7 ${game.developer}`}
                      </div>
                    </div>
                    {alreadyAdded && <span className="wl-search-result-added">On wishlist</span>}
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>

      {error && <div className="error-banner">{error}</div>}

      {items.length === 0 ? (
        <div className="empty-state">
          <div className="empty-icon" style={{ fontSize: '3.5rem' }}>&#128176;</div>
          <h2>Your wishlist is empty</h2>
          <p>Search for games above to start tracking deals.</p>
          <p className="subtitle">
            Connect your ITAD API key in Settings for cross-store price tracking.
          </p>
        </div>
      ) : (
        <table className="wl-table">
          <thead>
            <tr>
              <th style={{ width: '40%' }}>Game</th>
              <th>Price</th>
              <th>Threshold</th>
              <th style={{ textAlign: 'center' }}>Notify</th>
              <th>Checked</th>
              <th style={{ textAlign: 'right' }}></th>
            </tr>
          </thead>
          <tbody>
            {items.map((item, idx) => {
              const cover = item.images?.find((i) => i.coverType === 'Cover');
              const coverUrl = cover?.remoteUrl || cover?.url;
              const hasPrice = item.currentLowestPrice !== null && item.currentLowestPrice !== undefined;
              const isFree = hasPrice && item.currentLowestPrice === 0;
              const isExpanded = expandedId === item.id;
              const thresholdValue =
                editingThreshold[item.id] !== undefined
                  ? editingThreshold[item.id]
                  : item.priceThreshold !== null && item.priceThreshold !== undefined
                    ? item.priceThreshold.toString()
                    : '';

              return (
                <>
                  <tr
                    key={item.id}
                    className="wl-row wl-row-clickable"
                    style={{ animationDelay: `${idx * 30}ms` }}
                    onClick={() => handleExpandRow(item.id)}
                  >
                    <td>
                      <div className="wl-game-cell">
                        <ChevronIcon open={isExpanded} />
                        {coverUrl ? (
                          <img className="wl-game-thumb" src={coverUrl} alt="" loading="lazy" />
                        ) : (
                          <div className="wl-game-thumb-empty">&#127918;</div>
                        )}
                        <div className="wl-game-info">
                          <div className="wl-game-title" title={item.title}>{item.title}</div>
                          <div className="wl-game-sub">
                            {item.year > 0 && item.year}
                            {item.developer && ` \u00B7 ${item.developer}`}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td className="wl-price-cell" onClick={(e) => e.stopPropagation()}>
                      {hasPrice ? (
                        <div>
                          <span className={`wl-price-current ${isFree ? 'is-free' : 'has-deal'}`}>
                            {formatPrice(item.currentLowestPrice)}
                          </span>
                          {isFree && <span className="wl-discount-badge free-badge">FREE</span>}
                          {item.currentLowestStore && (
                            <div className="wl-price-store">{item.currentLowestStore}</div>
                          )}
                        </div>
                      ) : (
                        <span className="wl-price-current no-data">&mdash;</span>
                      )}
                    </td>
                    <td onClick={(e) => e.stopPropagation()}>
                      <div className="wl-threshold-input">
                        <span className="wl-threshold-prefix">$</span>
                        <input
                          className="wl-threshold-field"
                          type="number"
                          step="0.01"
                          min="0"
                          placeholder="any"
                          value={thresholdValue}
                          onChange={(e) =>
                            setEditingThreshold((prev) => ({ ...prev, [item.id]: e.target.value }))
                          }
                          onBlur={() => handleThresholdBlur(item)}
                          onKeyDown={(e) => {
                            if (e.key === 'Enter') (e.target as HTMLInputElement).blur();
                          }}
                        />
                      </div>
                    </td>
                    <td style={{ textAlign: 'center' }} onClick={(e) => e.stopPropagation()}>
                      <label className="wl-toggle">
                        <input
                          type="checkbox"
                          checked={item.notifyOnAnyDeal}
                          onChange={() => handleToggleNotify(item)}
                        />
                        <span className="wl-toggle-track" />
                        <span className="wl-toggle-knob" />
                      </label>
                    </td>
                    <td onClick={(e) => e.stopPropagation()}>
                      <span className="wl-time">{timeAgo(item.lastDealCheck)}</span>
                    </td>
                    <td onClick={(e) => e.stopPropagation()}>
                      <div className="wl-actions">
                        <button
                          className={`wl-btn-icon check-btn ${checkingId === item.id ? 'checking' : ''}`}
                          onClick={() => handleCheckDeals(item.id)}
                          disabled={checkingId === item.id}
                          title="Check deals"
                        >
                          <RefreshIcon />
                        </button>
                        <button
                          className="wl-btn-icon remove-btn"
                          onClick={() => handleRemove(item.id)}
                          title="Remove from wishlist"
                        >
                          <TrashIcon />
                        </button>
                      </div>
                    </td>
                  </tr>
                  {isExpanded && (
                    <tr key={`deals-${item.id}`} className="wl-deal-row">
                      <td colSpan={6}>
                        <div className="wl-deal-panel">
                          {loadingDeals ? (
                            <div className="wl-deal-none">Loading deals...</div>
                          ) : expandedDeals.length === 0 ? (
                            <div className="wl-deal-none">
                              No deal data yet. Click the refresh icon to check prices.
                            </div>
                          ) : (
                            <div className="wl-deal-grid">
                              {expandedDeals.map((deal, i) => (
                                <div key={i} className="wl-deal-card">
                                  {deal.dealUrl ? (
                                    <a href={deal.dealUrl} target="_blank" rel="noopener noreferrer">
                                      <span className="wl-deal-store">{deal.store}</span>
                                      <div className="wl-deal-pricing">
                                        <span className="wl-deal-current">
                                          {deal.isFree ? 'FREE' : `$${deal.currentPrice.toFixed(2)}`}
                                        </span>
                                        {deal.discountPercent > 0 && (
                                          <>
                                            <span className="wl-deal-original">
                                              ${deal.regularPrice.toFixed(2)}
                                            </span>
                                            <span className="wl-deal-pct">
                                              -{deal.discountPercent}%
                                            </span>
                                          </>
                                        )}
                                      </div>
                                    </a>
                                  ) : (
                                    <>
                                      <span className="wl-deal-store">{deal.store}</span>
                                      <div className="wl-deal-pricing">
                                        <span className="wl-deal-current">
                                          {deal.isFree ? 'FREE' : `$${deal.currentPrice.toFixed(2)}`}
                                        </span>
                                        {deal.discountPercent > 0 && (
                                          <>
                                            <span className="wl-deal-original">
                                              ${deal.regularPrice.toFixed(2)}
                                            </span>
                                            <span className="wl-deal-pct">
                                              -{deal.discountPercent}%
                                            </span>
                                          </>
                                        )}
                                      </div>
                                    </>
                                  )}
                                </div>
                              ))}
                            </div>
                          )}
                        </div>
                      </td>
                    </tr>
                  )}
                </>
              );
            })}
          </tbody>
        </table>
      )}
    </div>
  );
}

export default Wishlist;
