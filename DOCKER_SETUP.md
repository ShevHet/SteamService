# 🐳 Docker Setup Guide

Краткое руководство по запуску проекта в Docker.

## Быстрый старт

```bash
docker-compose up --build
```

## Структура сервисов

| Сервис | Порт | URL | Описание |
|--------|------|-----|----------|
| **Frontend** | 3000 | http://localhost:3000 | Next.js приложение |
| **Backend** | 8080 | http://localhost:8080 | ASP.NET API |
| **PostgreSQL** | 5432 | localhost:5432 | База данных |
| **Swagger** | - | http://localhost:8080/swagger | API документация |

## Остановка

```bash
# Остановить контейнеры
docker-compose down

# Остановить и удалить volumes (включая БД)
docker-compose down -v
```

## Переменные окружения

### Backend
- `ASPNETCORE_ENVIRONMENT`: Development/Production
- `ConnectionStrings__DefaultConnection`: строка подключения к PostgreSQL
- `ASPNETCORE_URLS`: URL для запуска (по умолчанию http://+:8080)

### Frontend
- `NEXT_PUBLIC_API_BASE_URL`: базовый URL API (http://backend:8080/api)
- `NODE_ENV`: production/development

## Troubleshooting

### Ports заняты

```bash
# Проверить занятые порты на Windows
netstat -ano | findstr "3000"
netstat -ano | findstr "8080"

# Освободить порт
taskkill /PID <PID> /F
```

### Пересборка образов

```bash
# Полная пересборка
docker-compose build --no-cache

# Пересборка конкретного сервиса
docker-compose build --no-cache backend
```

### Логи

```bash
# Все сервисы
docker-compose logs -f

# Конкретный сервис
docker-compose logs -f backend
docker-compose logs -f frontend
```

### База данных

```bash
# Подключение к PostgreSQL
docker exec -it steam_db psql -U postgres -d steamaggregator

# Сброс базы данных
docker-compose down -v
docker-compose up --build
```

## Health Checks

Все сервисы имеют health checks:

```bash
# Backend
curl http://localhost:8080/health

# Frontend (простая проверка)
curl http://localhost:3000
```

## Production

Для production сборки измените в `docker-compose.yml`:

```yaml
backend:
  environment:
    - ASPNETCORE_ENVIRONMENT=Production

frontend:
  environment:
    - NODE_ENV=production
```

