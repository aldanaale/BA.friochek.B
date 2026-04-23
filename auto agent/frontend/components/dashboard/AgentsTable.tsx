"use client";

import { useAppStore } from "@/store/useAppStore";
import { cn } from "@/lib/utils";
import { Agent } from "@/types";

const AGENTS: Agent[] = [
  { id: "1", name: "Procesador de facturas RRHH", pipeline: "Gmail → Claude → SAP B1",          status: "active",  todayCount: 234,  lastActivity: "hace 2 min" },
  { id: "2", name: "Triaje de tickets soporte",   pipeline: "Zendesk → Claude → Slack",         status: "active",  todayCount: 891,  lastActivity: "hace 30 seg" },
  { id: "3", name: "Clasificador de leads CRM",   pipeline: "HubSpot → Claude → Salesforce",    status: "paused",  todayCount: 0,    lastActivity: "hace 2h" },
  { id: "4", name: "Generador de reportes finanzas", pipeline: "SAP → Claude → Google Sheets",  status: "error",   todayCount: 12,   lastActivity: "hace 45 min" },
];

const STATUS_BADGE: Record<Agent["status"], string> = {
  active: "bg-[rgba(74,222,128,0.1)] text-[#4ade80] border border-[rgba(74,222,128,0.2)]",
  paused: "bg-[rgba(251,191,36,0.1)] text-[#fbbf24] border border-[rgba(251,191,36,0.2)]",
  error:  "bg-[rgba(248,113,113,0.1)] text-[#f87171] border border-[rgba(248,113,113,0.2)]",
};
const STATUS_DOT: Record<Agent["status"], string> = {
  active: "bg-[#4ade80] [animation:liveblink_2s_infinite]",
  paused: "bg-[#fbbf24]",
  error:  "bg-[#f87171] [animation:liveblink_0.8s_infinite]",
};
const STATUS_LABEL: Record<Agent["status"], string> = {
  active: "Activo",
  paused: "Pausado",
  error:  "Error",
};

export function AgentsTable() {
  const setView = useAppStore((s) => s.setView);

  return (
    <table className="w-full border-collapse border border-[rgba(255,255,255,0.07)]">
      <thead>
        <tr>
          {["Agente", "Estado", "Hoy", "Actividad", "Acciones"].map((h) => (
            <th
              key={h}
              className="px-4 py-[9px] font-mono text-[9px] font-medium tracking-[0.5px] text-[#4a5c70] border-b border-[rgba(255,255,255,0.07)] text-left bg-[#0d1219]"
            >
              {h}
            </th>
          ))}
        </tr>
      </thead>
      <tbody>
        {AGENTS.map((agent) => (
          <tr key={agent.id} className="group">
            <td className="px-4 py-3.5 border-b border-white/[0.04] align-top bg-[#1c2533] group-hover:bg-[#253041] transition-colors">
              <div className="font-semibold text-[14px] text-[#e8f0f8]">{agent.name}</div>
              <div className="font-mono text-[10px] text-[#4a5c70] mt-0.5">{agent.pipeline}</div>
            </td>
            <td className="px-4 py-3.5 border-b border-white/[0.04] bg-[#1c2533] group-hover:bg-[#253041] transition-colors">
              <span className={cn("inline-flex items-center gap-[5px] px-[9px] py-[3px] font-mono text-[10px] font-medium rounded-[2px]", STATUS_BADGE[agent.status])}>
                <span className={cn("w-[5px] h-[5px] rounded-full", STATUS_DOT[agent.status])} />
                {STATUS_LABEL[agent.status]}
              </span>
            </td>
            <td className="px-4 py-3.5 border-b border-white/[0.04] bg-[#1c2533] group-hover:bg-[#253041] transition-colors">
              <span
                className={cn("font-mono text-[15px] font-medium text-[#e8f0f8]", {
                  "opacity-30": agent.status === "paused",
                  "text-[#f87171]": agent.status === "error",
                })}
              >
                {agent.status === "paused" ? "—" : agent.todayCount}
              </span>
            </td>
            <td className="px-4 py-3.5 border-b border-white/[0.04] bg-[#1c2533] group-hover:bg-[#253041] transition-colors">
              <span className="font-mono text-[11px] text-[#4a5c70]">{agent.lastActivity}</span>
            </td>
            <td className="px-4 py-3.5 border-b border-white/[0.04] bg-[#1c2533] group-hover:bg-[#253041] transition-colors">
              <button
                onClick={() => setView("audit")}
                className="px-3 py-[5px] bg-transparent border border-[rgba(255,255,255,0.07)] font-mono text-[10px] font-normal cursor-pointer rounded-[2px] text-[#8a9ab0] transition-all duration-150 mr-1.5 hover:border-white/30 hover:text-[#e8f0f8]"
              >
                {agent.status === "error" ? "Ver error" : "Ver logs"}
              </button>
              <button
                className={cn(
                  "px-3 py-[5px] bg-transparent border border-[rgba(255,255,255,0.07)] font-mono text-[10px] font-normal cursor-pointer rounded-[2px] text-[#8a9ab0] transition-all duration-150 hover:border-white/30 hover:text-[#e8f0f8]",
                  agent.status === "error" && "hover:!border-[#f87171] hover:!text-[#f87171]"
                )}
              >
                {agent.status === "active" ? "Pausar" : agent.status === "paused" ? "Reanudar" : "Rollback"}
              </button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
