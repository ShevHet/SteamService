import { useMutation, useQueryClient } from '@tanstack/react-query';
import api from '@/src/lib/api-client';

interface SyncResponse {
  status: string;
  timestamp: string;
}

export const useSyncSteam = () => {
  const queryClient = useQueryClient();
  
  return useMutation<SyncResponse, Error>({
    mutationFn: async () => {
      const response = await api.post('/steam/sync');
      return response.data;
    },
    onSuccess: () => {
      // Инвалидируем кэш после синхронизации
      queryClient.invalidateQueries({ queryKey: ['games'] });
      queryClient.invalidateQueries({ queryKey: ['calendar'] });
      queryClient.invalidateQueries({ queryKey: ['analytics'] });
    },
  });
};

