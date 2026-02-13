import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { GameResource } from '../types';

interface GameCardProps {
  game: GameResource;
  onDelete?: (id: number) => void;
  onAdd?: (game: GameResource) => void;
  isSearchResult?: boolean;
}

function GameCard({ game, onDelete, onAdd, isSearchResult }: GameCardProps) {
  const { t } = useTranslation();
  const coverImage = game.images?.find((i) => i.coverType === 'Cover');
  const coverUrl = coverImage?.remoteUrl || coverImage?.url;

  const cardContent = (
    <>
      <div className="game-card-poster">
        {coverUrl ? (
          <img src={coverUrl} alt={game.title} loading="lazy" />
        ) : (
          <div className="game-card-placeholder">
            <span>&#127918;</span>
          </div>
        )}
        <div className="game-card-overlay">
          {isSearchResult && onAdd && (
            <button
              className="btn btn-sm btn-primary"
              onClick={(e) => {
                e.preventDefault();
                e.stopPropagation();
                onAdd(game);
              }}
            >
              {t('addGame.add')}
            </button>
          )}
          {!isSearchResult && onDelete && game.id > 0 && (
            <button
              className="btn btn-sm btn-danger"
              onClick={(e) => {
                e.preventDefault();
                e.stopPropagation();
                onDelete(game.id);
              }}
            >
              {t('addGame.remove')}
            </button>
          )}
        </div>
      </div>
      <div className="game-card-info">
        <h3 className="game-card-title" title={game.title}>
          {game.title}
        </h3>
        <div className="game-card-meta">
          {game.year > 0 && <span>{game.year}</span>}
          {game.platform && game.platform !== 'Unknown' && (
            <span className="badge">{game.platform.replace('_', ' ')}</span>
          )}
        </div>
        {game.developer && (
          <div className="game-card-developer">{game.developer}</div>
        )}
      </div>
    </>
  );

  // Search results aren't in the library yet — no detail page to link to
  if (isSearchResult) {
    return <div className="game-card">{cardContent}</div>;
  }

  return (
    <Link to={`/game/${game.id}`} className="game-card" style={{ textDecoration: 'none', color: 'inherit' }}>
      {cardContent}
    </Link>
  );
}

export default GameCard;
