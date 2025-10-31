import { useQuery } from '@tanstack/react-query';
import { Game, PaginatedGames } from '@/src/types/Game';
import api from '@/src/lib/api-client';

interface UseGamesParams {
  month: string;      // Required: "2025-11"
  platform?: string;  // Optional
  genre?: string;     // Optional
  page?: number;
  pageSize?: number;
}

export const useGames = (params: UseGamesParams) => {
  return useQuery<Game[] | PaginatedGames>({
    queryKey: ['games', params],
    queryFn: async () => {
      const response = await api.get('/v1/games', { params });
      return response.data;
    },
    enabled: !!params.month,
  });
};
