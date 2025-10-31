'use client';

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type PropsWithChildren,
} from 'react';

interface FiltersContextValue {
  selectedGenre: string | null;
  setGenre: (genre: string | null) => void;
  toggleGenre: (genre: string) => void;
  clear: () => void;
  isGenreSelected: (genre: string) => boolean;
}

const FiltersContext = createContext<FiltersContextValue | undefined>(undefined);

export const FiltersProvider = ({ children }: PropsWithChildren) => {
  // Инициализируем как null для избежания ошибок гидратации
  // Значение из localStorage загрузим в useEffect
  const [selectedGenre, setSelectedGenre] = useState<string | null>(null);
  
  // Загружаем из localStorage только на клиенте после монтирования
  useEffect(() => {
    if (typeof window !== 'undefined') {
      const saved = localStorage.getItem('selectedGenre');
      if (saved) {
        setSelectedGenre(saved);
      }
    }
  }, []);

  // Сохраняем в localStorage при изменении
  useEffect(() => {
    if (typeof window !== 'undefined') {
      if (selectedGenre) {
        localStorage.setItem('selectedGenre', selectedGenre);
      } else {
        localStorage.removeItem('selectedGenre');
      }
    }
  }, [selectedGenre]);

  const setGenre = useCallback((genre: string | null) => {
    setSelectedGenre(genre ?? null);
  }, []);

  const toggleGenre = useCallback((genre: string) => {
    setSelectedGenre((current) => {
      const newValue = current === genre ? null : genre;
      // Сохраняем синхронно в localStorage
      if (typeof window !== 'undefined') {
        if (newValue) {
          localStorage.setItem('selectedGenre', newValue);
        } else {
          localStorage.removeItem('selectedGenre');
        }
      }
      return newValue;
    });
  }, []);

  const clear = useCallback(() => {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('selectedGenre');
    }
    setSelectedGenre(null);
  }, []);

  const isGenreSelected = useCallback(
    (genre: string) => selectedGenre === genre,
    [selectedGenre]
  );

  const value = useMemo(
    () => ({ selectedGenre, setGenre, toggleGenre, clear, isGenreSelected }),
    [selectedGenre, setGenre, toggleGenre, clear, isGenreSelected]
  );

  return <FiltersContext.Provider value={value}>{children}</FiltersContext.Provider>;
};

export const useFilters = (): FiltersContextValue => {
  const context = useContext(FiltersContext);

  if (!context) {
    throw new Error('useFilters должен использоваться внутри FiltersProvider');
  }

  return context;
};
