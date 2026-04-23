"use client";

import { useAgentConsole } from "@/hooks/useAgentConsole";
import { cn } from "@/lib/utils";

const TYPE_CLASS: Record<string, string> = {
  ok:    "text-[#4ade80]",
  warn:  "text-[#fbbf24]",
  err:   "text-[#f87171]",
  dim:   "text-white/30",
  ember: "text-[#e05a22]",
};

export function LiveConsole() {
  const lines = useAgentConsole(9);

  return (
    <div className="flex-1 px-5 py-6 overflow-hidden font-mono text-[11.5px] leading-[1.8]">
      {lines.map((l, i) => (
        <div key={i} className="flex gap-3">
          <span className="text-white/[0.18] shrink-0">{l.ts}</span>
          <span className={cn(TYPE_CLASS[l.type] ?? "text-white/50")}>{l.msg}</span>
        </div>
      ))}
    </div>
  );
}
