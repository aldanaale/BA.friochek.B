"use client";

import { useEffect, useRef, useState } from "react";
import { ConsoleEntry } from "@/types";

const SEED_LINES: ConsoleEntry[] = [
  { ts: "10:34:02", type: "ok",    msg: "agent:facturas-rrhh · trigger fired · gmail" },
  { ts: "10:34:02", type: "dim",   msg: "fetching message id:msg-4821 · 12kb" },
  { ts: "10:34:03", type: "ok",    msg: "llm:claude-sonnet · extracting structured data" },
  { ts: "10:34:04", type: "ok",    msg: "extracted · rut:76.123.456-7 · monto:2450000" },
  { ts: "10:34:04", type: "ember", msg: "condition:monto > 1000000 → true" },
  { ts: "10:34:05", type: "ok",    msg: "slack · sent → #aprobaciones-grandes" },
  { ts: "10:34:06", type: "ok",    msg: "sap-b1 · docnum:4821 created · pending" },
  { ts: "10:34:06", type: "ok",    msg: "execution complete · 2.4s · $0.003" },
  { ts: "10:34:10", type: "dim",   msg: "agent:reportes-finanzas · scheduler 10:00" },
  { ts: "10:34:12", type: "ok",    msg: "sap-b1 · 847 transactions · $45.231.000" },
  { ts: "10:34:42", type: "err",   msg: "llm:claude-sonnet · TIMEOUT 30s · 12847 tok" },
  { ts: "10:34:42", type: "warn",  msg: "rollback available · state saved" },
];

export function useAgentConsole(maxLines = 9) {
  const [lines, setLines] = useState<ConsoleEntry[]>([]);
  const idxRef = useRef(0);
  const timerRef = useRef<ReturnType<typeof setTimeout>>();

  useEffect(() => {
    function tick() {
      const entry = SEED_LINES[idxRef.current % SEED_LINES.length];
      idxRef.current++;
      setLines((prev) => {
        const next = [...prev, entry];
        return next.length > maxLines ? next.slice(next.length - maxLines) : next;
      });
      timerRef.current = setTimeout(tick, 650 + Math.random() * 800);
    }
    timerRef.current = setTimeout(tick, 300);
    return () => clearTimeout(timerRef.current);
  }, [maxLines]);

  return lines;
}
