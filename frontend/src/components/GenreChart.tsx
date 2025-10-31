'use client';

import React, { useCallback, useMemo, useRef } from 'react';
import {
  PieChart,
  Pie,
  Cell,
  ResponsiveContainer,
  Legend,
  Tooltip,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
} from 'recharts';
import type { TooltipProps } from 'recharts';
import type { PieLabelRenderProps } from 'recharts/types/polar/Pie';
import type { NameType, ValueType } from 'recharts/types/component/DefaultTooltipContent';

interface GenreData {
  genre: string;
  count: number;
  percentage?: number;
}

interface GenreChartProps {
  data: GenreData[];
  type?: 'pie' | 'bar';
  title?: string;
  height?: number;
  selectedGenre?: string | null;
  onSelectGenre?: (genre: string) => void;
}

// Цветовая палитра для графиков
const COLORS = [
  '#3b82f6', // blue-500
  '#10b981', // green-500
  '#f59e0b', // amber-500
  '#ef4444', // red-500
  '#8b5cf6', // violet-500
  '#ec4899', // pink-500
  '#06b6d4', // cyan-500
  '#f97316', // orange-500
  '#6366f1', // indigo-500
  '#14b8a6', // teal-500
];

const getGenreColor = (genre: string): string => {
  let hash = 0;
  for (let i = 0; i < genre.length; i += 1) {
    hash = (hash * 31 + genre.charCodeAt(i)) >>> 0;
  }
  return COLORS[hash % COLORS.length];
};

type ChartDatum = {
  name: string;
  value: number;
  percentage?: number;
  color: string;
};

const CustomTooltip: React.FC<TooltipProps<ValueType, NameType>> = ({ active, payload }) => {
  if (active && payload && payload.length) {
    const datum = payload[0].payload as ChartDatum;
    return (
      <div className="bg-white dark:bg-gray-800 p-3 rounded-lg shadow-lg border border-gray-200 dark:border-gray-700">
        <p className="font-semibold text-gray-900 dark:text-gray-100">
          {datum.name}
        </p>
        <p className="text-sm text-gray-600 dark:text-gray-400">
          Игр: <span className="font-medium">{datum.value}</span>
        </p>
        {datum.percentage !== undefined && (
          <p className="text-sm text-gray-600 dark:text-gray-400">
            Процент: <span className="font-medium">{datum.percentage.toFixed(1)}%</span>
          </p>
        )}
      </div>
    );
  }
  return null;
};

