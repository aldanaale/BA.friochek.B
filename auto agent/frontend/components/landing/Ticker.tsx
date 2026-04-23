const TEXT =
  "\u00a0\u00a0AGENTES EN PRODUCCIÓN · 47.832 ACCIONES HOY · TASA DE ÉXITO 98.3% · ROLLBACK AUTOMÁTICO · MCP GATEWAY · LANZADO 2026 · SALESFORCE · SAP B1 · HUBSPOT · SLACK · GMAIL · WORKDAY · ZENDESK ·";

export function Ticker() {
  const doubled = TEXT + TEXT;
  return (
    <div className="bg-[#0d1219] text-[#4a5c70] font-mono text-[11px] tracking-[0.5px] py-1.5 overflow-hidden whitespace-nowrap border-b border-white/[0.05]">
      <span
        className="inline-block"
        style={{ animation: "ticker 30s linear infinite" }}
      >
        {doubled}
      </span>
    </div>
  );
}
