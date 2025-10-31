import { QueryClient } from '@tanstack/react-query';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000,        // 5 минут
      gcTime: 10 * 60 * 1000,       // 10 минут
      refetchOnWindowFocus: false,
      retry: 2,
      refetchOnMount: 'always'
    },
    mutations: {
      retry: 1,
    },
  },
});