export const GenreChart: React.FC<GenreChartProps> = ({
  data,
  type = 'pie',
  title,
  height = 400,
  selectedGenre,
  onSelectGenre,
}) => {
  const containerRef = useRef<HTMLDivElement | null>(null);

  const chartData = useMemo<ChartDatum[]>(
    () =>
      data.map((item) => ({
        name: item.genre,
        value: item.count,
        percentage: item.percentage,
        color: getGenreColor(item.genre),
      })),
    [data]
  );

  const handleSelectGenre = useCallback(
    (genre: string) => {
      onSelectGenre?.(genre);
    },
    [onSelectGenre]
  );

  const renderCustomLabel = (entry: PieLabelRenderProps) => {
    const { name, value } = entry;
    if (name === undefined || value === undefined) {
      return '';
    }
    return `${name}: ${value}`;
  };

  if (chartData.length === 0) {
    return (
      <div className="flex items-center justify-center h-64 text-gray-500">
        Нет данных для отображения
      </div>
    );
  }

  if (type === 'pie') {
    return (
      <div 
        ref={containerRef} 
        className="w-full relative" 
        style={{ padding: '20px', minHeight: `${height + 100}px`, width: '100%' }}
      >
        {title && (
          <h3 className="text-lg font-semibold mb-4 text-gray-900 dark:text-gray-100">
            {title}
          </h3>
        )}
        <div style={{ width: '100%', height: `${height}px`, position: 'relative' }}>
            <ResponsiveContainer width="100%" height={height}>
          <PieChart>
            <Pie
              data={chartData}
              cx="50%"
              cy="50%"
              labelLine={false}
              label={renderCustomLabel}
              outerRadius={120}
              fill="#8884d8"
              dataKey="value"
              onClick={(entry, index) => {
                const genre = chartData[index]?.name;
                if (genre) {
                  handleSelectGenre(genre);
                }
              }}
            >
              {chartData.map((entry, index) => (
                <Cell
                  key={`cell-${index}`}
                  fill={entry.color}
                  fillOpacity={
                    selectedGenre
                      ? entry.name.toLowerCase() === selectedGenre.toLowerCase()
                        ? 1
                        : 0.35
                      : 1
                  }
                  stroke={entry.name.toLowerCase() === selectedGenre?.toLowerCase() ? '#111827' : '#fff'}
                  strokeWidth={entry.name.toLowerCase() === selectedGenre?.toLowerCase() ? 2 : 1}
                  style={{ cursor: onSelectGenre ? 'pointer' : 'default' }}
                />
              ))}
            </Pie>
            <Tooltip content={<CustomTooltip />} />
            <Legend 
              verticalAlign="bottom" 
              height={36}
              formatter={(value: string | number) => (
                <span className="text-sm text-gray-700 dark:text-gray-300">
                  {value}
                </span>
              )}
            />
          </PieChart>
          </ResponsiveContainer>
        </div>
      </div>
    );
  }

  // Bar Chart
  return (
    <div 
      ref={containerRef} 
      className="w-full relative" 
      style={{ padding: '20px', minHeight: `${height + 100}px`, width: '100%' }}
    >
      {title && (
        <h3 className="text-lg font-semibold mb-4 text-gray-900 dark:text-gray-100">
          {title}
        </h3>
      )}
      <div style={{ width: '100%', height: `${height}px`, position: 'relative' }}>
          <ResponsiveContainer width="100%" height={height}>
        <BarChart data={chartData} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
          <XAxis 
            dataKey="name" 
            tick={{ fill: '#6b7280', fontSize: 12 }}
            angle={-45}
            textAnchor="end"
            height={100}
          />
          <YAxis tick={{ fill: '#6b7280', fontSize: 12 }} />
          <Tooltip content={<CustomTooltip />} />
          <Bar dataKey="value" fill="#3b82f6" radius={[8, 8, 0, 0]}>
            {chartData.map((entry, index) => (
              <Cell
                key={`cell-${index}`}
                fill={entry.color}
                fillOpacity={
                  selectedGenre
                    ? entry.name.toLowerCase() === selectedGenre.toLowerCase()
                      ? 1
                      : 0.35
                    : 1
                }
                style={{ cursor: onSelectGenre ? 'pointer' : 'default' }}
                onClick={() => {
                  handleSelectGenre(entry.name);
                }}
              />
            ))}
          </Bar>
        </BarChart>
          </ResponsiveContainer>
        </div>
      </div>
    );
  }

/**
 * Компонент для отображения трендов жанров
 */
interface GenreTrendData {
  genre: string;
  data: Array<{
    month: string;
    avgFollowers: number | null;
    rank: number | null;
  }>;
}

interface GenreTrendChartProps {
  data: GenreTrendData[];
  height?: number;
}

export const GenreTrendChart: React.FC<GenreTrendChartProps> = ({ data, height = 400 }) => {
  if (!data || data.length === 0) {
    return (
      <div className="flex items-center justify-center h-64 text-gray-500">
        Нет данных для отображения
      </div>
    );
  }

  // Преобразуем данные для графика трендов
  const months = data[0]?.data.map(d => d.month) || [];
  type TrendChartRow = {
    month: string;
    [genre: string]: string | number;
  };

  const chartData = months.map((month) => {
    const point: TrendChartRow = { month };
    data.forEach((genre) => {
      const monthData = genre.data.find((d) => d.month === month);
      if (monthData && monthData.avgFollowers !== null) {
        point[genre.genre] = monthData.avgFollowers;
      }
    });
    return point;
  });

  return (
    <div className="w-full">
      <h3 className="text-lg font-semibold mb-4 text-gray-900 dark:text-gray-100">
        Тренды жанров по средним фолловерам
      </h3>
      <ResponsiveContainer width="100%" height={height}>
        <BarChart data={chartData} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
          <XAxis 
            dataKey="month" 
            tick={{ fill: '#6b7280', fontSize: 12 }}
          />
          <YAxis 
            tick={{ fill: '#6b7280', fontSize: 12 }}
            label={{ value: 'Средние фолловеры', angle: -90, position: 'insideLeft' }}
          />
          <Tooltip 
            contentStyle={{ 
              backgroundColor: 'white', 
              border: '1px solid #e5e7eb',
              borderRadius: '8px',
              padding: '12px'
            }}
          />
          <Legend />
          {data.map((genre) => (
            <Bar 
              key={genre.genre} 
              dataKey={genre.genre} 
              fill={getGenreColor(genre.genre)}
              radius={[4, 4, 0, 0]}
            />
          ))}
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
};

export default GenreChart;

