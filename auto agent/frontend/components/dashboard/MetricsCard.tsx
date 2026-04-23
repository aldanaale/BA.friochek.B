import { cn } from "@/lib/utils";
import { Metric } from "@/types";

export function MetricsCard({ metric }: { metric: Metric }) {
  return (
    <div className="px-7 py-[22px] border-r border-[rgba(255,255,255,0.07)] last:border-r-0 bg-[#1c2533]">
      <div className="font-mono text-[9px] font-normal tracking-[0.5px] text-[#4a5c70] mb-2.5 uppercase">
        {metric.label}
      </div>
      <div
        className={cn(
          "text-[34px] font-bold tracking-[-1px] leading-none",
          { "text-[#4ade80]": metric.variant === "ok", "text-[#f87171]": metric.variant === "err", "text-[#e8f0f8]": !metric.variant || metric.variant === "default" }
        )}
        style={{ fontFamily: "var(--font-roboto-mono), monospace" }}
      >
        {metric.value}
      </div>
      <div className="font-mono text-[11px] text-[#8a9ab0] mt-[5px]">{metric.sub}</div>
      {metric.barPct !== undefined && (
        <div className="h-0.5 bg-white/[0.08] mt-2.5 rounded-[1px] overflow-hidden">
          <div className="h-full bg-[#c84b1a] rounded-[1px]" style={{ width: `${metric.barPct}%` }} />
        </div>
      )}
      {metric.delta && (
        <div className={cn("font-mono text-[11px] mt-1", metric.deltaUp ? "text-[#4ade80]" : "text-[#f87171]")}>
          {metric.delta}
        </div>
      )}
    </div>
  );
}
