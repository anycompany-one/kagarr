import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import {
  testIgdb,
  testDiscord,
  testIndexer,
  testDownloadClient,
  setApiKey,
  TestResult,
} from '../api';
import { supportedLanguages } from '../i18n';
import './GeneralSettings.css';

interface ServiceStatus {
  state: 'idle' | 'testing' | 'success' | 'failed';
  message?: string;
}

interface ConfiguredService {
  id: number;
  name: string;
  implementation: string;
}

function GeneralSettings() {
  const { t, i18n } = useTranslation();
  const [apiKeyValue, setApiKeyValue] = useState(
    () => localStorage.getItem('kagarr_api_key') || '',
  );
  const [saved, setSaved] = useState(false);

  // Service statuses
  const [igdbStatus, setIgdbStatus] = useState<ServiceStatus>({ state: 'idle' });
  const [discordStatus, setDiscordStatus] = useState<ServiceStatus>({ state: 'idle' });
  const [indexerStatuses, setIndexerStatuses] = useState<Record<number, ServiceStatus>>({});
  const [clientStatuses, setClientStatuses] = useState<Record<number, ServiceStatus>>({});

  // Configured services from API
  const [indexers, setIndexers] = useState<ConfiguredService[]>([]);
  const [clients, setClients] = useState<ConfiguredService[]>([]);
  const [loadingServices, setLoadingServices] = useState(true);

  const fetchServices = useCallback(async () => {
    const headers: Record<string, string> = {};
    const key = localStorage.getItem('kagarr_api_key');
    if (key) headers['X-Api-Key'] = key;

    try {
      const [idxRes, clRes] = await Promise.all([
        fetch('/api/v1/indexer', { headers }),
        fetch('/api/v1/downloadclient', { headers }),
      ]);
      if (idxRes.ok) setIndexers(await idxRes.json());
      if (clRes.ok) setClients(await clRes.json());
    } catch {
      // Services may not be configured yet
    } finally {
      setLoadingServices(false);
    }
  }, []);

  useEffect(() => {
    fetchServices();
  }, [fetchServices]);

  const handleSaveKey = () => {
    if (apiKeyValue.trim()) {
      setApiKey(apiKeyValue.trim());
    } else {
      localStorage.removeItem('kagarr_api_key');
    }
    setSaved(true);
    setTimeout(() => setSaved(false), 2500);
  };

  const handleLanguageChange = (code: string) => {
    i18n.changeLanguage(code);
  };

  const runTest = async (
    testFn: () => Promise<TestResult>,
    setSt: (s: ServiceStatus) => void,
  ) => {
    setSt({ state: 'testing' });
    try {
      const result = await testFn();
      setSt({
        state: result.isValid ? 'success' : 'failed',
        message: result.message,
      });
    } catch (err) {
      setSt({
        state: 'failed',
        message: err instanceof Error ? err.message : 'Connection failed',
      });
    }
  };

  const handleTestIgdb = () => runTest(testIgdb, setIgdbStatus);
  const handleTestDiscord = () => runTest(testDiscord, setDiscordStatus);

  const handleTestIndexer = (id: number) => {
    const setSt = (s: ServiceStatus) =>
      setIndexerStatuses(prev => ({ ...prev, [id]: s }));
    runTest(() => testIndexer(id), setSt);
  };

  const handleTestClient = (id: number) => {
    const setSt = (s: ServiceStatus) =>
      setClientStatuses(prev => ({ ...prev, [id]: s }));
    runTest(() => testDownloadClient(id), setSt);
  };

  const anyTesting =
    igdbStatus.state === 'testing' ||
    discordStatus.state === 'testing' ||
    Object.values(indexerStatuses).some(s => s.state === 'testing') ||
    Object.values(clientStatuses).some(s => s.state === 'testing');

  const handleTestAll = async () => {
    handleTestIgdb();
    handleTestDiscord();
    indexers.forEach(idx => handleTestIndexer(idx.id));
    clients.forEach(cl => handleTestClient(cl.id));
  };

  return (
    <div className="general-settings">
      {/* Language Section */}
      <div>
        <div className="gs-section-label">
          <h2>{t('settings.language')}</h2>
        </div>
        <div className="gs-apikey-panel">
          <select
            className="gs-apikey-input"
            value={i18n.language}
            onChange={e => handleLanguageChange(e.target.value)}
          >
            {supportedLanguages.map(lang => (
              <option key={lang.code} value={lang.code}>
                {lang.name}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* API Key Section */}
      <div>
        <div className="gs-section-label">
          <h2>{t('settings.authentication')}</h2>
        </div>
        <div className="gs-apikey-panel">
          <div className="gs-apikey-row">
            <input
              type="password"
              className="gs-apikey-input"
              value={apiKeyValue}
              onChange={e => setApiKeyValue(e.target.value)}
              placeholder={t('settings.apiKeyPlaceholder')}
              spellCheck={false}
              autoComplete="off"
            />
            <button className="btn btn-primary btn-sm" onClick={handleSaveKey}>
              {t('settings.save')}
            </button>
            {saved && (
              <span className="gs-apikey-saved">
                {t('settings.saved')}
              </span>
            )}
          </div>
          <p className="gs-apikey-hint">
            {t('settings.apiKeyHint')}{' '}
            {t('settings.apiKeyEnvHint')}
          </p>
        </div>
      </div>

      {/* Health Checks Section */}
      <div>
        <div className="gs-section-label">
          <h2>{t('settings.connectionHealth')}</h2>
          {!loadingServices && (
            <button
              className="gs-test-all-btn"
              onClick={handleTestAll}
              disabled={anyTesting}
            >
              {t('settings.testAll')}
            </button>
          )}
        </div>

        <div className="gs-health-grid">
          {/* Core services */}
          <ServiceCard
            name="IGDB"
            type="Metadata Provider"
            status={igdbStatus}
            onTest={handleTestIgdb}
          />
          <ServiceCard
            name="Discord"
            type="Notifications"
            status={discordStatus}
            onTest={handleTestDiscord}
          />

          {/* Indexers */}
          {indexers.length > 0 && (
            <div className="gs-subgroup">
              <div className="gs-subgroup-label">{t('settings.indexers')}</div>
              <div className="gs-health-grid">
                {indexers.map(idx => (
                  <ServiceCard
                    key={`idx-${idx.id}`}
                    name={idx.name}
                    type={idx.implementation}
                    status={indexerStatuses[idx.id] || { state: 'idle' }}
                    onTest={() => handleTestIndexer(idx.id)}
                  />
                ))}
              </div>
            </div>
          )}
          {!loadingServices && indexers.length === 0 && (
            <div className="gs-empty-note">
              {t('settings.noIndexers')}
            </div>
          )}

          {/* Download Clients */}
          {clients.length > 0 && (
            <div className="gs-subgroup">
              <div className="gs-subgroup-label">{t('settings.downloadClients')}</div>
              <div className="gs-health-grid">
                {clients.map(cl => (
                  <ServiceCard
                    key={`cl-${cl.id}`}
                    name={cl.name}
                    type={cl.implementation}
                    status={clientStatuses[cl.id] || { state: 'idle' }}
                    onTest={() => handleTestClient(cl.id)}
                  />
                ))}
              </div>
            </div>
          )}
          {!loadingServices && clients.length === 0 && (
            <div className="gs-empty-note">
              {t('settings.noClients')}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

function ServiceCard({
  name,
  type,
  status,
  onTest,
}: {
  name: string;
  type: string;
  status: ServiceStatus;
  onTest: () => void;
}) {
  const { t } = useTranslation();
  const stateClass = status.state !== 'idle' ? `gs-service--${status.state}` : '';

  return (
    <div className={`gs-service ${stateClass}`}>
      <div className={`gs-status-dot gs-status-dot--${status.state}`} />
      <div className="gs-service-info">
        <div className="gs-service-name">{name}</div>
        <div className="gs-service-type">{type}</div>
        {status.message && status.state !== 'testing' && (
          <div
            className={`gs-service-result gs-service-result--${status.state}`}
          >
            {status.message}
          </div>
        )}
      </div>
      <button
        className="gs-test-btn"
        onClick={onTest}
        disabled={status.state === 'testing'}
      >
        {status.state === 'testing' ? t('settings.testing') : t('settings.test')}
      </button>
    </div>
  );
}

export default GeneralSettings;
