import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

import en from './locales/en.json';
import ptBR from './locales/pt-BR.json';
import de from './locales/de.json';
import fr from './locales/fr.json';
import ja from './locales/ja.json';
import ko from './locales/ko.json';
import zhCN from './locales/zh-CN.json';
import ru from './locales/ru.json';
import pl from './locales/pl.json';

export const supportedLanguages = [
  { code: 'en', name: 'English' },
  { code: 'pt-BR', name: 'Portugu\u00eas (Brasil)' },
  { code: 'de', name: 'Deutsch' },
  { code: 'fr', name: 'Fran\u00e7ais' },
  { code: 'ja', name: '\u65E5\u672C\u8A9E' },
  { code: 'ko', name: '\uD55C\uAD6D\uC5B4' },
  { code: 'zh-CN', name: '\u4E2D\u6587 (\u7B80\u4F53)' },
  { code: 'ru', name: '\u0420\u0443\u0441\u0441\u043A\u0438\u0439' },
  { code: 'pl', name: 'Polski' },
] as const;

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: {
      en: { translation: en },
      'pt-BR': { translation: ptBR },
      de: { translation: de },
      fr: { translation: fr },
      ja: { translation: ja },
      ko: { translation: ko },
      'zh-CN': { translation: zhCN },
      ru: { translation: ru },
      pl: { translation: pl },
    },
    fallbackLng: 'en',
    interpolation: {
      escapeValue: false,
    },
    detection: {
      order: ['localStorage', 'navigator'],
      caches: ['localStorage'],
    },
  });

export default i18n;
