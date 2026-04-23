import type { Metadata } from "next";
import { Barlow, Barlow_Condensed, Roboto_Mono, IBM_Plex_Mono } from "next/font/google";
import "./globals.css";
import { Providers } from "@/providers";

const barlow = Barlow({
  variable: "--font-barlow",
  subsets: ["latin"],
  weight: ["300", "400", "500", "600", "700"],
});

const barlowCondensed = Barlow_Condensed({
  variable: "--font-barlow-condensed",
  subsets: ["latin"],
  weight: ["400", "600", "700", "800"],
});

const robotoMono = Roboto_Mono({
  variable: "--font-roboto-mono",
  subsets: ["latin"],
  weight: ["300", "400", "500", "700"],
});

const ibmPlexMono = IBM_Plex_Mono({
  variable: "--font-ibm-plex-mono",
  subsets: ["latin"],
  weight: ["300", "400", "500", "600", "700"],
});

export const metadata: Metadata = {
  title: "AutoAgent — Agentes que trabajan. Sin código.",
  description: "Tu equipo de operaciones construye, despliega y audita agentes de IA sobre sus propios sistemas. Sin depender de ingeniería.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="es">
      <body
        className={`${barlow.variable} ${barlowCondensed.variable} ${robotoMono.variable} ${ibmPlexMono.variable} antialiased font-sans`}
        style={{ background: '#141920', color: '#c8d8e8' }}
      >
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
