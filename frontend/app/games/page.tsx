'use client';

import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/src/components/ui/Card';
import api from '@/src/lib/api-client';

interface Game {
  id: string;
  steamAppId: number | null;
  name: string;
  slug: string | null;
  shortDescription: string | null;
  releaseDate: string | null;
  price: number | null;
  metacriticScore: number | null;
  genres: string[];
  platforms: string[];
  storeUrl: string | null;
  headerImageUrl: string | null;
  isComingSoon: boolean;
  createdAt: string;
}

export default function GamesPage() {
  const [selectedMonth, setSelectedMonth] = useState('2025-10');
  const [selectedPlatform, setSelectedPlatform] = useState<string>('');
  const [selectedGenre, setSelectedGenre] = useState<string>('');

  // Загрузка игр
  const { data: games, isLoading, error } = useQuery<Game[]>({
    queryKey: ['games', selectedMonth, selectedPlatform, selectedGenre],
    queryFn: async () => {
      let url = `/v1/games?month=${selectedMonth}`;
      if (selectedPlatform) url += `&platform=${selectedPlatform}`;
      if (selectedGenre) url += `&genre=${selectedGenre}`;
      
      const response = await api.get(url);
      return response.data;
    },
  });

  // Уникальные жанры и платформы для фильтров
  const allGenres = React.useMemo(() => {
    if (!games) return [];
    const genres = new Set<string>();
    games.forEach(game => game.genres.forEach(g => genres.add(g)));
    return Array.from(genres).sort();
  }, [games]);

  const allPlatforms = React.useMemo(() => {
    if (!games) return [];
    const platforms = new Set<string>();
    games.forEach(game => game.platforms.forEach(p => platforms.add(p)));
    return Array.from(platforms).sort();
  }, [games]);

  const formatDate = (dateString: string | null) => {
    if (!dateString) return 'TBA';
    const date = new Date(dateString);
    return date.toLocaleDateString('ru-RU', { year: 'numeric', month: 'long', day: 'numeric' });
  };

  const formatPrice = (price: number | null) => {
    if (price === null) return 'Цена не указана';
    if (price === 0) return 'Бесплатно';
    return `${price} ₽`;
  };

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-6">
      <div className="max-w-7xl mx-auto">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100 mb-8">
          🎮 Каталог игр
        </h1>

        {/* Фильтры */}
        <Card className="mb-6">
          <CardContent className="pt-6">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <label htmlFor="month" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Месяц релиза:
                </label>
                <select
                  id="month"
                  value={selectedMonth}
                  onChange={(e) => setSelectedMonth(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:ring-2 focus:ring-blue-500"
                >
                  <option value="2025-10">Октябрь 2025</option>
                  <option value="2025-11">Ноябрь 2025</option>
                  <option value="2025-12">Декабрь 2025</option>
                  <option value="2026-01">Январь 2026</option>
                </select>
              </div>

              <div>
                <label htmlFor="platform" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Платформа:
                </label>
                <select
                  id="platform"
                  value={selectedPlatform}
                  onChange={(e) => setSelectedPlatform(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:ring-2 focus:ring-blue-500"
                >
                  <option value="">Все платформы</option>
                  {allPlatforms.map(platform => (
                    <option key={platform} value={platform}>{platform}</option>
                  ))}
                </select>
              </div>

              <div>
                <label htmlFor="genre" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Жанр:
                </label>
                <select
                  id="genre"
                  value={selectedGenre}
                  onChange={(e) => setSelectedGenre(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:ring-2 focus:ring-blue-500"
                >
                  <option value="">Все жанры</option>
                  {allGenres.map(genre => (
                    <option key={genre} value={genre}>{genre}</option>
                  ))}
                </select>
              </div>
            </div>

            {/* Кнопка сброса фильтров */}
            {(selectedPlatform || selectedGenre) && (
              <div className="mt-4">
                <button
                  onClick={() => {
                    setSelectedPlatform('');
                    setSelectedGenre('');
                  }}
                  className="px-4 py-2 text-sm font-medium text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300"
                >
                  Сбросить фильтры
                </button>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Список игр */}
        {isLoading ? (
          <div className="flex items-center justify-center h-64">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
          </div>
        ) : error ? (
          <Card>
            <CardContent className="pt-6">
              <div className="text-center text-red-600 dark:text-red-400">
                Ошибка загрузки данных
              </div>
            </CardContent>
          </Card>
        ) : !games || games.length === 0 ? (
          <Card>
            <CardContent className="pt-6">
              <div className="text-center text-gray-500 dark:text-gray-400">
                Игры не найдены за выбранный период
              </div>
            </CardContent>
          </Card>
        ) : (
          <>
            {/* Статистика */}
            <div className="mb-6 flex items-center justify-between">
              <p className="text-sm text-gray-600 dark:text-gray-400">
                Найдено игр: <span className="font-semibold text-gray-900 dark:text-gray-100">{games.length}</span>
              </p>
            </div>

            {/* Сетка карточек игр */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {games.map((game) => (
                <Card key={game.id} className="hover:shadow-lg transition-shadow duration-200">
                  <CardContent className="p-0">
                    {/* Изображение */}
                    {game.headerImageUrl ? (
                      <img
                        src={game.headerImageUrl}
                        alt={game.name}
                        className="w-full h-48 object-cover rounded-t-lg"
                      />
                    ) : (
                      <div className="w-full h-48 bg-gradient-to-br from-gray-200 to-gray-300 dark:from-gray-700 dark:to-gray-800 rounded-t-lg flex items-center justify-center">
                        <span className="text-4xl">🎮</span>
                      </div>
                    )}

                    {/* Контент */}
                    <div className="p-4">
                      {/* Название */}
                      <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-2 line-clamp-2">
                        {game.name}
                      </h3>

                      {/* Описание */}
                      {game.shortDescription && (
                        <p className="text-sm text-gray-600 dark:text-gray-400 mb-3 line-clamp-2">
                          {game.shortDescription}
                        </p>
                      )}

                      {/* Жанры */}
                      {game.genres.length > 0 && (
                        <div className="flex flex-wrap gap-2 mb-3">
                          {game.genres.slice(0, 3).map((genre) => (
                            <span
                              key={genre}
                              className="px-2 py-1 text-xs font-medium bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200 rounded"
                            >
                              {genre}
                            </span>
                          ))}
                          {game.genres.length > 3 && (
                            <span className="px-2 py-1 text-xs font-medium bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-400 rounded">
                              +{game.genres.length - 3}
                            </span>
                          )}
                        </div>
                      )}

                      {/* Платформы */}
                      {game.platforms.length > 0 && (
                        <div className="flex flex-wrap gap-2 mb-3">
                          {game.platforms.map((platform) => (
                            <span
                              key={platform}
                              className="text-xs text-gray-600 dark:text-gray-400"
                            >
                              {platform === 'windows' && '🪟'}
                              {platform === 'mac' && '🍎'}
                              {platform === 'linux' && '🐧'}
                              {!['windows', 'mac', 'linux'].includes(platform) && '💻'}
                            </span>
                          ))}
                        </div>
                      )}

                      {/* Информация */}
                      <div className="space-y-2 mb-4">
                        <div className="flex items-center justify-between text-sm">
                          <span className="text-gray-600 dark:text-gray-400">Дата релиза:</span>
                          <span className="font-medium text-gray-900 dark:text-gray-100">
                            {formatDate(game.releaseDate)}
                          </span>
                        </div>
                        <div className="flex items-center justify-between text-sm">
                          <span className="text-gray-600 dark:text-gray-400">Цена:</span>
                          <span className="font-medium text-gray-900 dark:text-gray-100">
                            {formatPrice(game.price)}
                          </span>
                        </div>
                        {game.metacriticScore && (
                          <div className="flex items-center justify-between text-sm">
                            <span className="text-gray-600 dark:text-gray-400">Metacritic:</span>
                            <span
                              className={`font-medium ${
                                game.metacriticScore >= 75
                                  ? 'text-green-600 dark:text-green-400'
                                  : game.metacriticScore >= 50
                                  ? 'text-yellow-600 dark:text-yellow-400'
                                  : 'text-red-600 dark:text-red-400'
                              }`}
                            >
                              {game.metacriticScore}
                            </span>
                          </div>
                        )}
                      </div>

                      {/* Ссылка на Steam */}
                      {game.storeUrl && (
                        <a
                          href={game.storeUrl}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="block w-full px-4 py-2 text-center text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-md transition-colors"
                        >
                          Открыть в Steam
                        </a>
                      )}
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          </>
        )}
      </div>
    </div>
  );
}

