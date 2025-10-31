import { useQuery } from '@tanstack/react-query';
import { GenreTrend } from '@/src/types/Analytics';
import api from '@/src/lib/api-client';

export const useAnalytics = (months: number = 3, includeCurrentMonth: boolean = true) => {
  return useQuery<GenreTrend[]>({
    queryKey: ['analytics', 'trends', months, includeCurrentMonth],
    queryFn: async () => {
      const response = await api.get('/v1/analytics/trends', {
        params: { months, includeCurrentMonth }
      });
      return response.data;
    },
  });
};

