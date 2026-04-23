const STATS = [
  { num: "400", sup: "+", label: "empresas en producción" },
  { num: "98.3", sup: "%", label: "tasa de éxito promedio" },
  { num: "47M",  sup: "+", label: "acciones este mes" },
  { num: "4.6",  sup: "×", label: "ROI promedio mes 3" },
];

export function StatsStrip() {
  return (
    <div className="grid grid-cols-4 border-t border-[rgba(255,255,255,0.07)] bg-[#1c2533]">
      {STATS.map((s, i) => (
        <div
          key={i}
          className="px-10 py-8 border-r border-[rgba(255,255,255,0.07)] last:border-r-0"
        >
          <div
            className="text-[40px] font-bold tracking-[-1px] leading-none text-[#e8f0f8]"
            style={{ fontFamily: "var(--font-roboto-mono), monospace" }}
          >
            {s.num}
            <sup className="text-[20px] align-super text-[#e05a22]">{s.sup}</sup>
          </div>
          <div className="text-[12px] text-[#8a9ab0] mt-1.5 font-mono font-light">{s.label}</div>
        </div>
      ))}
    </div>
  );
}
