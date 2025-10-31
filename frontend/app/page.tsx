import Link from 'next/link';

export default function Home() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-indigo-50 to-purple-50 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-20">
        {/* Hero Section */}
        <div className="text-center mb-20">
          <h1 className="text-5xl md:text-6xl font-bold text-gray-900 dark:text-gray-100 mb-6">
            🎮 Steam Releases Aggregator
          </h1>
          <p className="text-xl md:text-2xl text-gray-600 dark:text-gray-400 mb-8 max-w-3xl mx-auto">
            Отслеживайте релизы игр в Steam, анализируйте тренды жанров и планируйте свои покупки
          </p>
          <div className="flex gap-4 justify-center flex-wrap">
            <Link
              href="/calendar"
              className="px-8 py-4 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg shadow-lg transition-all duration-200 transform hover:scale-105"
            >
              📅 Календарь релизов
            </Link>
            <Link
              href="/games"
              className="px-8 py-4 bg-indigo-600 hover:bg-indigo-700 text-white font-semibold rounded-lg shadow-lg transition-all duration-200 transform hover:scale-105"
            >
              🎯 Каталог игр
            </Link>
            <Link
              href="/analytics"
              className="px-8 py-4 bg-purple-600 hover:bg-purple-700 text-white font-semibold rounded-lg shadow-lg transition-all duration-200 transform hover:scale-105"
            >
              📊 Аналитика
            </Link>
          </div>
        </div>

        {/* Features */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-20">
          <div className="bg-white dark:bg-gray-800 p-8 rounded-xl shadow-lg hover:shadow-xl transition-shadow">
            <div className="text-4xl mb-4">📅</div>
            <h3 className="text-2xl font-bold text-gray-900 dark:text-gray-100 mb-3">
              Календарь релизов
            </h3>
            <p className="text-gray-600 dark:text-gray-400">
              Интерактивный календарь с датами выхода игр. Просматривайте релизы по месяцам и не пропустите важные даты.
            </p>
          </div>

          <div className="bg-white dark:bg-gray-800 p-8 rounded-xl shadow-lg hover:shadow-xl transition-shadow">
            <div className="text-4xl mb-4">🎯</div>
            <h3 className="text-2xl font-bold text-gray-900 dark:text-gray-100 mb-3">
              Умные фильтры
            </h3>
            <p className="text-gray-600 dark:text-gray-400">
              Фильтруйте игры по платформам (Windows, Mac, Linux), жанрам и датам релиза. Найдите именно то, что вам нужно.
            </p>
          </div>

          <div className="bg-white dark:bg-gray-800 p-8 rounded-xl shadow-lg hover:shadow-xl transition-shadow">
            <div className="text-4xl mb-4">📊</div>
            <h3 className="text-2xl font-bold text-gray-900 dark:text-gray-100 mb-3">
              Аналитика жанров
            </h3>
            <p className="text-gray-600 dark:text-gray-400">
              Визуализация трендов популярных жанров. Графики и диаграммы помогут понять динамику рынка игр.
            </p>
          </div>
        </div>

        {/* Stats */}
        <div className="bg-white dark:bg-gray-800 rounded-xl shadow-lg p-8 mb-20">
          <h2 className="text-3xl font-bold text-center text-gray-900 dark:text-gray-100 mb-8">
            Возможности платформы
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6 text-center">
            <div>
              <div className="text-4xl font-bold text-blue-600 dark:text-blue-400 mb-2">1000+</div>
              <div className="text-gray-600 dark:text-gray-400">Игр в базе</div>
            </div>
            <div>
              <div className="text-4xl font-bold text-indigo-600 dark:text-indigo-400 mb-2">50+</div>
              <div className="text-gray-600 dark:text-gray-400">Жанров</div>
            </div>
            <div>
              <div className="text-4xl font-bold text-purple-600 dark:text-purple-400 mb-2">3</div>
              <div className="text-gray-600 dark:text-gray-400">Платформы</div>
            </div>
            <div>
              <div className="text-4xl font-bold text-pink-600 dark:text-pink-400 mb-2">24/7</div>
              <div className="text-gray-600 dark:text-gray-400">Обновления</div>
            </div>
          </div>
        </div>

        {/* API Info */}
        <div className="bg-gradient-to-r from-blue-600 to-purple-600 rounded-xl shadow-lg p-8 text-white text-center">
          <h2 className="text-3xl font-bold mb-4">
            Powered by Steam API
          </h2>
          <p className="text-lg mb-6 opacity-90">
            Все данные синхронизируются напрямую из Steam Store API для максимальной актуальности
          </p>
          <a
            href="http://localhost:5000/swagger"
            target="_blank"
            rel="noopener noreferrer"
            className="inline-block px-6 py-3 bg-white text-blue-600 font-semibold rounded-lg hover:bg-gray-100 transition-colors"
          >
            API Документация
          </a>
        </div>
      </div>
    </div>
  );
}
