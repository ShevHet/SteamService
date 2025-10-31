import type { Game } from './Game';

export interface CalendarDay {
  date: string; // ISO date string
  count: number;
  games?: Game[]; // Optional for backward compatibility
}

export interface CalendarMonth {
  month: string;
  days: CalendarDay[];
}