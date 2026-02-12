import { NavLink } from 'react-router-dom';

function Navbar() {
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
          Library
        </NavLink>
        <NavLink
          to="/add"
          className={({ isActive }) => `navbar-link ${isActive ? 'active' : ''}`}
        >
          Add Game
        </NavLink>
        <NavLink
          to="/wishlist"
          className={({ isActive }) => `navbar-link ${isActive ? 'active' : ''}`}
        >
          Wishlist
        </NavLink>
        <NavLink
          to="/activity"
          className={({ isActive }) => `navbar-link ${isActive ? 'active' : ''}`}
        >
          Activity
        </NavLink>
        <NavLink
          to="/history"
          className={({ isActive }) => `navbar-link ${isActive ? 'active' : ''}`}
        >
          History
        </NavLink>
        <NavLink
          to="/settings"
          className={({ isActive }) => `navbar-link ${isActive ? 'active' : ''}`}
        >
          Settings
        </NavLink>
      </div>
    </nav>
  );
}

export default Navbar;
