"use client";

import { useAppStore } from "@/store/useAppStore";
import { LiveConsole } from "./LiveConsole";
import { FlowMini } from "./FlowMini";

export function Hero() {
  const setView = useAppStore((s) => s.setView);

  return (
    <div className="grid grid-cols-2 min-h-[calc(100vh-80px)] border-b border-[rgba(255,255,255,0.07)]">
      {/* Left */}
      <div className="px-14 py-[72px] border-r border-[rgba(255,255,255,0.07)] flex flex-col justify-between bg-[#141920]">
        <div>
          <div className="font-mono text-[11px] font-normal tracking-[0.5px] text-[#e05a22] mb-8 flex items-center gap-2.5">
            <span className="text-[#4a5c70]">~/autoagent $</span>
            {" "}deploy --env prod
            <span
              className="inline-block w-2 h-[14px] bg-[#e05a22] ml-0.5 align-middle"
              style={{ animation: "blink 1s step-end infinite" }}
            />
          </div>
          <h1
            className="text-[clamp(36px,4.2vw,58px)] leading-[1.1] tracking-[-1px] font-bold text-[#e8f0f8]"
            style={{ fontFamily: "var(--font-roboto-mono), monospace" }}
          >
            <span className="text-[#8a9ab0] font-light">// </span>Agentes que<br />
            trabajan.<br />
            <span className="text-[#e05a22]">Sin código.</span>
          </h1>
          <p className="font-mono text-[13px] text-[#4a5c70] mt-5 leading-[1.7] font-light max-w-[420px]">
            <span className="text-[#4a7a4a]">{"/*"}</span>{" "}Tu equipo de operaciones construye,<br />
            despliega y audita agentes de IA sobre<br />
            sus propios sistemas. Sin depender de<br />
            ingeniería. Sin meses de espera.{" "}
            <span className="text-[#4a7a4a]">{"*/"}</span>
          </p>
        </div>
        <div className="flex gap-3 mt-auto pt-12">
          <button
            onClick={() => setView("dashboard")}
            className="px-[26px] py-3 bg-[#c84b1a] text-white font-mono text-[13px] font-medium border-none cursor-pointer rounded-[2px] transition-all duration-150 flex items-center gap-2 hover:bg-[#e05a22]"
          >
            $ empezar →
          </button>
          <button
            onClick={() => setView("builder")}
            className="px-[22px] py-3 bg-transparent text-[#8a9ab0] font-mono text-[13px] font-normal border border-[rgba(255,255,255,0.07)] cursor-pointer rounded-[2px] transition-all duration-150 hover:border-white/20 hover:text-[#e8f0f8]"
          >
            ver constructor
          </button>
        </div>
      </div>

      {/* Right — live runtime panel */}
      <div className="bg-[#0d1219] relative overflow-hidden flex flex-col border-l border-[rgba(255,255,255,0.07)]">
        <div className="flex items-center gap-2 px-5 py-3.5 border-b border-white/[0.06]">
          <span className="w-[9px] h-[9px] rounded-full bg-[#ef4444]" />
          <span className="w-[9px] h-[9px] rounded-full bg-[#f59e0b]" />
          <span className="w-[9px] h-[9px] rounded-full bg-[#22c55e]" />
          <span className="ml-1 font-mono text-[11px] text-white/30">autoagent · runtime · prod-cluster-01</span>
          <span className="ml-auto flex items-center gap-[5px] font-mono text-[10px] text-[#e05a22]">
            <span
              className="w-[5px] h-[5px] rounded-full bg-[#e05a22]"
              style={{ animation: "liveblink 1.2s ease-in-out infinite" }}
            />
            LIVE
          </span>
        </div>
        <LiveConsole />
        <FlowMini />
      </div>
    </div>
  );
}
