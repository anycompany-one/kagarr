import { NavLink, Routes, Route, Navigate } from 'react-router-dom';
import GeneralSettings from './GeneralSettings';
import IndexerSettings from './IndexerSettings';
import DownloadClientSettings from './DownloadClientSettings';

function Settings() {
  return (
    <div className="settings">
      <div className="settings-header">
        <h1>Settings</h1>
      </div>
      <div className="settings-layout">
        <nav className="settings-sidebar">
          <NavLink
            to="/settings/general"
            className={({ isActive }) => `settings-nav-item ${isActive ? 'active' : ''}`}
          >
            General
          </NavLink>
          <NavLink
            to="/settings/indexers"
            className={({ isActive }) => `settings-nav-item ${isActive ? 'active' : ''}`}
          >
            Indexers
          </NavLink>
          <NavLink
            to="/settings/downloadclients"
            className={({ isActive }) => `settings-nav-item ${isActive ? 'active' : ''}`}
          >
            Download Clients
          </NavLink>
        </nav>
        <div className="settings-content">
          <Routes>
            <Route path="general" element={<GeneralSettings />} />
            <Route path="indexers" element={<IndexerSettings />} />
            <Route path="downloadclients" element={<DownloadClientSettings />} />
            <Route path="*" element={<Navigate to="general" replace />} />
          </Routes>
        </div>
      </div>
    </div>
  );
}

export default Settings;
