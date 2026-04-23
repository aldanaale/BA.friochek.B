"use client";

import { cn } from "@/lib/utils";

const DATA = [18,30,52,38,62,78,58,44,68,84,73,90,100,86,70,92,65,48,78,62,70,86,74,58];

export function ActivityChart() {
  return (
    <div>
      <div className="font-mono text-[11px] font-normal text-[#4a5c70] tracking-[0.3px]">
        // acciones / hora
      </div>
      <div className="font-mono text-[10px] text-[#4a5c70] mt-0.5">últimas 24h</div>
      <div className="h-[90px] flex items-end gap-[3px] mt-3.5">
        {DATA.map((h, i) => (
          <div
            key={i}
            className={cn(
              "flex-1 rounded-t-[1px] transition-opacity duration-200 hover:bg-[#e05a22]",
              h > 85 ? "bg-[#c84b1a]" : h > 60 ? "bg-white/25" : "bg-white/[0.12]"
            )}
            style={{ height: `${h}%` }}
          />
        ))}
      </div>
    </div>
  );
}
