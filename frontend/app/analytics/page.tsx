'use client';

import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { GenreChart, GenreTrendChart } from '@/src/components/GenreChart';
import { Card, CardContent, CardHeader, CardTitle } from '@/src/components/ui/Card';
import { useQuery } from '@tanstack/react-query';
import api from '@/src/lib/api-client';
import { useFilters } from '@/src/hooks/useFilters';

interface GenrePieData {
  genre: string;
  count: number;
  percentage: number;
}

interface GenreTrendData {
  genre: string;
  data: Array<{
    month: string;
    avgFollowers: number | null;
    rank: number | null;
  }>;
}

export default function AnalyticsPage() {
  const [selectedMonth, setSelectedMonth] = useState('2025-10');
  const [chartType, setChartType] = useState<'pie' | 'bar'>('pie');
  const { selectedGenre, toggleGenre, clear: clearFilters } = useFilters();

  // Загрузка данных для Pie Chart
  const { data: genrePieData, isLoading: isPieLoading } = useQuery<GenrePieData[]>({
    queryKey: ['genres-pie', selectedMonth],
    queryFn: async () => {
      const response = await api.get(`/v1/analytics/genres-pie?month=${selectedMonth}`);
      return response.data;
    },
  });

  // Загрузка данных для трендов
  const { data: trendData, isLoading: isTrendLoading } = useQuery<GenreTrendData[]>({
    queryKey: ['genres-trends'],
    queryFn: async () => {
      const response = await api.get('/v1/analytics/trends?months=6');
      return response.data;
    },
  });

  useEffect(() => {
    // Не сбрасываем фильтр автоматически - пользователь может выбрать жанр,
    // который есть в других месяцах или будет добавлен позже
    // Это может быть полезно при переходе между страницами
    if (!selectedGenre || !genrePieData || genrePieData.length === 0) return;

    const exists = genrePieData.some((item) => item.genre.toLowerCase() === selectedGenre.toLowerCase());
    // Не сбрасываем фильтр автоматически, если жанр не найден в текущем месяце
    // Это позволяет сохранять фильтр при переходе между страницами
  }, [selectedGenre, genrePieData]);

  const pieChartData = useMemo(() => genrePieData ?? [], [genrePieData]);

  const filteredPieData = useMemo(() => {
    if (!genrePieData) return [];
    if (!selectedGenre) return genrePieData;

    const lower = selectedGenre.toLowerCase();
    const match = genrePieData.filter((item) => item.genre.toLowerCase() === lower);
    return match.length > 0 ? match : genrePieData;
  }, [genrePieData, selectedGenre]);

  const filteredTrendData = useMemo(() => {
    if (!trendData) return [];
    if (!selectedGenre) return trendData;

    const lower = selectedGenre.toLowerCase();
    const match = trendData.filter((item) => item.genre.toLowerCase() === lower);
    return match.length > 0 ? match : trendData;
  }, [trendData, selectedGenre]);

  const handleGenreSelect = useCallback(
    (genre: string) => {
      toggleGenre(genre);
    },
    [toggleGenre]
  );

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-6">
      <div className="max-w-7xl mx-auto">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100 mb-8">
          📊 Аналитика по жанрам
        </h1>

        {/* Фильтры */}
        <Card className="mb-6">
          <CardContent className="pt-6">
            <div className="flex flex-wrap gap-4 items-center">
              <div className="flex items-center gap-2">
                <label htmlFor="month" className="text-sm font-medium text-gray-700 dark:text-gray-300">
                  Месяц:
                </label>
                <select
                  id="month"
                  value={selectedMonth}
                  onChange={(e) => setSelectedMonth(e.target.value)}
                  className="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:ring-2 focus:ring-blue-500"
                >
                  <option value="2025-10">Октябрь 2025</option>
                  <option value="2025-11">Ноябрь 2025</option>
                  <option value="2025-12">Декабрь 2025</option>
                  <option value="2026-01">Январь 2026</option>
                </select>
              </div>

              <div className="flex items-center gap-2">
                <label className="text-sm font-medium text-gray-700 dark:text-gray-300">
                  Тип графика:
                </label>
                <div className="flex gap-2">
                  <button
                    onClick={() => setChartType('pie')}
                    className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
                      chartType === 'pie'
                        ? 'bg-blue-500 text-white'
                        : 'bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-300 dark:hover:bg-gray-600'
                    }`}
                  >
                    Круговая
                  </button>
                  <button
                    onClick={() => setChartType('bar')}
                    className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
                      chartType === 'bar'
                        ? 'bg-blue-500 text-white'
                        : 'bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-300 dark:hover:bg-gray-600'
                    }`}
                  >
                    Столбчатая
                  </button>
                </div>
              </div>
            </div>
            {selectedGenre && (
              <div className="mt-4 flex flex-wrap items-center justify-between gap-3 rounded-lg border border-blue-200 bg-blue-50 px-4 py-3 text-sm text-blue-800 dark:border-blue-900 dark:bg-blue-900/30 dark:text-blue-200">
                <span>
                  Активный фильтр по жанру: <span className="font-semibold">{selectedGenre}</span>
                </span>
                <button
                  type="button"
                  onClick={clearFilters}
                  className="font-semibold hover:underline"
                >
                  Сбросить фильтр
                </button>
              </div>
            )}
          </CardContent>
        </Card>

        {/* График распределения жанров */}
        <Card className="mb-6">
          <CardHeader>
            <CardTitle>Распределение игр по жанрам</CardTitle>
          </CardHeader>
          <CardContent 
            style={{ position: 'relative', minHeight: '600px' }}
          >
            {isPieLoading ? (
              <div className="flex items-center justify-center h-96">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
              </div>
            ) : genrePieData && genrePieData.length > 0 ? (
              <div data-chart-area style={{ position: 'relative', width: '100%' }}>
                <GenreChart 
                  data={pieChartData}
                  type={chartType}
                  height={500}
                  selectedGenre={selectedGenre}
                  onSelectGenre={handleGenreSelect}
                />
              </div>
            ) : (
              <div className="flex items-center justify-center h-64 text-gray-500">
                Нет данных за выбранный месяц
              </div>
            )}
          </CardContent>
        </Card>

        {/* Тренды жанров */}
        <Card>
          <CardHeader>
            <CardTitle>Тренды популярности жанров</CardTitle>
          </CardHeader>
          <CardContent>
            {isTrendLoading ? (
              <div className="flex items-center justify-center h-96">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
              </div>
            ) : trendData && trendData.length > 0 ? (
              <GenreTrendChart data={filteredTrendData} height={500} />
            ) : (
              <div className="flex items-center justify-center h-64 text-gray-500">
                Нет данных для отображения трендов
              </div>
            )}
          </CardContent>
        </Card>

        {/* Статистика */}
        {genrePieData && genrePieData.length > 0 && (
          <Card className="mt-6">
            <CardHeader>
              <CardTitle>Топ жанров за {selectedMonth}</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {(filteredPieData.length > 0 ? filteredPieData : genrePieData).slice(0, 6).map((genre, index) => (
                  <div
                    key={genre.genre}
                    className="p-4 bg-gradient-to-br from-blue-50 to-indigo-50 dark:from-gray-800 dark:to-gray-700 rounded-lg border border-blue-100 dark:border-gray-600"
                  >
                    <div className="flex items-center justify-between mb-2">
                      <span className="text-2xl font-bold text-blue-600 dark:text-blue-400">
                        #{index + 1}
                      </span>
                      <span className="text-lg font-semibold text-gray-700 dark:text-gray-300">
                        {genre.percentage.toFixed(1)}%
                      </span>
                    </div>
                    <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-1">
                      {genre.genre}
                    </h3>
                    <p className="text-sm text-gray-600 dark:text-gray-400">
                      {genre.count} {genre.count === 1 ? 'игра' : 'игр'}
                    </p>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}

