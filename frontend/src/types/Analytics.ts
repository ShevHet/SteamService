export interface GenreTrendPoint {
  month: string;
  avgFollowers: number | null;
  rank: number | null;
}

export interface GenreTrend {
  genre: string;
  data: GenreTrendPoint[];
}