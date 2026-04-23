"use client";

const MONO = "var(--font-ibm-plex-mono), monospace";
const HEAD = "var(--font-roboto-mono), monospace";

interface Step {
  n: number;
  ok: boolean;
  title: string;
  detail?: string;
  io?: { tag: string; content: string; isErr?: boolean }[];
  dur: string;
  rollback?: boolean;
}

function StepCircle({ n, ok }: { n: number; ok: boolean }) {
  return (
    <div style={{
      width: '22px', height: '22px', borderRadius: '50%',
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      fontFamily: MONO, fontSize: '10px', fontWeight: 700,
      background: ok ? 'rgba(74,222,128,.1)' : 'rgba(248,113,113,.1)',
      color: ok ? '#4ade80' : '#f87171',
      border: ok ? '1px solid rgba(74,222,128,.2)' : '1px solid rgba(248,113,113,.2)',
    }}>
      {n}
    </div>
  );
}

function ExecStep({ step }: { step: Step }) {
  return (
    <div style={{ display: 'flex', gap: 0, borderBottom: '1px solid rgba(255,255,255,.04)', transition: 'background .15s' }}
      onMouseEnter={e => (e.currentTarget.style.background = 'rgba(255,255,255,.02)')}
      onMouseLeave={e => (e.currentTarget.style.background = 'transparent')}
    >
      <div style={{ width: '48px', flexShrink: 0, display: 'flex', alignItems: 'flex-start', justifyContent: 'center', paddingTop: '15px', borderRight: '1px solid rgba(255,255,255,.04)' }}>
        <StepCircle n={step.n} ok={step.ok} />
      </div>
      <div style={{ flex: 1, padding: '13px 20px', background: '#1c2533' }}>
        <div style={{ fontFamily: MONO, fontSize: '12px', fontWeight: 600, color: step.ok ? '#e8f0f8' : '#f87171' }}>{step.title}</div>
        {step.detail && (
          <div style={{ fontFamily: MONO, fontSize: '10px', color: step.ok ? '#4a5c70' : '#b52b2b', marginTop: '3px' }}>{step.detail}</div>
        )}
        {step.io?.map((io, i) => (
          <div key={i} style={{ background: '#141920', border: '1px solid rgba(255,255,255,.07)', borderRadius: '2px', padding: '9px 12px', marginTop: '8px', fontFamily: MONO, fontSize: '10px', color: '#8a9ab0', lineHeight: 1.7 }}>
            <div style={{ fontFamily: MONO, fontSize: '9px', color: io.isErr ? '#f87171' : '#4a5c70', letterSpacing: '.3px', marginBottom: '4px', fontWeight: 500 }}>{io.tag}</div>
            <span style={{ color: io.isErr ? '#f87171' : undefined }}>{io.content}</span>
          </div>
        ))}
        <div style={{ fontFamily: MONO, fontSize: '10px', color: step.ok ? '#4a5c70' : '#f87171', marginTop: '6px' }}>⏱ {step.dur}</div>
        {step.rollback && (
          <button
            style={{
              display: 'inline-flex', alignItems: 'center', gap: '6px', marginTop: '10px',
              padding: '6px 14px', background: 'rgba(248,113,113,.08)', border: '1px solid rgba(248,113,113,.2)',
              color: '#f87171', fontFamily: MONO, fontSize: '10px', fontWeight: 500,
              cursor: 'pointer', borderRadius: '2px', transition: 'all .15s',
            }}
            onMouseEnter={e => (e.currentTarget.style.background = 'rgba(248,113,113,.15)')}
            onMouseLeave={e => (e.currentTarget.style.background = 'rgba(248,113,113,.08)')}
          >
            ⏪ Rollback al estado anterior
          </button>
        )}
      </div>
    </div>
  );
}

