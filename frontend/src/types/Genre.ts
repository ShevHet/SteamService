/**
 * Типы для работы с жанрами игр
 */

export interface Genre {
  name: string;
  slug?: string;
}

export interface GenreStat {
  genre: string;
  count: number;
  percentage?: number;
}

