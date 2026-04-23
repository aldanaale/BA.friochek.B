const PALETTE = [
  { cat: "Triggers",  nodes: [{ icon: "📨", label: "Email" }, { icon: "🪝", label: "Webhook" }, { icon: "⏰", label: "Scheduler" }] },
  { cat: "IA",        nodes: [{ icon: "🧠", label: "Llamar LLM" }, { icon: "🔎", label: "RAG search" }, { icon: "📝", label: "Generar texto" }] },
  { cat: "Lógica",    nodes: [{ icon: "⚡", label: "Condición" }, { icon: "🔄", label: "Loop" }, { icon: "👤", label: "Human-in-loop" }] },
  { cat: "Apps",      nodes: [{ icon: "💬", label: "Slack" }, { icon: "🏢", label: "SAP B1" }, { icon: "☁", label: "Salesforce" }, { icon: "📊", label: "Sheets" }, { icon: "✉", label: "Gmail" }] },
];

export function NodePalette() {
  return (
    <div className="w-[175px] shrink-0 bg-[#0d1219] border-r border-[rgba(255,255,255,0.07)] overflow-y-auto py-3.5">
      {PALETTE.map((section) => (
        <div key={section.cat}>
          <div className="font-mono text-[9px] font-normal text-[#4a5c70]/60 px-4 pt-3 pb-1.5 tracking-[0.3px] uppercase">
            {section.cat}
          </div>
          {section.nodes.map((node) => (
            <div
              key={node.label}
              draggable
              className="flex items-center gap-2 px-4 py-[7px] font-mono text-[11px] font-normal text-[#8a9ab0] cursor-grab transition-all duration-150 border-l-2 border-transparent hover:bg-white/[0.04] hover:text-[#e8f0f8] hover:border-l-[#c84b1a]"
            >
              <span className="text-[13px] w-4 text-center">{node.icon}</span>
              {node.label}
            </div>
          ))}
        </div>
      ))}
    </div>
  );
}
