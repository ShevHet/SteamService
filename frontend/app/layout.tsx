import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import { ReactQueryProvider } from '@/src/providers/QueryProvider';
import { FiltersProvider } from '@/src/hooks/useFilters';
import Navigation from '@/src/components/Navigation';

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Steam Releases Aggregator - Агрегатор релизов Steam",
  description: "Отслеживайте релизы игр в Steam, анализируйте тренды жанров и планируйте свои покупки",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="ru">
      <body
        className={`${geistSans.variable} ${geistMono.variable} antialiased`}
      >
        <ReactQueryProvider>
          <FiltersProvider>
            <Navigation />
            {children}
          </FiltersProvider>
        </ReactQueryProvider>
      </body>
    </html>
  );
}
