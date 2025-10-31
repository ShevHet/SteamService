# 🎮 SteamDB Calendar Clone

Полнофункциональный клон календаря релизов Steam-игр с аналитикой и статистикой.

## 📋 Описание проекта

SteamDB Calendar Clone - это fullstack-приложение для агрегации и визуализации релизов игр из Steam Store. Проект позволяет:
- Просматривать календарь релизов с фильтрацией по платформам и жанрам
- Анализировать популярность жанров с помощью интерактивных графиков
- Отслеживать тренды и статистику за разные периоды
- Синхронизировать данные из Steam Store API

## 🛠 Технологический стек

### Backend
- **ASP.NET Core 8.0** - фреймворк веб-API
- **PostgreSQL 15** - реляционная база данных
- **Entity Framework Core** - ORM для работы с БД
- **Polly** - библиотека для устойчивости (retry, circuit breaker)
- **Swagger** - документация API
- **Npgsql** - драйвер для PostgreSQL

### Frontend
- **Next.js 16** - React-фреймворк с SSR
- **React 19** - библиотека UI
- **TypeScript 5** - типизированный JavaScript
- **Tailwind CSS 4** - utility-first CSS фреймворк
- **TanStack Query (React Query)** - управление состоянием и кешированием
- **Recharts** - библиотека для графиков и диаграмм
- **Axios** - HTTP клиент

### DevOps
- **Docker** - контейнеризация
- **Docker Compose** - оркестрация контейнеров
- **Git** - система контроля версий

## 🚀 Быстрый старт

### Требования

- Docker Desktop 20.10+
- Docker Compose 2.0+

### Запуск проекта

1. **Клонируйте репозиторий:**
```bash
git clone https://github.com/yourusername/SteamService.git
cd SteamService
```

2. **Запустите все сервисы:**
```bash
docker-compose up --build
```

3. **Откройте в браузере:**
- Frontend: http://localhost:3000
- Backend API: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger
- PostgreSQL: localhost:5432

### Остановка проекта

```bash
docker-compose down
```

Для удаления данных базы:
```bash
docker-compose down -v
```

## 📁 Структура проекта

```
SteamService/
├── backend/                    # Backend приложение
│   ├── Controllers/           # API контроллеры
│   │   ├── AnalyticsController.cs
│   │   ├── GameController.cs
│   │   ├── HealthController.cs
│   │   └── SteamController.cs
│   ├── Data/                  # EF Core контекст
│   │   ├── ApplicationDbContext.cs
│   │   ├── DataSeeder.cs
│   │   └── DataSeederRunner.cs
│   ├── DTOs/                  # Data Transfer Objects
│   ├── Migrations/            # Миграции БД
│   ├── Models/                # Модели данных
│   ├── Services/              # Бизнес-логика
│   │   ├── ISteamService.cs
│   │   ├── SteamService.cs
│   │   └── SteamSyncBackgroundService.cs
│   ├── appsettings.json       # Конфигурация
│   ├── Program.cs             # Точка входа
│   └── Dockerfile             # Docker конфигурация
│
├── frontend/                   # Frontend приложение
│   ├── app/                   # Next.js App Router
│   │   ├── analytics/        # Страница аналитики
│   │   ├── calendar/         # Страница календаря
│   │   ├── games/            # Страница списка игр
│   │   ├── layout.tsx
│   │   └── page.tsx
│   ├── src/
│   │   ├── components/       # React компоненты
│   │   │   ├── CalendarPage.tsx
│   │   │   ├── GenreChart.tsx
│   │   │   ├── GameCard.tsx
│   │   │   └── ui/           # UI компоненты
│   │   ├── hooks/            # Custom React hooks
│   │   ├── lib/              # Утилиты и клиенты
│   │   ├── types/            # TypeScript типы
│   │   └── providers/        # Context providers
│   ├── package.json
│   └── Dockerfile            # Docker конфигурация
│
├── Migrations/                # Миграции EF Core
├── docker-compose.yml         # Docker Compose конфигурация
├── SteamService.csproj        # Проект .NET
└── README.md                  # Документация

```

## 🔌 API Эндпоинты

### Games API (`/api/v1/games`)

