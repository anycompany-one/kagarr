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
      </div>
    </nav>
  );
}

export default Navbar;
