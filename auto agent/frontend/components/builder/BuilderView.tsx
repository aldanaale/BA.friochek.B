"use client";

import { ReactFlow, Background, Controls, Node, Edge } from '@xyflow/react';
import '@xyflow/react/dist/style.css';

const MONO = "var(--font-ibm-plex-mono), monospace";

/* ── Custom node exactly matching HTML .fn classes ── */
function AutoAgentNode({ data }: { data: any }) {
  const base: React.CSSProperties = {
    background: '#1c2533',
    border: '1px solid rgba(255,255,255,.07)',
    borderRadius: '3px',
    padding: '13px 15px',
    minWidth: '145px',
    boxShadow: '0 4px 16px rgba(0,0,0,.3)',
    cursor: 'pointer',
    transition: 'all .15s',
    userSelect: 'none',
    fontFamily: MONO,
  };

  let nodeStyle = { ...base };
  let tagColor = '#4a5c70';

  if (data.type === 'trig') {
    nodeStyle = { ...base, borderColor: 'rgba(99,102,241,.4)', background: 'rgba(99,102,241,.07)' };
    tagColor = 'rgba(129,140,248,.8)';
  } else if (data.type === 'act') {
    nodeStyle = { ...base, borderColor: 'rgba(74,222,128,.3)', background: 'rgba(74,222,128,.05)' };
    tagColor = '#4ade80';
  } else if (data.type === 'cond') {
    nodeStyle = { ...base, borderColor: 'rgba(251,191,36,.35)', background: 'rgba(251,191,36,.05)' };
    tagColor = '#fbbf24';
  }

  if (data.sel) {
    nodeStyle = { ...nodeStyle, borderColor: '#c84b1a', boxShadow: '0 0 0 1px rgba(200,75,26,.3), 0 6px 20px rgba(0,0,0,.4)' };
  }

  return (
    <div style={nodeStyle}>
      <div style={{ fontSize: '8px', color: tagColor, marginBottom: '5px', letterSpacing: '.3px' }}>{data.tag}</div>
      <div style={{ fontSize: '12px', fontWeight: 600, lineHeight: 1.3, color: '#e8f0f8' }}>{data.title}</div>
      <div style={{ fontSize: '10px', color: '#4a5c70', marginTop: '3px' }}>{data.sub}</div>
      {data.run && (
        <div style={{ display: 'flex', alignItems: 'center', gap: '5px', fontSize: '9px', color: '#4ade80', marginTop: '6px' }}>
          <div style={{ width: '7px', height: '7px', border: '1.5px solid rgba(74,222,128,.2)', borderTopColor: '#4ade80', borderRadius: '50%', animation: 'spin .7s linear infinite' }} />
          procesando...
        </div>
      )}
    </div>
  );
}

const nodeTypes = { autoagent: AutoAgentNode };

const initialNodes: Node[] = [
  { id: '1', type: 'autoagent', position: { x: 58, y: 118 }, data: { tag: 'Trigger', title: 'Email recibido', sub: 'facturas@empresa.com', type: 'trig', run: true } },
  { id: '2', type: 'autoagent', position: { x: 278, y: 118 }, data: { tag: 'Acción IA', title: 'Extraer datos', sub: 'Claude Sonnet 4.6', type: 'act', sel: true } },
  { id: '3', type: 'autoagent', position: { x: 488, y: 118 }, data: { tag: 'Condición', title: '¿Monto > $1M?', sub: 'campo: monto', type: 'cond' } },
  { id: '4', type: 'autoagent', position: { x: 696, y: 74 }, data: { tag: 'Acción', title: 'Slack', sub: '#aprobaciones', type: 'act' } },
  { id: '5', type: 'autoagent', position: { x: 696, y: 168 }, data: { tag: 'Acción', title: 'SAP B1', sub: 'registro automático', type: 'act' } },
];

const initialEdges: Edge[] = [
  { id: 'e1-2', source: '1', target: '2', type: 'straight', style: { stroke: 'rgba(255,255,255,.18)', strokeWidth: 1.5 } },
  { id: 'e2-3', source: '2', target: '3', type: 'straight', style: { stroke: 'rgba(255,255,255,.18)', strokeWidth: 1.5 } },
  { id: 'e3-4', source: '3', target: '4', type: 'smoothstep', animated: true, style: { stroke: 'rgba(42,122,75,.4)', strokeWidth: 1.5, strokeDasharray: '5 3' }, label: 'Sí >$1M', labelStyle: { fontFamily: MONO, fontSize: '9px', fill: 'rgba(160,106,16,.8)' } },
  { id: 'e3-5', source: '3', target: '5', type: 'smoothstep', animated: true, style: { stroke: 'rgba(42,122,75,.4)', strokeWidth: 1.5, strokeDasharray: '5 3' }, label: 'No', labelStyle: { fontFamily: MONO, fontSize: '9px', fill: 'rgba(160,106,16,.8)' } },
];

