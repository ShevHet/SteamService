export interface Game {
  id: string;
  steamAppId?: number;
  name: string;
  slug?: string;
  shortDescription?: string;
  fullDescription?: string;
  releaseDate?: string;
  price?: number;
  metacriticScore?: number;
  genres: string[];
  platforms: string[];
  storeUrl?: string;
  headerImageUrl?: string;
  isComingSoon: boolean;
  createdAt: string;
}

export interface PaginatedGames {
  page: number;
  pageSize: number;
  total: number;
  items: Game[];
}

export interface ImportResult {
  added: number;
  updated: number;
  failed: number;
  errors: string[];
}
