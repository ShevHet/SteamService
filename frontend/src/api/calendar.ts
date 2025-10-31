import api from '../services/api';
import type { CalendarMonth } from '@/src/types/calendar';
import type { Game } from '@/src/types/Game';

export const calendarApi = {
  /**
   * Получить календарь релизов за месяц
   * @param month Формат "YYYY-MM" (например, "2025-11")
   */
  async getCalendar(month: string, genre?: string): Promise<CalendarMonth> {
    const params: Record<string, string> = { month };
    if (genre && genre.trim()) {
      params.genre = genre.trim();
    }
    const response = await api.get<CalendarMonth>(`/api/v1/games/calendar`, { params });
    return response.data;
  },

  /**
   * Получить игры за конкретный день
   * @param month Формат "YYYY-MM" (например, "2025-11")
   * @param day День месяца
   */
  async getGamesByDate(month: string, day: number): Promise<Game[]> {
    const year = parseInt(month.split('-')[0]);
    const monthNum = parseInt(month.split('-')[1]);
    
    const response = await api.get<Game[]>(`/api/v1/games`, {
      params: {
        month: `${year}-${String(monthNum).padStart(2, '0')}`,
        date: `${year}-${String(monthNum).padStart(2, '0')}-${String(day).padStart(2, '0')}`
      }
    });
    return response.data;
  },

  /**
   * Получить список игр за месяц
   * @param month Формат "YYYY-MM" (например, "2025-11")
   */
  async getGamesByMonth(month: string, genre?: string): Promise<Game[]> {
    const params: Record<string, string> = { month };
    if (genre && genre.trim()) {
      params.genre = genre.trim();
    }
    const response = await api.get<Game[]>(`/api/v1/games`, { params });
    return response.data;
  },
};
