import { useQuery } from '@tanstack/react-query';
import { calendarApi } from '@/src/api/calendar';
import type { CalendarMonth } from '@/src/types/calendar';
import type { Game } from '@/src/types/Game';

export const useCalendar = (month: string, genre?: string | null) => {
  const normalizedGenre = genre ?? null;
  return useQuery<CalendarMonth>({
    queryKey: ['calendar', month, normalizedGenre],
    queryFn: () => calendarApi.getCalendar(month, normalizedGenre ?? undefined),
    retry: 1,
    retryDelay: 1000,
  });
};

export const useGamesByMonth = (month: string, genre?: string | null) => {
  const normalizedGenre = genre ?? null;
  return useQuery<Game[]>({
    queryKey: ['games', month, normalizedGenre],
    queryFn: () => calendarApi.getGamesByMonth(month, normalizedGenre ?? undefined),
    retry: 1,
    retryDelay: 1000,
  });
};
