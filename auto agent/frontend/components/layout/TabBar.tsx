"use client";

import { useAppStore, View } from "@/store/useAppStore";
import { cn } from "@/lib/utils";

const TABS: { label: string; view: View }[] = [
  { label: "Landing",    view: "landing" },
  { label: "Dashboard",  view: "dashboard" },
  { label: "Constructor",view: "builder" },
  { label: "Auditoría",  view: "audit" },
];

export function TabBar() {
  const { view, setView } = useAppStore();

  return (
    <header className="fixed top-0 left-0 right-0 z-50 h-12 bg-[#0c0b09] flex items-stretch border-b-2 border-[#c84b1a]">
      {/* Logo */}
      <div className="flex items-center gap-2.5 px-6 border-r border-white/10 font-[family-name:var(--font-barlow-condensed)] font-extrabold text-[17px] tracking-wide text-white">
        <span className="w-[22px] h-[22px] bg-[#c84b1a] flex items-center justify-center text-[11px] font-black text-white rounded-[2px]">
          A
        </span>
        AutoAgent
      </div>

      {/* Tabs */}
      <nav className="flex flex-1">
        {TABS.map((t) => (
          <button
            key={t.view}
            onClick={() => setView(t.view)}
            className={cn(
              "flex items-center px-[22px] font-[family-name:var(--font-barlow-condensed)] text-[13px] font-semibold tracking-[0.8px] uppercase border-r border-white/[0.07] cursor-pointer transition-all duration-150 select-none",
              view === t.view
                ? "text-white bg-[rgba(200,75,26,0.2)] border-b-2 border-b-[#c84b1a] -mb-0.5"
                : "text-white/40 hover:text-white/75 hover:bg-white/[0.04]"
            )}
          >
            {t.label}
          </button>
        ))}
      </nav>

      {/* Right actions */}
      <div className="flex items-center gap-2 px-4 ml-auto">
        <button className="px-[14px] py-[5px] rounded-[2px] text-[12px] font-semibold font-[family-name:var(--font-barlow-condensed)] tracking-[0.5px] uppercase border border-white/25 text-white/70 bg-transparent hover:border-white hover:text-white transition-all duration-150">
          Docs
        </button>
        <button
          onClick={() => setView("dashboard")}
          className="px-[14px] py-[5px] rounded-[2px] text-[12px] font-semibold font-[family-name:var(--font-barlow-condensed)] tracking-[0.5px] uppercase bg-[#c84b1a] text-white border-none hover:bg-[#e05a22] transition-all duration-150"
        >
          Entrar →
        </button>
      </div>
    </header>
  );
}