| Метод | Эндпоинт | Описание | Параметры |
|-------|----------|----------|-----------|
| GET | `/api/v1/games` | Список игр за месяц | `month`, `platform?`, `genre?` |
| GET | `/api/v1/games/calendar` | Календарь релизов | `month`, `platform?`, `genre?` |

**Пример:**
```bash
GET http://localhost:8080/api/v1/games?month=2025-11&platform=windows
GET http://localhost:8080/api/v1/games/calendar?month=2025-11&genre=Action
```

### Analytics API (`/api/v1/analytics`)

| Метод | Эндпоинт | Описание | Параметры |
|-------|----------|----------|-----------|
| GET | `/api/v1/analytics/genres-pie` | Статистика жанров (pie chart) | `month`, `platform?` |
| GET | `/api/v1/analytics/trends` | Тренды жанров | `months=6`, `includeCurrentMonth?` |

**Пример:**
```bash
GET http://localhost:8080/api/v1/analytics/genres-pie?month=2025-11
GET http://localhost:8080/api/v1/analytics/trends?months=6
```

### Steam API (`/api/steam`)

| Метод | Эндпоинт | Описание |
|-------|----------|----------|
| POST | `/api/steam/sync` | Запуск синхронизации данных из Steam |

### Health API (`/api/health`)

| Метод | Эндпоинт | Описание |
|-------|----------|----------|
| GET | `/api/health` | Проверка состояния сервиса |
| GET | `/health` | Endpoint для Docker healthcheck |

## 🔧 Конфигурация

### Переменные окружения

#### Backend
- `ASPNETCORE_ENVIRONMENT` - окружение (Development/Production)
- `ConnectionStrings__DefaultConnection` - строка подключения к PostgreSQL
- `ASPNETCORE_URLS` - URL для запуска (по умолчанию http://+:8080)

#### Frontend
- `NEXT_PUBLIC_API_BASE_URL` - базовый URL API (http://backend:8080/api)
- `NODE_ENV` - окружение (development/production)

### База данных

**Подключение:**
```
Host=db;Port=5432;Database=steamaggregator;Username=postgres;Password=postgres
```

**Автоматическая миграция:** При первом запуске применяются все pending миграции

**Seeder:** При старте автоматически заполняются тестовые данные

## 📊 Функциональность

### Календарь релизов
- 📅 Просмотр релизов по дням месяца
- 🎯 Фильтрация по платформам (Windows, Linux, macOS)
- 🎮 Фильтрация по жанрам
- 📱 Адаптивный дизайн

### Аналитика
- 📈 Pie Chart - распределение жанров
- 📊 Bar Chart - сравнительная статистика
- 📉 Trend Chart - динамика популярности жанров
- 🔢 Показ процентов и количественных данных

### Дополнительно
- ⚡ Автоматическая синхронизация с Steam Store
- 🔄 Health checks и мониторинг
- 📝 Swagger документация
- 🐳 Docker контейнеризация

## 🗺 Roadmap / TODO

### ✅ Завершено
- [x] Backend API с PostgreSQL
- [x] Frontend на Next.js с TypeScript
- [x] Календарь релизов с фильтрацией
- [x] Аналитика и графики
- [x] Docker контейнеризация
- [x] Автоматические миграции БД
- [x] Seeder данных
- [x] Swagger документация

### 🚧 В разработке
- [ ] Аутентификация и авторизация
- [ ] Избранные игры
- [ ] Уведомления о релизах
- [ ] Экспорт данных в CSV/JSON

### 🔮 Планируется
- [ ] Мобильное приложение
- [ ] Расширенная аналитика
- [ ] Сравнение игр
- [ ] Интеграция с другими платформами
- [ ] Система рекомендаций

## 🧪 Тестирование

### Health Check
```bash
curl http://localhost:8080/health
```

### API Endpoints
```bash
# Получить игры за месяц
curl http://localhost:8080/api/v1/games?month=2025-11

# Получить статистику жанров
curl http://localhost:8080/api/v1/analytics/genres-pie?month=2025-11

# Запустить синхронизацию
curl -X POST http://localhost:8080/api/steam/sync
```

## 🖼 Скриншоты

> _Здесь будут размещены скриншоты интерфейса_
>
> 1. Главная страница
> 2. Календарь релизов
> 3. Страница аналитики
> 4. Детали игры

## 📝 Лицензия

MIT License

## 👥 Авторы

Development Team

---

**Приятного использования! 🎮**
