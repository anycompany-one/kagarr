import { NavLink, Routes, Route, Navigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import GeneralSettings from './GeneralSettings';
import IndexerSettings from './IndexerSettings';
import DownloadClientSettings from './DownloadClientSettings';
import RemotePathMappingSettings from './RemotePathMappingSettings';

function Settings() {
  const { t } = useTranslation();

  return (
    <div className="settings">
      <div className="settings-header">
        <h1>{t('settings.title')}</h1>
      </div>
      <div className="settings-layout">
        <nav className="settings-sidebar">
          <NavLink
            to="/settings/general"
            className={({ isActive }) => `settings-nav-item ${isActive ? 'active' : ''}`}
          >
            {t('settings.general')}
          </NavLink>
          <NavLink
            to="/settings/indexers"
            className={({ isActive }) => `settings-nav-item ${isActive ? 'active' : ''}`}
          >
            {t('settings.indexers')}
          </NavLink>
          <NavLink
            to="/settings/downloadclients"
            className={({ isActive }) => `settings-nav-item ${isActive ? 'active' : ''}`}
          >
            {t('settings.downloadClients')}
          </NavLink>
          <NavLink
            to="/settings/remotepath"
            className={({ isActive }) => `settings-nav-item ${isActive ? 'active' : ''}`}
          >
            {t('settings.remotePathMappings')}
          </NavLink>
        </nav>
        <div className="settings-content">
          <Routes>
            <Route path="general" element={<GeneralSettings />} />
            <Route path="indexers" element={<IndexerSettings />} />
            <Route path="downloadclients" element={<DownloadClientSettings />} />
            <Route path="remotepath" element={<RemotePathMappingSettings />} />
            <Route path="*" element={<Navigate to="general" replace />} />
          </Routes>
        </div>
      </div>
    </div>
  );
}

export default Settings;
