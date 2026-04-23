interface FlowNode {
  label: string;
  sub: string;
  variant: "done" | "active" | "idle";
}

const NODES: FlowNode[] = [
  { label: "📨 Email",     sub: "trigger",  variant: "done" },
  { label: "🧠 Extraer",   sub: "claude",   variant: "done" },
  { label: "⚡ Condición", sub: "monto >1M", variant: "active" },
  { label: "💬 Slack+SAP", sub: "acciones", variant: "idle" },
];

const NODE_CLASS: Record<FlowNode["variant"], string> = {
  done:   "border-[rgba(74,222,128,0.35)] text-[#4ade80] bg-[rgba(74,222,128,0.06)]",
  active: "border-[rgba(224,90,34,0.5)] text-white bg-[rgba(200,75,26,0.12)]",
  idle:   "border-white/[0.09] text-white/50 bg-white/[0.05]",
};

export function FlowMini() {
  return (
    <div className="mx-5 mb-5 bg-white/[0.02] border border-white/[0.06] rounded-[3px] p-[18px]">
      <div className="flex items-center">
        {NODES.map((node, i) => (
          <div key={i} className="flex items-center flex-1 min-w-0">
            <div className="shrink-0">
              <div className={`border rounded-[3px] px-[11px] py-[7px] font-mono text-[10px] whitespace-nowrap ${NODE_CLASS[node.variant]}`}>
                {node.label}
              </div>
              <div className="text-[9px] text-white/[0.18] mt-1 font-mono text-center">{node.sub}</div>
            </div>
            {i < NODES.length - 1 && (
              <div className="flex-1 h-px bg-white/10 relative mx-1">
                <span className="absolute -right-1 -top-[9px] text-white/[0.18] text-base">›</span>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
