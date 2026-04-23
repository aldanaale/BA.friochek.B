"use client";

import { useNavigationStore } from '@/store/navigation';

const COND = "var(--font-barlow-condensed), sans-serif";
const MONO = "var(--font-ibm-plex-mono), monospace";
const HEAD = "var(--font-roboto-mono), monospace";

const chBars = [18,30,52,38,62,78,58,44,68,84,73,90,100,86,70,92,65,48,78,62,70,86,74,58];

type BadgeKind = 'ok' | 'warn' | 'err';

function Badge({ kind, label }: { kind: BadgeKind; label: string }) {
  const styles: Record<BadgeKind, React.CSSProperties> = {
    ok:   { background: 'rgba(74,222,128,.1)',  color: '#4ade80', border: '1px solid rgba(74,222,128,.2)' },
    warn: { background: 'rgba(251,191,36,.1)',  color: '#fbbf24', border: '1px solid rgba(251,191,36,.2)' },
    err:  { background: 'rgba(248,113,113,.1)', color: '#f87171', border: '1px solid rgba(248,113,113,.2)' },
  };
  const dotAnim: Record<BadgeKind, string> = {
    ok:   'liveblink 2s infinite',
    warn: 'none',
    err:  'liveblink .8s infinite',
  };
  const dotColor: Record<BadgeKind, string> = {
    ok: '#4ade80', warn: '#fbbf24', err: '#f87171',
  };
  return (
    <span style={{ display: 'inline-flex', alignItems: 'center', gap: '5px', padding: '3px 9px', fontFamily: MONO, fontSize: '10px', fontWeight: 500, borderRadius: '2px', ...styles[kind] }}>
      <span style={{ width: '5px', height: '5px', borderRadius: '50%', background: dotColor[kind], display: 'inline-block', animation: dotAnim[kind] }} />
      {label}
    </span>
  );
}

const SB_ITEM_BASE: React.CSSProperties = {
  display: 'flex', alignItems: 'center', gap: '10px',
  padding: '9px 18px', fontFamily: MONO, fontSize: '12px',
  fontWeight: 400, color: '#4a5c70', cursor: 'pointer', transition: 'all .15s',
};

