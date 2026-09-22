export interface LanguageOption {
  code: string;
  label: string;
}

export const SUPPORTED_LANGUAGES: LanguageOption[] = [
  { code: 'en', label: 'English' },
  { code: 'sr', label: 'Srpski' },
  { code: 'de', label: 'Deutsch' },
  { code: 'fr', label: 'Français' },
];

export const LANGUAGE_STORAGE_KEY = 'language';

export function getInitialLanguage(): string {
  try {
    const stored = localStorage.getItem(LANGUAGE_STORAGE_KEY);
    if (stored && SUPPORTED_LANGUAGES.some((option) => option.code === stored)) {
      return stored;
    }
  } catch {
    return 'en';
  }
  return 'en';
}