const PALETTE = [
  { cat: 'Triggers', items: ['📨 Email', '🪝 Webhook', '⏰ Scheduler'] },
  { cat: 'IA', items: ['🧠 Llamar LLM', '🔎 RAG search', '📝 Generar texto'] },
  { cat: 'Lógica', items: ['⚡ Condición', '🔄 Loop', '👤 Human-in-loop'] },
  { cat: 'Apps', items: ['💬 Slack', '🏢 SAP B1', '☁ Salesforce', '📊 Sheets', '✉ Gmail'] },
];

export function BuilderView() {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: 'calc(100vh - 48px)', background: '#141920' }}>

      {/* Topbar */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '12px', padding: '0 20px', height: '44px', background: '#0d1219', borderBottom: '1px solid rgba(255,255,255,.07)', flexShrink: 0 }}>
        <input
          defaultValue="Procesador de facturas RRHH"
          style={{ fontFamily: MONO, fontSize: '13px', fontWeight: 500, background: 'transparent', border: 'none', color: '#e8f0f8', padding: '4px 8px', borderRadius: '2px', outline: 'none', cursor: 'text', transition: 'background .15s' }}
          onMouseEnter={e => (e.currentTarget.style.background = 'rgba(255,255,255,.05)')}
          onMouseLeave={e => (e.currentTarget.style.background = 'transparent')}
        />
        <span style={{ padding: '3px 9px', fontFamily: MONO, fontSize: '9px', borderRadius: '2px', background: 'rgba(251,191,36,.1)', color: '#fbbf24', border: '1px solid rgba(251,191,36,.2)' }}>Borrador</span>
        <div style={{ flex: 1 }} />
        <span style={{ fontFamily: MONO, fontSize: '10px', color: '#4a5c70' }}>guardado hace 3 min</span>
        <button
          style={{ padding: '6px 16px', fontFamily: MONO, fontSize: '11px', fontWeight: 500, cursor: 'pointer', borderRadius: '2px', background: 'transparent', border: '1px solid rgba(255,255,255,.12)', color: '#8a9ab0', transition: 'all .15s' }}
          onMouseEnter={e => { e.currentTarget.style.borderColor = 'rgba(255,255,255,.3)'; e.currentTarget.style.color = '#e8f0f8'; }}
          onMouseLeave={e => { e.currentTarget.style.borderColor = 'rgba(255,255,255,.12)'; e.currentTarget.style.color = '#8a9ab0'; }}
        >
          ▷ Probar
        </button>
        <button
          style={{ padding: '6px 16px', fontFamily: MONO, fontSize: '11px', fontWeight: 500, cursor: 'pointer', borderRadius: '2px', background: '#2a7a4b', border: 'none', color: '#fff', transition: 'background .15s' }}
          onMouseEnter={e => (e.currentTarget.style.background = '#235f3b')}
          onMouseLeave={e => (e.currentTarget.style.background = '#2a7a4b')}
        >
          ⬆ Publicar
        </button>
      </div>

      <div style={{ display: 'flex', flex: 1, overflow: 'hidden' }}>

        {/* Palette */}
        <div style={{ width: '175px', flexShrink: 0, background: '#0d1219', borderRight: '1px solid rgba(255,255,255,.07)', overflowY: 'auto', padding: '14px 0' }}>
          {PALETTE.map(({ cat, items }) => (
            <div key={cat}>
              <div style={{ fontFamily: MONO, fontSize: '9px', color: '#4a5c70', opacity: 0.6, padding: '12px 16px 5px', letterSpacing: '.3px' }}>{cat}</div>
              {items.map(item => {
                const [icon, ...rest] = item.split(' ');
                return (
                  <div
                    key={item}
                    style={{ display: 'flex', alignItems: 'center', gap: '8px', padding: '7px 16px', fontFamily: MONO, fontSize: '11px', color: '#8a9ab0', cursor: 'grab', transition: 'all .15s', borderLeft: '2px solid transparent' }}
                    onMouseEnter={e => { const el = e.currentTarget as HTMLElement; el.style.background = 'rgba(255,255,255,.04)'; el.style.color = '#e8f0f8'; el.style.borderLeftColor = '#c84b1a'; }}
                    onMouseLeave={e => { const el = e.currentTarget as HTMLElement; el.style.background = 'transparent'; el.style.color = '#8a9ab0'; el.style.borderLeftColor = 'transparent'; }}
                  >
                    <span style={{ fontSize: '13px', width: '16px', textAlign: 'center' }}>{icon}</span>
                    {rest.join(' ')}
                  </div>
                );
              })}
            </div>
          ))}
        </div>

        {/* Canvas */}
        <div style={{ flex: 1, position: 'relative', background: '#141920', backgroundImage: 'radial-gradient(circle, rgba(255,255,255,.06) 1px, transparent 1px)', backgroundSize: '22px 22px' }}>
          <ReactFlow
            colorMode="dark"
            nodes={initialNodes}
            edges={initialEdges}
            nodeTypes={nodeTypes}
            fitView
            minZoom={0.5}
            maxZoom={1.5}
            style={{ background: 'transparent' }}
          >
            <Controls />
          </ReactFlow>
        </div>

        {/* Properties panel */}
        <div style={{ width: '268px', flexShrink: 0, background: '#0d1219', borderLeft: '1px solid rgba(255,255,255,.07)', display: 'flex', flexDirection: 'column', overflowY: 'auto' }}>
          <div style={{ padding: '15px 18px', borderBottom: '1px solid rgba(255,255,255,.07)' }}>
            <div style={{ fontFamily: MONO, fontSize: '9px', color: '#4a5c70', letterSpacing: '.3px' }}>Nodo seleccionado</div>
            <div style={{ fontFamily: MONO, fontSize: '14px', fontWeight: 600, marginTop: '3px', color: '#e8f0f8' }}>Extraer datos</div>
          </div>

          <div style={{ padding: '16px 18px', flex: 1 }}>
            {[
              { label: 'Modelo LLM', type: 'select', options: ['Claude Sonnet 4.6 — recomendado', 'GPT-4o', 'Gemini 1.5 Pro', 'Claude Haiku 4.5 — rápido'] },
              { label: 'Prompt del sistema', type: 'textarea', value: 'Extrae del email: RUT proveedor, monto total, fecha, número de factura. Responde solo en JSON válido.', color: '#c8d8e8' },
              { label: 'Output schema', type: 'textarea', value: '{"rut":"string","monto":"number","fecha":"date","numero":"string"}', color: '#2a7a4b' },
              { label: 'Timeout', type: 'input', value: '30 segundos' },
            ].map(field => (
              <div key={field.label} style={{ marginBottom: '16px' }}>
                <label style={{ fontFamily: MONO, fontSize: '9px', color: '#4a5c70', marginBottom: '5px', display: 'block', letterSpacing: '.3px' }}>{field.label}</label>
                {field.type === 'select' ? (
                  <select style={{ width: '100%', background: '#1c2533', border: '1px solid rgba(255,255,255,.07)', borderRadius: '2px', color: '#e8f0f8', fontFamily: MONO, fontSize: '12px', padding: '7px 10px', outline: 'none' }}>
                    {field.options!.map(o => <option key={o}>{o}</option>)}
                  </select>
                ) : field.type === 'textarea' ? (
                  <textarea
                    defaultValue={field.value}
                    style={{ width: '100%', background: '#1c2533', border: '1px solid rgba(255,255,255,.07)', borderRadius: '2px', color: field.color || '#e8f0f8', fontFamily: MONO, fontSize: '11px', padding: '7px 10px', outline: 'none', resize: 'none', height: '72px', lineHeight: 1.6 }}
                  />
                ) : (
                  <input
                    defaultValue={field.value}
                    style={{ width: '100%', background: '#1c2533', border: '1px solid rgba(255,255,255,.07)', borderRadius: '2px', color: '#e8f0f8', fontFamily: MONO, fontSize: '12px', padding: '7px 10px', outline: 'none' }}
                  />
                )}
              </div>
            ))}
          </div>

          <div style={{ padding: '15px 18px', borderTop: '1px solid rgba(255,255,255,.07)' }}>
            <button
              style={{ width: '100%', padding: '10px', background: '#c84b1a', color: '#fff', fontFamily: MONO, fontSize: '11px', fontWeight: 500, border: 'none', cursor: 'pointer', borderRadius: '2px', transition: 'background .15s' }}
              onMouseEnter={e => (e.currentTarget.style.background = '#e05a22')}
              onMouseLeave={e => (e.currentTarget.style.background = '#c84b1a')}
            >
              ▷ Ejecutar prueba
            </button>
            <div style={{ marginTop: '10px', background: '#141920', border: '1px solid rgba(255,255,255,.07)', borderRadius: '2px', padding: '12px', fontFamily: MONO, fontSize: '10px', color: '#4ade80', lineHeight: 1.7, whiteSpace: 'pre-wrap' }}>
{`✓ Resultado (1.2s · 312 tok)
{
  "rut": "76.123.456-7",
  "monto": 2450000,
  "fecha": "2026-04-17",
  "numero": "4821"
}`}
            </div>
          </div>
        </div>

      </div>
    </div>
  );
}