export function DashboardView() {
  const { setActiveTab } = useNavigationStore();

  return (
    <div style={{ display: 'flex', height: 'calc(100vh - 48px)', background: '#141920' }}>

      {/* ── SIDEBAR ── */}
      <div style={{ width: '196px', flexShrink: 0, background: '#0d1219', display: 'flex', flexDirection: 'column', borderRight: '1px solid rgba(255,255,255,.07)' }}>

        {/* Active: Dashboard */}
        <div style={{ ...SB_ITEM_BASE, color: '#e8f0f8', background: 'rgba(200,75,26,.12)', borderLeft: '2px solid #c84b1a', paddingLeft: '16px' }}>
          <span>⊞</span> Dashboard
        </div>

        <div onClick={() => setActiveTab('v-build')} style={SB_ITEM_BASE}
          onMouseEnter={e => { (e.currentTarget as HTMLElement).style.color = '#c8d8e8'; (e.currentTarget as HTMLElement).style.background = 'rgba(255,255,255,.03)'; }}
          onMouseLeave={e => { (e.currentTarget as HTMLElement).style.color = '#4a5c70'; (e.currentTarget as HTMLElement).style.background = 'transparent'; }}>
          <span>⬡</span> Mis agentes
        </div>

        <div onClick={() => setActiveTab('v-build')} style={SB_ITEM_BASE}
          onMouseEnter={e => { (e.currentTarget as HTMLElement).style.color = '#c8d8e8'; (e.currentTarget as HTMLElement).style.background = 'rgba(255,255,255,.03)'; }}
          onMouseLeave={e => { (e.currentTarget as HTMLElement).style.color = '#4a5c70'; (e.currentTarget as HTMLElement).style.background = 'transparent'; }}>
          <span>✦</span> Constructor
        </div>

        <div style={{ height: '1px', background: 'rgba(255,255,255,.07)', margin: '4px 0' }} />

        <div style={{ fontFamily: MONO, fontSize: '9px', color: '#4a5c70', opacity: 0.5, padding: '14px 18px 4px', letterSpacing: '.5px' }}>Sistema</div>

        {[
          { icon: '⬡', label: 'Integraciones' },
        ].map(({ icon, label }) => (
          <div key={label} style={SB_ITEM_BASE}
            onMouseEnter={e => { (e.currentTarget as HTMLElement).style.color = '#c8d8e8'; (e.currentTarget as HTMLElement).style.background = 'rgba(255,255,255,.03)'; }}
            onMouseLeave={e => { (e.currentTarget as HTMLElement).style.color = '#4a5c70'; (e.currentTarget as HTMLElement).style.background = 'transparent'; }}>
            <span>{icon}</span> {label}
          </div>
        ))}

        <div onClick={() => setActiveTab('v-audit')} style={SB_ITEM_BASE}
          onMouseEnter={e => { (e.currentTarget as HTMLElement).style.color = '#c8d8e8'; (e.currentTarget as HTMLElement).style.background = 'rgba(255,255,255,.03)'; }}
          onMouseLeave={e => { (e.currentTarget as HTMLElement).style.color = '#4a5c70'; (e.currentTarget as HTMLElement).style.background = 'transparent'; }}>
          <span>◉</span> Auditoría
          <span style={{ marginLeft: 'auto', background: '#c84b1a', color: '#fff', fontSize: '9px', fontWeight: 700, padding: '2px 6px', borderRadius: '2px', fontFamily: MONO }}>1</span>
        </div>

        <div style={SB_ITEM_BASE}
          onMouseEnter={e => { (e.currentTarget as HTMLElement).style.color = '#c8d8e8'; (e.currentTarget as HTMLElement).style.background = 'rgba(255,255,255,.03)'; }}
          onMouseLeave={e => { (e.currentTarget as HTMLElement).style.color = '#4a5c70'; (e.currentTarget as HTMLElement).style.background = 'transparent'; }}>
          <span>⊕</span> Marketplace
        </div>

        <div style={{ height: '1px', background: 'rgba(255,255,255,.07)', margin: '4px 0' }} />

        <div style={{ fontFamily: MONO, fontSize: '9px', color: '#4a5c70', opacity: 0.5, padding: '14px 18px 4px', letterSpacing: '.5px' }}>Cuenta</div>

        <div style={SB_ITEM_BASE}
          onMouseEnter={e => { (e.currentTarget as HTMLElement).style.color = '#c8d8e8'; (e.currentTarget as HTMLElement).style.background = 'rgba(255,255,255,.03)'; }}
          onMouseLeave={e => { (e.currentTarget as HTMLElement).style.color = '#4a5c70'; (e.currentTarget as HTMLElement).style.background = 'transparent'; }}>
          <span>⚙</span> Configuración
        </div>

        <div style={{ marginTop: 'auto', borderTop: '1px solid rgba(255,255,255,.07)', padding: '14px 18px' }}>
          <div style={{ fontFamily: MONO, fontSize: '11px', color: '#4a5c70' }}>
            <strong style={{ display: 'block', color: '#c8d8e8', fontSize: '12px', marginBottom: '2px', fontWeight: 700 }}>Ops Team</strong>
            Plan Team · 47K/100K
          </div>
        </div>
      </div>

      {/* ── MAIN ── */}
      <div style={{ flex: 1, overflowY: 'auto', background: '#141920' }}>

        {/* Topbar */}
        <div style={{ padding: '20px 32px', borderBottom: '1px solid rgba(255,255,255,.07)', display: 'flex', alignItems: 'flex-end', justifyContent: 'space-between' }}>
          <div>
            <div style={{ fontFamily: HEAD, fontSize: '24px', fontWeight: 600, lineHeight: 1, color: '#e8f0f8', letterSpacing: '-.5px' }}>buenos_dias.run()</div>
            <div style={{ fontFamily: MONO, fontSize: '11px', color: '#4a5c70', marginTop: '5px' }}>// mié 17 abr 2026 · 10:34 · prod-cluster-01</div>
          </div>
          <div style={{ fontFamily: MONO, fontSize: '11px', color: '#8a9ab0', border: '1px solid rgba(255,255,255,.07)', padding: '6px 14px', borderRadius: '2px', display: 'flex', alignItems: 'center', gap: '8px', background: '#1c2533' }}>
            <span style={{ width: '5px', height: '5px', borderRadius: '50%', background: '#4ade80', animation: 'liveblink 2s infinite', display: 'inline-block' }} />
            últimos 7 días
          </div>
        </div>

        {/* Metrics */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4,1fr)', borderBottom: '1px solid rgba(255,255,255,.07)' }}>
          {[
            { label: 'Agentes activos', val: '12', valColor: '#4ade80', sub: 'de 15 en tu plan', delta: '↑ 2 nuevos esta semana', deltaColor: '#4ade80', bar: null },
            { label: 'Acciones ejecutadas', val: '47.832', valColor: '#e8f0f8', sub: 'de 100.000 este mes', delta: null, deltaColor: '', bar: 47 },
            { label: 'Tasa de éxito', val: '98.3%', valColor: '#4ade80', sub: 'últimas 24h', delta: '↑ +0.4% vs ayer', deltaColor: '#4ade80', bar: null },
            { label: 'Ahorro estimado', val: '$23.4k', valColor: '#e8f0f8', sub: 'horas de trabajo manual', delta: 'ROI 4.6× este mes', deltaColor: '#4ade80', bar: null },
          ].map((m, i) => (
            <div key={i} style={{ padding: '22px 28px', borderRight: i < 3 ? '1px solid rgba(255,255,255,.07)' : 'none', background: '#1c2533' }}>
              <div style={{ fontFamily: MONO, fontSize: '9px', letterSpacing: '.5px', color: '#4a5c70', marginBottom: '10px' }}>{m.label}</div>
              <div style={{ fontFamily: HEAD, fontSize: '34px', fontWeight: 700, letterSpacing: '-1px', lineHeight: 1, color: m.valColor }}>{m.val}</div>
              <div style={{ fontFamily: MONO, fontSize: '11px', color: '#8a9ab0', marginTop: '5px' }}>{m.sub}</div>
              {m.delta && <div style={{ fontFamily: MONO, fontSize: '11px', marginTop: '4px', color: m.deltaColor }}>{m.delta}</div>}
              {m.bar !== null && (
                <div style={{ height: '2px', background: 'rgba(255,255,255,.08)', marginTop: '10px', borderRadius: '1px', overflow: 'hidden' }}>
                  <div style={{ height: '100%', background: '#c84b1a', borderRadius: '1px', width: `${m.bar}%` }} />
                </div>
              )}
            </div>
          ))}
        </div>

        {/* Agents Table */}
        <div style={{ padding: '24px 32px' }}>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '14px' }}>
            <div style={{ fontFamily: MONO, fontSize: '11px', color: '#4a5c70', letterSpacing: '.3px' }}>// agentes activos</div>
            <button
              onClick={() => setActiveTab('v-build')}
              style={{ padding: '7px 16px', background: '#c84b1a', color: '#fff', fontFamily: MONO, fontSize: '11px', fontWeight: 500, border: 'none', cursor: 'pointer', borderRadius: '2px', transition: 'background .15s' }}
              onMouseEnter={e => (e.currentTarget.style.background = '#e05a22')}
              onMouseLeave={e => (e.currentTarget.style.background = '#c84b1a')}
            >
              + nuevo agente
            </button>
          </div>

          <table style={{ width: '100%', borderCollapse: 'collapse', border: '1px solid rgba(255,255,255,.07)' }}>
            <thead>
              <tr>
                {['Agente', 'Estado', 'Hoy', 'Actividad', 'Acciones'].map(h => (
                  <th key={h} style={{ padding: '9px 16px', fontFamily: MONO, fontSize: '9px', fontWeight: 500, letterSpacing: '.5px', color: '#4a5c70', borderBottom: '1px solid rgba(255,255,255,.07)', textAlign: 'left', background: '#0d1219' }}>{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {[
                { name: 'Procesador de facturas RRHH', pipe: 'Gmail → Claude → SAP B1', badge: 'ok' as BadgeKind, label: 'Activo', today: '234', todayColor: '#e8f0f8', ago: 'hace 2 min', btns: [{ label: 'Ver logs', action: () => setActiveTab('v-audit') }, { label: 'Pausar' }] },
                { name: 'Triaje de tickets soporte', pipe: 'Zendesk → Claude → Slack', badge: 'ok' as BadgeKind, label: 'Activo', today: '891', todayColor: '#e8f0f8', ago: 'hace 30 seg', btns: [{ label: 'Ver logs', action: () => setActiveTab('v-audit') }, { label: 'Pausar' }] },
                { name: 'Clasificador de leads CRM', pipe: 'HubSpot → Claude → Salesforce', badge: 'warn' as BadgeKind, label: 'Pausado', today: '—', todayColor: '#e8f0f8', ago: 'hace 2h', btns: [{ label: 'Ver logs' }, { label: 'Reanudar' }] },
                { name: 'Generador de reportes finanzas', pipe: 'SAP → Claude → Google Sheets', badge: 'err' as BadgeKind, label: 'Error', today: '12', todayColor: '#f87171', ago: 'hace 45 min', btns: [{ label: 'Ver error', action: () => setActiveTab('v-audit') }, { label: 'Rollback', danger: true }] },
              ].map((row, i, arr) => (
                <tr key={i}>
                  {(['name-pipe', 'badge', 'today', 'ago', 'btns'] as const).map(col => {
                    const tdStyle: React.CSSProperties = {
                      padding: '14px 16px',
                      borderBottom: i < arr.length - 1 ? '1px solid rgba(255,255,255,.04)' : 'none',
                      verticalAlign: 'top',
                      background: '#1c2533',
                    };
                    if (col === 'name-pipe') return (
                      <td key={col} style={tdStyle}>
                        <div style={{ fontWeight: 600, fontSize: '14px', color: '#e8f0f8' }}>{row.name}</div>
                        <div style={{ fontFamily: MONO, fontSize: '10px', color: '#4a5c70', marginTop: '3px' }}>{row.pipe}</div>
                      </td>
                    );
                    if (col === 'badge') return <td key={col} style={tdStyle}><Badge kind={row.badge} label={row.label} /></td>;
                    if (col === 'today') return (
                      <td key={col} style={tdStyle}>
                        <span style={{ fontFamily: MONO, fontSize: '15px', fontWeight: 500, color: row.todayColor, opacity: row.today === '—' ? 0.3 : 1 }}>{row.today}</span>
                      </td>
                    );
                    if (col === 'ago') return <td key={col} style={tdStyle}><span style={{ fontFamily: MONO, fontSize: '11px', color: '#4a5c70' }}>{row.ago}</span></td>;
                    return (
                      <td key={col} style={tdStyle}>
                        {row.btns.map((b, bi) => (
                          <button
                            key={bi}
                            onClick={b.action}
                            style={{
                              padding: '5px 12px', background: 'transparent',
                              border: '1px solid rgba(255,255,255,.07)',
                              fontFamily: MONO, fontSize: '10px', cursor: 'pointer',
                              borderRadius: '2px', color: '#8a9ab0', transition: 'all .15s', marginRight: '6px',
                            }}
                            onMouseEnter={e => {
                              const el = e.currentTarget;
                              if ((b as any).danger) { el.style.borderColor = '#f87171'; el.style.color = '#f87171'; }
                              else { el.style.borderColor = 'rgba(255,255,255,.3)'; el.style.color = '#e8f0f8'; }
                            }}
                            onMouseLeave={e => { e.currentTarget.style.borderColor = 'rgba(255,255,255,.07)'; e.currentTarget.style.color = '#8a9ab0'; }}
                          >
                            {b.label}
                          </button>
                        ))}
                      </td>
                    );
                  })}
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Bottom: Alerts + Chart */}
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 264px', borderTop: '1px solid rgba(255,255,255,.07)' }}>

          {/* Alerts */}
          <div style={{ padding: '24px 32px', borderRight: '1px solid rgba(255,255,255,.07)', background: '#1c2533' }}>
            <div style={{ fontFamily: MONO, fontSize: '11px', color: '#4a5c70', letterSpacing: '.3px', marginBottom: '10px' }}>// alertas</div>

            {[
              { g: '!', gColor: '#f87171', msg: 'Agente finanzas — error paso 3/7', msgColor: '#f87171', meta: 'Timeout SAP B1', link: true },
              { g: '▲', gColor: '#fbbf24', msg: 'Uso al 47% — ritmo acelerado', msgColor: '#e8f0f8', meta: 'Proyección: límite en 18 días · hace 2h', link: false },
              { g: '✓', gColor: '#4ade80', msg: 'Agente RRHH en producción', msgColor: '#e8f0f8', meta: 'Primera ejecución exitosa · hace 6h', link: false },
            ].map((al, i, arr) => (
              <div key={i} style={{ display: 'flex', gap: '14px', padding: '12px 0', borderBottom: i < arr.length - 1 ? '1px solid rgba(255,255,255,.04)' : 'none' }}>
                <div style={{ fontFamily: MONO, fontSize: '13px', flexShrink: 0, width: '16px', color: al.gColor }}>{al.g}</div>
                <div>
                  <div style={{ fontSize: '13px', fontWeight: 500, color: al.msgColor }}>{al.msg}</div>
                  <div style={{ fontFamily: MONO, fontSize: '10px', color: '#4a5c70', marginTop: '3px' }}>
                    {al.meta}{al.link && <> · <span onClick={() => setActiveTab('v-audit')} style={{ color: '#e05a22', cursor: 'pointer', textDecoration: 'underline', textUnderlineOffset: '2px' }}>Ver auditoría →</span></>}
                  </div>
                </div>
              </div>
            ))}
          </div>

          {/* Chart */}
          <div style={{ padding: '24px', background: '#1c2533' }}>
            <div style={{ fontFamily: MONO, fontSize: '11px', color: '#4a5c70', letterSpacing: '.3px' }}>// acciones / hora</div>
            <div style={{ fontFamily: MONO, fontSize: '10px', color: '#4a5c70', marginTop: '2px' }}>últimas 24h</div>
            <div style={{ height: '90px', display: 'flex', alignItems: 'flex-end', gap: '3px', marginTop: '14px' }}>
              {chBars.map((h, i) => (
                <div
                  key={i}
                  style={{
                    flex: 1, borderRadius: '1px 1px 0 0',
                    height: `${h}%`,
                    background: h > 85 ? '#c84b1a' : h > 60 ? 'rgba(255,255,255,.25)' : 'rgba(255,255,255,.12)',
                    transition: 'opacity .2s',
                    cursor: 'default',
                  }}
                  onMouseEnter={e => (e.currentTarget.style.background = '#e05a22')}
                  onMouseLeave={e => (e.currentTarget.style.background = h > 85 ? '#c84b1a' : h > 60 ? 'rgba(255,255,255,.25)' : 'rgba(255,255,255,.12)')}
                />
              ))}
            </div>
          </div>
        </div>

      </div>
    </div>
  );
}
