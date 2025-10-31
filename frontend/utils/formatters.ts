/**
 * Утилиты для форматирования данных
 */

/**
 * Форматирует дату в читаемый формат
 */
export function formatDate(date: string | Date): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  return d.toLocaleDateString('ru-RU', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
}

/**
 * Форматирует цену с валютой
 */
export function formatPrice(price?: number): string {
  if (!price) return 'Бесплатно';
  return `$${price.toFixed(2)}`;
}

/**
 * Форматирует процентное значение
 */
export function formatPercentage(value: number, decimals: number = 1): string {
  return `${value.toFixed(decimals)}%`;
}