const exec1Steps: Step[] = [
  { n: 1, ok: true, title: 'Scheduler — ejecución diaria 10:00', detail: 'cron: 0 10 * * * · disparado puntual', dur: '12ms' },
  { n: 2, ok: true, title: 'SAP B1 — extraer transacciones del día', detail: 'endpoint: /api/BusinessPartners/transactions', io: [{ tag: 'Output', content: '{"count": 847, "total": 45231000, "currency": "CLP"}' }], dur: '2.1s' },
  { n: 3, ok: false, title: 'Claude Sonnet — generar análisis de transacciones', detail: 'Timeout después de 30s — respuesta incompleta', io: [{ tag: 'Input enviado', content: 'Analiza 847 transacciones. Total $45.231.000 CLP. Identifica anomalías...' }, { tag: 'Error', content: 'TIMEOUT: no response within 30s · tokens: 12.847/45.000', isErr: true }], dur: '30.0s (timeout)', rollback: true },
];

const exec2Steps: Step[] = [
  { n: 1, ok: true, title: 'Email recibido — facturas@empresa.com', detail: 'De: proveedor@acme.cl · Asunto: Factura N°4821', dur: '8ms' },
  { n: 2, ok: true, title: 'Claude Sonnet — extracción de datos', io: [{ tag: 'Output', content: '{"rut": "76.123.456-7", "monto": 2450000, "fecha": "2026-04-17", "numero": "4821"}' }], dur: '1.2s · 312 tokens · $0.003' },
  { n: 3, ok: true, title: 'Condición — monto $2.450.000 > $1.000.000 → Sí', detail: 'Ruta: notificar Slack + registrar SAP', dur: '1ms' },
  { n: 4, ok: true, title: 'Slack → #aprobaciones-grandes', dur: '220ms' },
  { n: 5, ok: true, title: 'SAP B1 — factura N°4821 registrada', detail: 'Proveedor: ACME SpA · Estado: Pendiente aprobación', dur: '890ms · Total: 2.4s' },
];

