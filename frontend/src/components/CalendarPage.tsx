'use client';

import React, { useState, useMemo, useEffect } from 'react';
import { useCalendar, useGamesByMonth } from '@/src/hooks/useCalendar';
import { useFilters } from '@/src/hooks/useFilters';
import { Card, CardContent } from './ui/Card';
import { Dialog, DialogContent } from './ui/Dialog';
import type { Game } from '@/src/types/Game';

interface CalendarPageProps {
  month?: string; // Формат "YYYY-MM"
}

export const CalendarPage: React.FC<CalendarPageProps> = ({ month = '2025-11' }) => {
  const { selectedGenre, clear: clearFilters } = useFilters();
  const { data: calendarData, isLoading } = useCalendar(month, selectedGenre);
  const { data: gamesData } = useGamesByMonth(month, selectedGenre);

  // Используем данные от API (уже отфильтрованные на сервере, если selectedGenre есть)
  // Добавляем клиентскую фильтрацию как дополнительную проверку
  const filteredGames = useMemo(() => {
    if (!gamesData) {
      return [] as Game[];
    }
    
    // Если фильтр установлен, API уже должен был отфильтровать
    // Но добавляем клиентскую проверку на случай рассинхронизации
    if (!selectedGenre) {
      return gamesData;
    }

    const lower = selectedGenre.toLowerCase();
    return gamesData.filter((game) =>
      game.genres && game.genres.length > 0 &&
      game.genres.some((genre) => genre.toLowerCase() === lower)
    );
  }, [gamesData, selectedGenre]);
  
  const [selectedDay, setSelectedDay] = useState<{ date: Date; games: Game[] } | null>(null);

  // Получаем год и месяц
  const [year, monthNum] = month.split('-').map(Number);

  // Создаем карту дат с играми
  const gamesByDate = useMemo(() => {
    const map = new Map<string, Game[]>();
    filteredGames.forEach((game) => {
      if (game.releaseDate) {
        const date = new Date(game.releaseDate);
        const dateKey = date.toISOString().split('T')[0];
        if (!map.has(dateKey)) {
          map.set(dateKey, []);
        }
        map.get(dateKey)!.push(game);
      }
    });
    return map;
  }, [filteredGames]);

  // Создаем массив дней для календаря
  const calendarDays = useMemo(() => {
    const firstDay = new Date(year, monthNum - 1, 1);
    const lastDay = new Date(year, monthNum, 0);
    const daysInMonth = lastDay.getDate();
    const startDayOfWeek = firstDay.getDay(); // 0 = воскресенье, 1 = понедельник

    const days: Array<{ date: Date; count: number; games: Game[] }> = [];

    // Добавляем пустые дни в начале (чтобы неделя начиналась с понедельника)
    const emptyDays = (startDayOfWeek + 6) % 7; // Преобразуем воскресенье = 0 в воскресенье = 6
    // Получаем последний день предыдущего месяца
    const prevMonthLastDay = new Date(year, monthNum - 1, 0).getDate();
    for (let i = 0; i < emptyDays; i++) {
      // Вычисляем дату: последний день предыдущего месяца минус количество пустых дней плюс текущий индекс
      const dayOfPrevMonth = prevMonthLastDay - emptyDays + i + 1;
      days.push({ date: new Date(year, monthNum - 2, dayOfPrevMonth), count: 0, games: [] });
    }

    // Добавляем дни месяца
    for (let day = 1; day <= daysInMonth; day++) {
      const date = new Date(year, monthNum - 1, day);
      const dateKey = date.toISOString().split('T')[0];
      const games = gamesByDate.get(dateKey) || [];

      // Ищем соответствующий день в calendarData (учитывая возможные проблемы с часовыми поясами)
      const calendarDay = calendarData?.days.find((d) => {
        const dDate = new Date(d.date);
        // Сравниваем даты без учета времени
        const dDateStr = dDate.toISOString().split('T')[0];
        const targetDateStr = date.toISOString().split('T')[0];
        return dDateStr === targetDateStr;
      });
      const calendarCount = calendarDay?.count;

      // Логика выбора count:
      // 1. Если есть реальные игры в gamesByDate - используем их количество (это более надежный источник)
      // 2. Если игр нет, но calendarCount > 0 - используем его (показываем единичку, при клике загрузим)
      // 3. При выбранном фильтре всегда используем games.length
      const count = selectedGenre 
        ? games.length 
        : (games.length > 0 ? games.length : (calendarCount ?? 0));

      days.push({
        date,
        count,
        games,
      });
    }

    // Добавляем пустые дни в конце до 35 (5 недель × 7 дней)
    const totalCells = 35;
    let nextMonthDay = 1;
    while (days.length < totalCells) {
      days.push({ date: new Date(year, monthNum, nextMonthDay), count: 0, games: [] });
      nextMonthDay++;
    }

    return days;
  }, [year, monthNum, calendarData, gamesByDate, selectedGenre]);

  const handleDayClick = async (day: { date: Date; count: number; games: Game[] }) => {
    // Разрешаем открытие при count > 0, даже если games.length === 0
    // (например, при отключенном фильтре могут быть дни с данными из API, но без игр в локальном состоянии)
    if (day.count > 0) {
      // Если есть игры в локальном состоянии, используем их
      if (day.games.length > 0) {
        setSelectedDay(day);
      } else {
        // Если игр нет, но count > 0, загружаем игры для этого дня
        try {
          const dateKey = day.date.toISOString().split('T')[0];
          const [year, monthNumStr] = dateKey.split('-');
          const monthStr = `${year}-${monthNumStr}`;
          
          // Загружаем игры через API для месяца и фильтруем по дате
          const { calendarApi } = await import('@/src/api/calendar');
          const games = await calendarApi.getGamesByMonth(monthStr, selectedGenre ?? undefined);
          
          // Фильтруем игры по конкретной дате
          const filteredGames = games.filter((game) => {
            if (!game.releaseDate) return false;
            const gameDate = new Date(game.releaseDate);
            const gameDateKey = gameDate.toISOString().split('T')[0];
            return gameDateKey === dateKey;
          });
          
          setSelectedDay({ date: day.date, games: filteredGames });
        } catch (error) {
          // В случае ошибки показываем день с пустым списком игр
          setSelectedDay({ date: day.date, games: [] });
        }
      }
    }
  };

  const handleCloseDialog = () => {
    setSelectedDay(null);
  };

  const dayNames = ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс'];
  const monthNames = [
    'Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь',
    'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь'
  ];

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-lg">Загрузка календаря...</div>
      </div>
    );
  }

  // Отображаем ошибку, если есть
  if (!calendarData && !isLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
          <h2 className="text-lg font-semibold text-red-800 dark:text-red-200 mb-2">
            Ошибка загрузки календаря
          </h2>
          <p className="text-red-600 dark:text-red-400">
            Не удалось загрузить данные календаря. Убедитесь, что бэкенд запущен и доступен.
          </p>
          {selectedGenre && (
            <p className="text-sm text-red-500 dark:text-red-300 mt-2">
              Активный фильтр: {selectedGenre}
            </p>
          )}
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-6">
        Календарь релизов - {monthNames[monthNum - 1]} {year}
      </h1>

      {selectedGenre && (
        <div className="mb-6 flex flex-wrap items-center justify-between gap-3 rounded-lg border border-blue-200 bg-blue-50 px-4 py-3 text-blue-800 dark:border-blue-900 dark:bg-blue-900/30 dark:text-blue-200">
          <span className="text-sm font-medium">
            Активный фильтр по жанру: <span className="font-semibold">{selectedGenre}</span>
          </span>
          <button
            type="button"
            onClick={clearFilters}
            className="text-sm font-semibold hover:underline"
          >
            Сбросить фильтр
          </button>
        </div>
      )}

      {/* Заголовки дней недели */}
      <div className="grid grid-cols-7 gap-2 mb-2">
        {dayNames.map((day) => (
          <div key={day} className="text-center font-semibold text-gray-600 dark:text-gray-400">
            {day}
          </div>
        ))}
      </div>

      {/* Календарная сетка */}
      <div className="grid grid-cols-7 gap-2">
        {calendarDays.map((day, index) => {
          const isCurrentMonth = day.date.getMonth() === monthNum - 1;
          const isToday = day.date.toDateString() === new Date().toDateString();
          const hasReleases = day.count > 0;

          return (
            <Card
              key={index}
              className={`min-h-[80px] ${!isCurrentMonth ? 'opacity-30' : ''} ${isToday ? 'ring-2 ring-blue-500' : ''} ${hasReleases ? 'hover:bg-blue-50 dark:hover:bg-blue-900/20 cursor-pointer' : ''}`}
              onClick={() => handleDayClick(day)}
            >
              <CardContent className="flex flex-col items-center justify-center h-full p-2">
                <div className={`text-sm ${!isCurrentMonth ? 'text-gray-400' : 'text-gray-700 dark:text-gray-300'}`}>
                  {day.date.getDate()}
                </div>
                {hasReleases && (
                  <div className="mt-1 px-2 py-1 rounded-full bg-blue-500 text-white text-xs font-semibold">
                    {day.count}
                  </div>
                )}
              </CardContent>
            </Card>
          );
        })}
      </div>

      {/* Модалка с играми */}
      <Dialog isOpen={!!selectedDay} onClose={handleCloseDialog} title="Игры на этой дате">
        <DialogContent>
          {selectedDay && selectedDay.games.length > 0 && (
            <div className="space-y-3">
              {selectedDay.games.map((game) => (
                <div
                  key={game.id}
                  className="flex items-start space-x-3 p-3 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
                >
                  {game.headerImageUrl && (
                    <img
                      src={game.headerImageUrl}
                      alt={game.name}
                      className="w-16 h-16 object-cover rounded"
                    />
                  )}
                  <div className="flex-1">
                    <h3 className="font-semibold text-lg">{game.name}</h3>
                    {game.shortDescription && (
                      <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                        {game.shortDescription}
                      </p>
                    )}
                    <div className="mt-2 flex flex-wrap gap-2">
                      {game.genres.map((genre) => (
                        <span
                          key={genre}
                          className="px-2 py-1 text-xs bg-gray-200 dark:bg-gray-700 rounded"
                        >
                          {genre}
                        </span>
                      ))}
                    </div>
                    {game.price && (
                      <p className="text-sm font-semibold mt-2">${game.price}</p>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
};

