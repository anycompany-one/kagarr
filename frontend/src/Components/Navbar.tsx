import { NavLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

function Navbar() {
  const { t } = useTranslation();

  return (
    <nav className="navbar">
      <div className="navbar-brand">
        <NavLink to="/" className="navbar-logo">
          Kagarr
        </NavLink>
      </div>
      <div className="navbar-links">
        <NavLink
          to="/"
          end
          className={({ isActive }) => `navbar-link ${isActive ? 'active' : ''}`}
        >
          {t('nav.library')}
        </NavLink>
        <NavLink
          to="/add"
          className={({ isActive }) => `navbar-link ${isActive ? 'active' : ''}`}
        >
          {t('nav.addGame')}
        </NavLink>
        <NavLink
          to="/wishlist"
          className={({ isActive }) => `navbar-link ${isActive ? 'active' : ''}`}
        >
          {t('nav.wishlist')}
        </NavLink>
        <NavLink
          to="/activity"
          className={({ isActive }) => `navbar-link ${isActive ? 'active' : ''}`}
        >
          {t('nav.activity')}
        </NavLink>
        <NavLink
          to="/history"
          className={({ isActive }) => `navbar-link ${isActive ? 'active' : ''}`}
        >
          {t('nav.history')}
        </NavLink>
        <NavLink
          to="/settings"
          className={({ isActive }) => `navbar-link ${isActive ? 'active' : ''}`}
        >
          {t('nav.settings')}
        </NavLink>
      </div>
    </nav>
  );
}

export default Navbar;
