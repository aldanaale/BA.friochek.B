"use client";

import { useAppStore } from "@/store/useAppStore";
import { Alert } from "@/types";

const ALERTS: Alert[] = [
  { id: "1", level: "error",   message: "Agente finanzas — error paso 3/7", meta: "Timeout SAP B1", gotoAudit: true },
  { id: "2", level: "warning", message: "Uso al 47% — ritmo acelerado",     meta: "Proyección: límite en 18 días · hace 2h" },
  { id: "3", level: "success", message: "Agente RRHH en producción",         meta: "Primera ejecución exitosa · hace 6h" },
];

const LEVEL_ICON: Record<Alert["level"], string>  = { error: "!", warning: "▲", success: "✓" };
const LEVEL_COLOR: Record<Alert["level"], string> = { error: "#f87171", warning: "#fbbf24", success: "#4ade80" };

export function AlertsPanel() {
  const setView = useAppStore((s) => s.setView);

  return (
    <div>
      {ALERTS.map((alert) => (
        <div key={alert.id} className="flex gap-3.5 py-3 border-b border-white/[0.04] last:border-b-0">
          <div
            className="font-mono text-[13px] shrink-0 w-4"
            style={{ color: LEVEL_COLOR[alert.level] }}
          >
            {LEVEL_ICON[alert.level]}
          </div>
          <div>
            <div
              className="text-[13px] font-medium text-[#e8f0f8]"
              style={alert.level === "error" ? { color: "#f87171" } : undefined}
            >
              {alert.message}
            </div>
            <div className="font-mono text-[10px] text-[#4a5c70] mt-0.5">
              {alert.meta}
              {alert.gotoAudit && (
                <>
                  {" · "}
                  <button
                    onClick={() => setView("audit")}
                    className="text-[#e05a22] underline underline-offset-2 cursor-pointer bg-transparent border-none p-0 font-mono text-[10px]"
                  >
                    Ver auditoría →
                  </button>
                </>
              )}
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
