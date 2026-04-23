"use client";

import { useAppStore } from "@/store/useAppStore";
import { cn } from "@/lib/utils";

interface SidebarItem {
  icon: string;
  label: string;
  view?: "dashboard" | "builder" | "audit";
  badge?: string;
  active?: boolean;
}

const ITEMS: SidebarItem[] = [
  { icon: "⊞", label: "Dashboard",    view: "dashboard", active: true },
  { icon: "⬡", label: "Mis agentes",  view: "builder" },
  { icon: "✦", label: "Constructor",  view: "builder" },
];

const SYSTEM_ITEMS: SidebarItem[] = [
  { icon: "⬡", label: "Integraciones" },
  { icon: "◉", label: "Auditoría",    view: "audit", badge: "1" },
  { icon: "⊕", label: "Marketplace" },
];

const ACCOUNT_ITEMS: SidebarItem[] = [
  { icon: "⚙", label: "Configuración" },
];

function Item({ item }: { item: SidebarItem }) {
  const { view, setView } = useAppStore();
  const isActive = item.view ? view === item.view && item.active : false;

  return (
    <button
      onClick={() => item.view && setView(item.view)}
      className={cn(
        "flex items-center gap-2.5 px-[18px] py-[9px] font-mono text-[12px] font-normal text-[#4a5c70] cursor-pointer transition-all duration-150 w-full text-left",
        isActive
          ? "text-[#e8f0f8] bg-[rgba(200,75,26,0.12)] border-l-2 border-l-[#c84b1a] !pl-[16px]"
          : "hover:text-[#c8d8e8] hover:bg-white/[0.03]"
      )}
    >
      <span>{item.icon}</span>
      <span>{item.label}</span>
      {item.badge && (
        <span className="ml-auto bg-[#c84b1a] text-white text-[9px] font-bold px-1.5 py-0.5 rounded-[2px] font-mono">
          {item.badge}
        </span>
      )}
    </button>
  );
}

function SectionLabel({ label }: { label: string }) {
  return (
    <span className="font-mono text-[9px] font-normal tracking-[0.5px] text-[#4a5c70]/50 px-[18px] pt-3.5 pb-1 block">
      {label}
    </span>
  );
}

export function Sidebar() {
  return (
    <aside className="w-[196px] shrink-0 bg-[#0d1219] flex flex-col border-r border-[rgba(255,255,255,0.07)]">
      {ITEMS.map((item) => <Item key={item.label} item={item} />)}
      <div className="h-px bg-[rgba(255,255,255,0.07)] my-1" />
      <SectionLabel label="Sistema" />
      {SYSTEM_ITEMS.map((item) => <Item key={item.label} item={item} />)}
      <div className="h-px bg-[rgba(255,255,255,0.07)] my-1" />
      <SectionLabel label="Cuenta" />
      {ACCOUNT_ITEMS.map((item) => <Item key={item.label} item={item} />)}
      <div className="mt-auto border-t border-[rgba(255,255,255,0.07)] px-[18px] py-3.5">
        <div className="font-mono text-[11px] text-[#4a5c70]">
          <strong className="block text-[#c8d8e8] text-[12px] mb-0.5">Ops Team</strong>
          Plan Team · 47K/100K
        </div>
      </div>
    </aside>
  );
}