export function AuditView() {
  return (
    <div style={{ background: '#141920', paddingBottom: '60px', minHeight: 'calc(100vh - 48px)' }}>

      {/* Hero */}
      <div style={{ padding: '40px 48px', borderBottom: '1px solid rgba(255,255,255,.07)', display: 'flex', alignItems: 'flex-end', justifyContent: 'space-between', background: '#0d1219' }}>
        <div>
          <div style={{ fontFamily: HEAD, fontSize: '38px', fontWeight: 700, lineHeight: 1.05, color: '#e8f0f8', letterSpacing: '-1px' }}>
            audit.<em style={{ fontStyle: 'normal', color: '#e05a22' }}>log</em>()
          </div>
          <div style={{ fontFamily: MONO, fontSize: '12px', color: '#8a9ab0', marginTop: '10px', fontWeight: 300, maxWidth: '340px', lineHeight: 1.7 }}>
            // cada decisión del agente trazada. inputs, outputs, tiempos, costos. rollback con un clic.
          </div>
        </div>
        <div style={{ display: 'flex', gap: '8px' }}>
          <select style={{ padding: '8px 14px', background: '#1c2533', border: '1px solid rgba(255,255,255,.07)', color: '#c8d8e8', fontFamily: MONO, fontSize: '12px', borderRadius: '2px', cursor: 'pointer', outline: 'none' }}>
            <option>Todos los agentes</option>
            <option>Procesador facturas RRHH</option>
            <option>Reportes finanzas</option>
          </select>
          <button
            style={{ padding: '8px 18px', background: '#c84b1a', border: 'none', color: '#fff', fontFamily: MONO, fontSize: '11px', fontWeight: 500, cursor: 'pointer', borderRadius: '2px', transition: 'background .15s' }}
            onMouseEnter={e => (e.currentTarget.style.background = '#e05a22')}
            onMouseLeave={e => (e.currentTarget.style.background = '#c84b1a')}
          >
            Exportar CSV
          </button>
        </div>
      </div>

      {/* List */}
      <div style={{ maxWidth: '900px', margin: '0 auto', padding: '0 48px' }}>

        {/* Exec block 1 — Error */}
        <div style={{ border: '1px solid rgba(255,255,255,.07)', marginTop: '28px', borderRadius: '2px', overflow: 'hidden' }}>
          <div
            style={{ display: 'flex', alignItems: 'center', gap: '14px', padding: '13px 20px', background: '#1c2533', borderBottom: '1px solid rgba(255,255,255,.07)', cursor: 'pointer', transition: 'background .15s' }}
            onMouseEnter={e => (e.currentTarget.style.background = '#253041')}
            onMouseLeave={e => (e.currentTarget.style.background = '#1c2533')}
          >
            <span style={{ fontFamily: MONO, fontSize: '11px', color: '#e05a22', fontWeight: 500 }}>#exec-9821</span>
            <div style={{ flex: 1 }}>
              <div style={{ fontFamily: MONO, fontSize: '13px', fontWeight: 600, color: '#e8f0f8' }}>Generador de reportes finanzas</div>
              <div style={{ fontFamily: MONO, fontSize: '10px', color: '#4a5c70', marginTop: '2px' }}>SAP → Claude Sonnet → Google Sheets</div>
            </div>
            <span style={{ display: 'inline-flex', alignItems: 'center', gap: '5px', padding: '3px 9px', fontFamily: MONO, fontSize: '10px', fontWeight: 500, borderRadius: '2px', background: 'rgba(248,113,113,.1)', color: '#f87171', border: '1px solid rgba(248,113,113,.2)' }}>
              <span style={{ width: '5px', height: '5px', borderRadius: '50%', background: '#f87171', display: 'inline-block', animation: 'liveblink .8s infinite' }} />Error paso 3
            </span>
            <span style={{ fontFamily: MONO, fontSize: '11px', color: '#4a5c70' }}>hoy 10:22 · hace 45 min</span>
          </div>
          {exec1Steps.map(s => <ExecStep key={s.n} step={s} />)}
        </div>

        {/* Exec block 2 — Success */}
        <div style={{ border: '1px solid rgba(255,255,255,.07)', marginTop: '28px', borderRadius: '2px', overflow: 'hidden' }}>
          <div
            style={{ display: 'flex', alignItems: 'center', gap: '14px', padding: '13px 20px', background: '#1c2533', borderBottom: '1px solid rgba(255,255,255,.07)', cursor: 'pointer', transition: 'background .15s' }}
            onMouseEnter={e => (e.currentTarget.style.background = '#253041')}
            onMouseLeave={e => (e.currentTarget.style.background = '#1c2533')}
          >
            <span style={{ fontFamily: MONO, fontSize: '11px', color: '#e05a22', fontWeight: 500 }}>#exec-9820</span>
            <div style={{ flex: 1 }}>
              <div style={{ fontFamily: MONO, fontSize: '13px', fontWeight: 600, color: '#e8f0f8' }}>Procesador de facturas RRHH</div>
              <div style={{ fontFamily: MONO, fontSize: '10px', color: '#4a5c70', marginTop: '2px' }}>Gmail → Claude Sonnet → SAP B1 + Slack</div>
            </div>
            <span style={{ display: 'inline-flex', alignItems: 'center', gap: '5px', padding: '3px 9px', fontFamily: MONO, fontSize: '10px', fontWeight: 500, borderRadius: '2px', background: 'rgba(74,222,128,.1)', color: '#4ade80', border: '1px solid rgba(74,222,128,.2)' }}>
              <span style={{ width: '5px', height: '5px', borderRadius: '50%', background: '#4ade80', display: 'inline-block', animation: 'liveblink 2s infinite' }} />Completado
            </span>
            <span style={{ fontFamily: MONO, fontSize: '11px', color: '#4a5c70' }}>hoy 10:18 · hace 49 min</span>
          </div>
          {exec2Steps.map(s => <ExecStep key={s.n} step={s} />)}
        </div>

      </div>
    </div>
  );
}
