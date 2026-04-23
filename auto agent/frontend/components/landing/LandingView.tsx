"use client";

import { useNavigationStore } from '@/store/navigation';
import { useAgentConsole } from '@/hooks/useAgentConsole';
import { useRef, useEffect } from 'react';

/* ─── Tiny sub-components ────────────────────────────────────── */

function ConsoleLine({ ts, type, msg }: { ts: string; type: string; msg: string }) {
  const colorClass =
    type === 'ok' ? 'text-[#4ade80]' :
      type === 'err' ? 'text-[#f87171]' :
        type === 'warn' ? 'text-[#fbbf24]' :
          type === 'ember' ? 'text-[#e05a22]' :
            'text-[#4a5c70]';
  return (
    <div className="flex gap-4">
      <span className="text-[#4a5c70] opacity-50 shrink-0">{ts}</span>
      <span className={colorClass}>{msg}</span>
    </div>
  );
}

/* ─── Landing Page ────────────────────────────────────────────── */

export function LandingView() {
  const { setActiveTab } = useNavigationStore();
  const lines = useAgentConsole(9);
  const consoleRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (consoleRef.current) {
      consoleRef.current.scrollTop = consoleRef.current.scrollHeight;
    }
  }, [lines]);

  return (
    <div>
      {/* ── Ticker ── */}
      <div className="bg-[#0d1219] overflow-hidden whitespace-nowrap border-b border-[#ffffff07] py-1.5">
        <span className="inline-block font-mono text-[11px] tracking-[1.5px] uppercase text-[#4a5c70] select-none animate-[ticker_30s_linear_infinite]">
          &nbsp;&nbsp;AGENTES EN PRODUCCIÓN · 47.832 ACCIONES HOY · TASA DE ÉXITO 98.3% · ROLLBACK AUTOMÁTICO · MCP GATEWAY · LANZADO 2026 · SALESFORCE · SAP B1 · HUBSPOT · SLACK · GMAIL · WORKDAY · ZENDESK ·&nbsp;&nbsp;AGENTES EN PRODUCCIÓN · 47.832 ACCIONES HOY · TASA DE ÉXITO 98.3% · ROLLBACK AUTOMÁTICO · MCP GATEWAY · LANZADO 2026 · SALESFORCE · SAP B1 · HUBSPOT · SLACK · GMAIL · WORKDAY · ZENDESK ·
        </span>
      </div>

      {/* ── Hero ── */}
      <div className="grid grid-cols-2 min-h-[calc(100vh-80px)] border-b border-[#ffffff07]">
        {/* Left */}
        <div className="pt-[72px] pb-16 pl-14 pr-10 border-r border-[#ffffff07] flex flex-col justify-between bg-[#141920] relative overflow-hidden">
          <div className="absolute top-[10%] left-[-10%] w-[600px] h-[600px] bg-[#c84b1a] opacity-[0.015] rounded-full blur-[80px] pointer-events-none" />

          <div className="relative z-10">
            <div className="font-mono text-[11px] tracking-[0.5px] text-[#e05a22] mb-8 flex items-center gap-2">
              <span className="text-[#4a5c70]">~/autoagent $</span>
              {' '}deploy --env prod
              <span className="inline-block w-2 h-[14px] bg-[#e05a22] animate-[blink_1s_step-end_infinite] align-middle ml-1" />
            </div>

            <h1 className="font-headline text-[clamp(36px,4.2vw,58px)] leading-[1.1] tracking-[-1px] font-bold text-[#e8f0f8]">
              <span className="text-[#8a9ab0] font-light">// </span>Agentes que<br />
              trabajan.<br />
              <span className="text-[#e05a22]">Sin código.</span>
            </h1>

            <p className="font-mono text-[13px] text-[#4a5c70] mt-5 leading-[1.7] font-light max-w-[420px]">
              <span className="text-[#2a7a4b]">/*</span>
              {' '}Tu equipo de operaciones construye,<br />
              despliega y audita agentes de IA sobre<br />
              sus propios sistemas. Sin depender de<br />
              ingeniería. Sin meses de espera. <span className="text-[#2a7a4b]">*/</span>
            </p>
          </div>

          <div className="flex gap-3 pt-12 relative z-10">
            <button
              onClick={() => setActiveTab('v-dash')}
              className="h-[44px] px-[26px] bg-[#c84b1a] text-white font-mono text-[13px] font-medium border-none cursor-pointer rounded-[2px] transition-colors hover:bg-[#e05a22] flex items-center gap-2"
            >
              $ empezar →
            </button>
            <button
              onClick={() => setActiveTab('v-build')}
              className="h-[44px] px-[22px] bg-transparent text-[#8a9ab0] border border-[#ffffff1a] font-mono text-[13px] cursor-pointer rounded-[2px] transition-all hover:border-[#ffffff33] hover:text-[#e8f0f8]"
            >
              ver constructor
            </button>
          </div>
        </div>

        {/* Right — Live Console */}
        <div className="bg-[#0d1219] relative overflow-hidden flex flex-col border-l border-[#ffffff07]">
          <div className="absolute top-[30%] right-[-15%] w-[800px] h-[800px] bg-[#c84b1a] opacity-[0.01] rounded-full blur-[100px] pointer-events-none" />

          {/* Console header */}
          <div className="flex items-center gap-2 py-3.5 px-5 border-b border-[#ffffff09] shrink-0">
            <div className="w-[9px] h-[9px] rounded-full bg-[#ef4444]" />
            <div className="w-[9px] h-[9px] rounded-full bg-[#f59e0b]" />
            <div className="w-[9px] h-[9px] rounded-full bg-[#22c55e]" />
            <span className="ml-1 font-mono text-[11px] text-[#4a5c70]">autoagent · runtime · prod-cluster-01</span>
            <span className="ml-auto flex items-center gap-[5px] font-mono text-[10px] text-[#e05a22]">
              <span className="w-[5px] h-[5px] rounded-full bg-[#e05a22] animate-[liveblink_1.2s_ease-in-out_infinite]" />
              LIVE
            </span>
          </div>

          {/* Console output */}
          <div
            ref={consoleRef}
            className="flex-1 px-5 py-6 overflow-hidden font-mono text-[11.5px] leading-[1.8] flex flex-col z-10"
          >
            {lines.map((line, i) => (
              <ConsoleLine key={i} {...line} />
            ))}
          </div>

          {/* Mini flow preview */}
          <div className="mx-5 mb-5 bg-[#ffffff05] border border-[#ffffff09] rounded-[3px] p-[18px] z-10">
            <div className="flex items-center gap-0">
              <div>
                <div className="h-[30px] px-3 border border-[#4ade8059] bg-[#4ade800f] rounded-[3px] flex items-center font-mono text-[10px] text-[#4ade80] whitespace-nowrap">
                  📨 Email
                </div>
                <div className="font-mono text-[9px] text-[#4a5c70] mt-1 text-center">trigger</div>
              </div>

              <div className="flex-1 h-px bg-[#ffffff1a] relative mx-1">
                <span className="absolute -right-[5px] -top-[9px] text-[#ffffff2e] text-[16px]">›</span>
              </div>

              <div>
                <div className="h-[30px] px-3 border border-[#4ade8059] bg-[#4ade800f] rounded-[3px] flex items-center font-mono text-[10px] text-[#4ade80] whitespace-nowrap">
                  🧠 Extraer
                </div>
                <div className="font-mono text-[9px] text-[#4a5c70] mt-1 text-center">claude</div>
              </div>

              <div className="flex-1 h-px bg-[#ffffff1a] relative mx-1">
                <span className="absolute -right-[5px] -top-[9px] text-[#ffffff2e] text-[16px]">›</span>
              </div>

              <div>
                <div className="h-[30px] px-3 border border-[#e05a2280] bg-[#c84b1a1f] rounded-[3px] flex items-center font-mono text-[10px] text-white whitespace-nowrap">
                  ⚡ Condición
                </div>
                <div className="font-mono text-[9px] text-[#4a5c70] mt-1 text-center">monto &gt;1M</div>
              </div>

              <div className="flex-1 h-px bg-[#ffffff1a] relative mx-1">
                <span className="absolute -right-[5px] -top-[9px] text-[#ffffff2e] text-[16px]">›</span>
              </div>

              <div>
                <div className="h-[30px] px-3 border border-[#ffffff17] bg-[#ffffff08] rounded-[3px] flex items-center font-mono text-[10px] text-[#8a9ab0] whitespace-nowrap">
                  💬 Slack+SAP
                </div>
                <div className="font-mono text-[9px] text-[#4a5c70] mt-1 text-center">acciones</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* ── Stats Strip ── */}
      <div className="grid grid-cols-4 border-t border-[#ffffff07] bg-[#1c2533]">
        {[
          { num: '400', sup: '+', label: 'empresas en producción' },
          { num: '98.3', sup: '%', label: 'tasa de éxito promedio' },
          { num: '47M', sup: '+', label: 'acciones este mes' },
          { num: '4.6', sup: '×', label: 'ROI promedio mes 3' },
        ].map((s, i) => (
          <div key={i} className={`py-8 px-10 ${i < 3 ? 'border-r border-[#ffffff07]' : ''}`}>
            <div className="font-headline text-[40px] font-bold tracking-[-1px] leading-none text-[#e8f0f8]">
              {s.num}<sup className="text-[20px] align-super text-[#e05a22]">{s.sup}</sup>
            </div>
            <div className="font-mono text-[12px] text-[#8a9ab0] mt-1.5 font-light">{s.label}</div>
          </div>
        ))}
      </div>

      {/* ── Features ── */}
      <div className="border-t border-[#ffffff07] bg-[#141920]">
        <div className="grid grid-cols-[240px_1fr] py-14 px-14 pb-10 border-b border-[#ffffff07]">
          <div className="font-mono text-[11px] text-[#e05a22] pt-1.5 tracking-[0.3px]">// funcionalidades</div>
          <h2 className="font-headline text-[34px] leading-[1.15] font-bold text-[#e8f0f8] tracking-[-0.5px]">
            Lo que falta en<br />
            <em className="not-italic text-[#e05a22]">las otras plataformas</em>
          </h2>
        </div>

        <div className="grid grid-cols-2 border-t border-[#ffffff07]">
          {[
            {
              tag: 'Constructor',
              title: 'Flujos visuales sin código',
              body: 'Arrastra nodos, conecta sistemas y define condiciones. Tu equipo de operaciones lo construye sin tickets a ingeniería.',
            },
            {
              tag: 'Auditoría',
              title: 'Cada decisión explicada',
              body: 'Cada paso logueado con input, output y razonamiento. Cuando algo falla, sabes exactamente dónde, cuándo y por qué.',
            },
            {
              tag: 'Resiliencia',
              title: 'Rollback automático',
              body: 'Error en producción. Un clic. El agente vuelve al último estado estable sin pérdida de datos ni llamada a ingeniería.',
            },
            {
              tag: 'Integraciones',
              title: 'Tu stack, conectado',
              body: 'Salesforce, SAP, HubSpot, Workday, Slack, Gmail y más de 50 conectores via MCP. Listos en minutos, no meses.',
            },
          ].map((f, i) => (
            <div
              key={i}
              className={`py-11 px-13 transition-colors hover:bg-[#ffffff04]
                ${i % 2 === 0 ? 'border-r border-[#ffffff07]' : ''}
                ${i < 2 ? 'border-b border-[#ffffff07]' : ''}
              `}
            >
              <div className="font-mono text-[10px] text-[#4a5c70] mb-3.5 tracking-[0.3px]">{f.tag}</div>
              <div className="font-headline text-[22px] leading-[1.25] mb-3 font-semibold text-[#e8f0f8] tracking-[-0.3px]">{f.title}</div>
              <p className="text-[14px] text-[#8a9ab0] leading-[1.7] font-light">{f.body}</p>
            </div>
          ))}
        </div>
      </div>

      {/* ── Pricing ── */}
      <div className="border-t border-[#ffffff07] py-20 px-14 bg-[#0d1219] text-[#c8d8e8]">
        <div className="font-mono text-[11px] text-[#e05a22] mb-3.5 tracking-[0.3px]">// precios</div>
        <h2 className="font-headline text-[44px] leading-[1.05] font-bold mb-[52px] text-[#e8f0f8] tracking-[-1px]">
          Simple.<br />Sin sorpresas.
        </h2>

        <div className="grid grid-cols-3 border border-[#ffffff14]">
          {/* Builder */}
          <div className="p-10 border-r border-[#ffffff14] flex flex-col bg-[#1c2533]">
            <div className="font-mono text-[11px] tracking-[0.5px] text-[#4a5c70] mb-[18px]">Builder</div>
            <div className="font-headline text-[48px] font-bold tracking-[-1.5px] leading-none text-[#e8f0f8]">
              $500<span className="text-[16px] font-light opacity-60 font-mono">/mes</span>
            </div>
            <div className="font-mono text-[13px] text-[#4a5c70] mt-2.5 mb-6 font-light">Para empezar con agentes reales</div>
            <hr className="border-none border-t border-[#ffffff1a] mb-5" />
            {['3 agentes activos', '10.000 acciones al mes', '5 integraciones', 'Auditoría básica'].map((f) => (
              <div key={f} className="font-mono text-[13px] text-[#8a9ab0] py-[5px] flex gap-2 font-light">
                <span className="text-[#e05a22] shrink-0">//</span>{f}
              </div>
            ))}
            <div className="mt-auto pt-7">
              <button
                onClick={() => setActiveTab('v-dash')}
                className="w-full py-3 font-mono text-[12px] font-medium cursor-pointer rounded-[2px] transition-all bg-transparent border border-[#ffffff2e] text-[#8a9ab0] hover:border-[#ffffff66] hover:text-white"
              >
                Comenzar gratis
              </button>
            </div>
          </div>

          {/* Team — featured */}
          <div className="p-10 border-r border-[#ffffff14] flex flex-col bg-[#c84b1a]">
            <div className="font-mono text-[11px] tracking-[0.5px] text-[#ffffff99] mb-[18px]">Team</div>
            <div className="font-headline text-[48px] font-bold tracking-[-1.5px] leading-none text-white">
              $2.000<span className="text-[16px] font-light opacity-60 font-mono">/mes</span>
            </div>
            <div className="font-mono text-[13px] text-[#ffffffbf] mt-2.5 mb-6 font-light">Para equipos de operaciones</div>
            <hr className="border-none border-t border-[#ffffff1a] mb-5" />
            {['15 agentes activos', '100.000 acciones al mes', 'Integraciones ilimitadas', 'Auditoría completa + rollback', 'Soporte prioritario'].map((f) => (
              <div key={f} className="font-mono text-[13px] text-[#ffffffd9] py-[5px] flex gap-2 font-light">
                <span className="text-[#ffffff99] shrink-0">//</span>{f}
              </div>
            ))}
            <div className="mt-auto pt-7">
              <button
                onClick={() => setActiveTab('v-dash')}
                className="w-full py-3 font-mono text-[12px] font-semibold cursor-pointer rounded-[2px] transition-colors bg-white text-[#c84b1a] hover:bg-[#f0f0f0] border-none"
              >
                Empezar ahora
              </button>
            </div>
          </div>

          {/* Enterprise */}
          <div className="p-10 flex flex-col bg-[#1c2533]">
            <div className="font-mono text-[11px] tracking-[0.5px] text-[#4a5c70] mb-[18px]">Enterprise</div>
            <div className="font-headline text-[48px] font-bold tracking-[-1.5px] leading-none text-[#e8f0f8]">
              $10k+<span className="text-[16px] font-light opacity-60 font-mono">/mes</span>
            </div>
            <div className="font-mono text-[13px] text-[#4a5c70] mt-2.5 mb-6 font-light">Para grandes organizaciones</div>
            <hr className="border-none border-t border-[#ffffff1a] mb-5" />
            {['Agentes ilimitados', 'On-premise disponible', 'SLA + RBAC + SSO', 'SOC 2 Type II'].map((f) => (
              <div key={f} className="font-mono text-[13px] text-[#8a9ab0] py-[5px] flex gap-2 font-light">
                <span className="text-[#e05a22] shrink-0">//</span>{f}
              </div>
            ))}
            <div className="mt-auto pt-7">
              <button className="w-full py-3 font-mono text-[12px] font-medium cursor-pointer rounded-[2px] transition-all bg-transparent border border-[#ffffff2e] text-[#8a9ab0] hover:border-[#ffffff66] hover:text-white">
                Hablar con ventas
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
