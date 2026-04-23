# AUTOAGENT — CONTEXTO DE PROGRAMACIÓN

Este documento es el contexto de referencia para programar el frontend de AutoAgent.
Cuando recibas una tarea de código, lee este archivo primero para entender la arquitectura, las convenciones y las decisiones ya tomadas.

---

## QUÉ ES EL PRODUCTO

AutoAgent es un SaaS enterprise no-code para construir y gestionar agentes de IA.
El usuario final es un equipo de operaciones (no ingenieros). El producto tiene 4 vistas principales:

1. **Landing** — marketing, conversión, precios
2. **Dashboard** — métricas en tiempo real, tabla de agentes, alertas
3. **Constructor** — editor visual drag-and-drop de flujos de agentes
4. **Auditoría** — historial de ejecuciones con trazabilidad paso a paso y rollback

---

## STACK FRONTEND (lo que debes usar siempre)

```
Framework:     Next.js 15 (App Router)
Editor visual: React Flow
Estilos:       Tailwind CSS v3
Componentes:   shadcn/ui (copiar al proyecto, no importar de npm directamente)
Tipografía:    Roboto Mono (titulares), IBM Plex Mono (datos/código), Barlow (cuerpo)
Estado global: Zustand
Data fetching: TanStack Query v5
Forms:         React Hook Form + Zod
WebSockets:    native WebSocket API (para estados de agente en tiempo real)
HTTP client:   fetch nativo con wrapper tipado en lib/api.ts
```

---

## STACK BACKEND (con lo que se integra el frontend)

```
API REST:      FastAPI corriendo en http://localhost:8000
Auth:          JWT Bearer token — access token 15min, refresh token 7 días
WebSocket:     ws://localhost:8000/ws/{execution_id}
Base de datos: PostgreSQL 16 + pgvector (NO acceder directo desde frontend)
Cache/Queue:   Redis (solo backend, invisible al frontend)
Billing:       Stripe — el frontend usa Stripe.js para checkout
```

---

## ESTRUCTURA DE CARPETAS (seguir siempre esta convención)

```
src/
├── app/
│   ├── (marketing)/
│   │   ├── page.tsx              # Landing
│   │   ├── precios/page.tsx
│   │   └── layout.tsx
│   ├── (app)/
│   │   ├── layout.tsx            # Shell con sidebar
│   │   ├── dashboard/page.tsx
│   │   ├── agents/
│   │   │   ├── page.tsx          # Lista de agentes
│   │   │   └── [id]/page.tsx     # Detalle de agente
│   │   ├── builder/
│   │   │   └── [id]/page.tsx     # Constructor visual
│   │   └── audit/
│   │       ├── page.tsx          # Lista de ejecuciones
│   │       └── [id]/page.tsx     # Detalle de ejecución
│   └── api/                      # BFF routes de Next.js
│       ├── auth/[...nextauth]/
│       └── agents/
├── components/
│   ├── ui/                       # shadcn/ui components
│   ├── builder/
│   │   ├── nodes/                # Componentes de nodos React Flow
│   │   │   ├── TriggerNode.tsx
│   │   │   ├── LLMNode.tsx
│   │   │   ├── ConditionNode.tsx
│   │   │   ├── ActionNode.tsx
│   │   │   └── HumanReviewNode.tsx
│   │   ├── NodePalette.tsx       # Panel izquierdo con nodos arrastrables
│   │   ├── PropertiesPanel.tsx   # Panel derecho de configuración
│   │   └── AgentCanvas.tsx       # Canvas principal React Flow
│   ├── dashboard/
│   │   ├── MetricCard.tsx
│   │   ├── AgentsTable.tsx
│   │   ├── AlertsPanel.tsx
│   │   └── ActivityChart.tsx
│   └── audit/
│       ├── ExecutionList.tsx
│       ├── ExecutionSteps.tsx
│       └── RollbackButton.tsx
├── lib/
│   ├── api.ts                    # Cliente HTTP tipado
│   ├── ws.ts                     # Cliente WebSocket
│   ├── auth.ts                   # Helpers de JWT
│   └── utils.ts                  # cn(), formatters
├── stores/
│   ├── builderStore.ts           # Estado del constructor (Zustand)
│   └── authStore.ts              # Usuario y token (Zustand)
├── hooks/
│   ├── useAgents.ts              # TanStack Query hooks
│   ├── useExecutions.ts
│   └── useAgentSocket.ts         # WebSocket hook
└── types/
    ├── agent.ts                  # Tipos de agente y grafo
    ├── execution.ts              # Tipos de ejecución y pasos
    └── api.ts                    # Tipos de respuestas de la API
```

---

## DISEÑO Y ESTILOS (seguir siempre estas reglas)

### Paleta de colores (CSS variables en globals.css)
```css
--slate:        #141920   /* fondo principal de todas las vistas */
--slate2:       #1c2533   /* fondo de cards, tables, sidebars */
--slate3:       #253041   /* hover de filas, fondo de inputs */
--slate-rule:   rgba(255,255,255,0.07)   /* bordes */
--slate-faint:  #4a5c70   /* texto desactivado, labels */
--slate-muted:  #8a9ab0   /* texto secundario */
--slate-text:   #c8d8e8   /* texto normal */
--slate-bright: #e8f0f8   /* texto destacado, titulares */
--ember:        #c84b1a   /* acento primario — botones CTA, bordes activos */
--ember2:       #e05a22   /* hover de ember */
--green:        #4ade80   /* estado activo, éxito */
--amber:        #fbbf24   /* advertencia, borrador */
--red:          #f87171   /* error, destructivo */
```

### Tipografía
```css
/* Titulares de secciones, números grandes */
font-family: 'Roboto Mono', 'IBM Plex Mono', monospace;

/* Datos, etiquetas, código, timestamps, pipes */
font-family: 'IBM Plex Mono', monospace;

/* Cuerpo de texto, párrafos */
font-family: 'Barlow', sans-serif;
```

### Reglas de diseño (nunca romper estas)
- Fondo base siempre `--slate` (#141920), NUNCA blanco ni paper
- Bordes siempre `--slate-rule` (rgba(255,255,255,0.07)), 1px solid, sin radius grande
- border-radius máximo: `4px` para cards, `2px` para botones y badges
- Botón primario: fondo `--ember`, texto blanco, sin sombra
- Sombras: `box-shadow: 0 4px 16px rgba(0,0,0,0.3)` — nunca sombras de colores
- Sin gradientes decorativos, sin glassmorphism, sin blur decorativo
- Títulos de sección siempre con prefijo `//` en monospace: `// agentes activos`
- Labels y etiquetas en monospace, font-weight 400-500, NUNCA uppercase con letter-spacing agresivo

### Clases de Tailwind equivalentes a usar
```
bg-[#141920]         → --slate
bg-[#1c2533]         → --slate2
bg-[#253041]         → --slate3
border-white/[0.07]  → --slate-rule
text-[#4a5c70]       → --slate-faint
text-[#8a9ab0]       → --slate-muted
text-[#c8d8e8]       → --slate-text
text-[#e8f0f8]       → --slate-bright
bg-[#c84b1a]         → --ember
text-[#4ade80]        → --green
text-[#fbbf24]       → --amber
text-[#f87171]       → --red
```

---

## TIPOS TYPESCRIPT (usar siempre estos)

```typescript
// types/agent.ts

export type AgentStatus = 'draft' | 'active' | 'paused' | 'error'

export type NodeType = 'trigger' | 'llm' | 'condition' | 'action' | 'human_review'

export interface AgentNode {
  id: string
  type: NodeType
  subtype?: string           // 'email' | 'webhook' | 'scheduler' para triggers
  label: string
  subtitle?: string
  config: Record<string, unknown>
  position: { x: number; y: number }
}

export interface AgentEdge {
  id: string
  source: string
  target: string
  condition?: string         // nombre de la rama para conditional edges
}

export interface AgentGraph {
  nodes: AgentNode[]
  edges: AgentEdge[]
}

export interface Agent {
  id: string
  org_id: string
  name: string
  description?: string
  status: AgentStatus
  graph: AgentGraph
  trigger_type: string
  executions_today: number
  success_rate_7d: number
  created_at: string
  last_execution_at?: string
}

// types/execution.ts

export type ExecutionStatus = 'running' | 'success' | 'error' | 'rolled_back' | 'waiting'
export type StepType = 'trigger' | 'llm' | 'condition' | 'action' | 'human_review'
export type StepStatus = 'success' | 'error' | 'running' | 'skipped'

export interface ExecutionStep {
  id: string
  step_number: number
  node_type: StepType
  node_name: string
  status: StepStatus
  duration_ms: number
  input_data: Record<string, unknown>
  output_data: Record<string, unknown>
  llm_model?: string
  llm_tokens?: number
  llm_cost_usd?: number
  tool_name?: string
  tool_integration?: string
  error_message?: string
}

export interface Execution {
  id: string
  agent_id: string
  agent_name: string
  status: ExecutionStatus
  trigger_type: string
  started_at: string
  finished_at?: string
  duration_ms?: number
  total_cost_usd?: number
  error?: {
    step_number: number
    node_name: string
    error_type: string
    message: string
    can_rollback: boolean
  }
  steps?: ExecutionStep[]
}
```

---

## CLIENTE HTTP (lib/api.ts — usar siempre este patrón)

```typescript
// lib/api.ts
const BASE = process.env.NEXT_PUBLIC_API_URL

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const token = getAccessToken()   // desde authStore de Zustand
  const res = await fetch(`${BASE}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...init?.headers,
    },
  })

  if (!res.ok) {
    const error = await res.json()
    throw new Error(error?.error?.message ?? 'Error desconocido')
  }

  return res.json()
}

export const api = {
  // Agents
  getAgents: (params?: Record<string, string>) =>
    request<{ data: Agent[]; total: number }>(`/agents?${new URLSearchParams(params)}`),

  getAgent: (id: string) =>
    request<Agent>(`/agents/${id}`),

  createAgent: (body: Partial<Agent>) =>
    request<Agent>('/agents', { method: 'POST', body: JSON.stringify(body) }),

  updateAgent: (id: string, body: Partial<Agent>) =>
    request<Agent>(`/agents/${id}`, { method: 'PATCH', body: JSON.stringify(body) }),

  publishAgent: (id: string) =>
    request<Agent>(`/agents/${id}/publish`, { method: 'POST' }),

  triggerAgent: (id: string, data: Record<string, unknown>) =>
    request<{ execution_id: string }>(`/agents/${id}/trigger`, {
      method: 'POST',
      body: JSON.stringify({ data }),
    }),

  // Executions
  getExecutions: (params?: Record<string, string>) =>
    request<{ data: Execution[]; total: number }>(`/executions?${new URLSearchParams(params)}`),

  getExecution: (id: string) =>
    request<Execution>(`/executions/${id}`),

  getExecutionSteps: (id: string) =>
    request<ExecutionStep[]>(`/executions/${id}/steps`),

  rollback: (id: string) =>
    request<{ success: boolean }>(`/executions/${id}/rollback`, { method: 'POST' }),

  // Billing
  getUsage: () =>
    request<{ agents: { active: number; limit: number }; actions: { used: number; limit: number } }>(
      '/billing/usage/current'
    ),
}
```

---

## WEBSOCKET PARA TIEMPO REAL (hooks/useAgentSocket.ts)

```typescript
// hooks/useAgentSocket.ts
// Conectar durante una ejecución para recibir actualizaciones de pasos en tiempo real

export function useAgentSocket(executionId: string | null) {
  const [steps, setSteps] = useState<ExecutionStep[]>([])
  const [status, setStatus] = useState<ExecutionStatus>('running')

  useEffect(() => {
    if (!executionId) return
    const ws = new WebSocket(`${process.env.NEXT_PUBLIC_WS_URL}/ws/${executionId}`)

    ws.onmessage = (event) => {
      const msg = JSON.parse(event.data)

      if (msg.type === 'step_complete') {
        setSteps(prev => [...prev, msg.step])
      }
      if (msg.type === 'execution_complete') {
        setStatus(msg.status)
        ws.close()
      }
    }

    return () => ws.close()
  }, [executionId])

  return { steps, status }
}
```

---

## ESTADO DEL CONSTRUCTOR (stores/builderStore.ts)

```typescript
// stores/builderStore.ts — Zustand store para el constructor visual
import { create } from 'zustand'
import { Node, Edge, addEdge, applyNodeChanges, applyEdgeChanges } from 'reactflow'

interface BuilderStore {
  nodes: Node[]
  edges: Edge[]
  selectedNodeId: string | null
  isDirty: boolean           // true cuando hay cambios sin guardar

  // Acciones
  setNodes: (nodes: Node[]) => void
  setEdges: (edges: Edge[]) => void
  onNodesChange: (changes: any) => void
  onEdgesChange: (changes: any) => void
  onConnect: (connection: any) => void
  selectNode: (id: string | null) => void
  updateNodeConfig: (id: string, config: Record<string, unknown>) => void
  loadGraph: (graph: AgentGraph) => void
  getGraphJSON: () => AgentGraph    // serializar para enviar a la API
  markSaved: () => void
}
```

---

## CONVENCIONES DE CÓDIGO

### Componentes
- Siempre funcionales con TypeScript
- Props tipadas con interface, nunca `any`
- Exportar como `export default` para páginas, `export function` para componentes
- Nombrar archivos en PascalCase para componentes, camelCase para hooks y utils

### Estilos
- Usar `cn()` de `lib/utils.ts` para clases condicionales (wrapper de `clsx` + `tailwind-merge`)
- No usar `style={}` inline salvo para valores dinámicos que Tailwind no puede expresar
- Variantes de componentes con `cva` (class-variance-authority)

### Datos
- Siempre usar TanStack Query para datos del servidor — nunca `useEffect` + `fetch` manual
- Los hooks de queries van en `hooks/use*.ts`
- Los mutations invalidan el query cache correspondiente tras éxito

### Errores
- Usar Error Boundaries en layouts de (app)
- Los errores de API se muestran con toast (sonner)
- Los estados de carga con skeletons, nunca spinners centrados en pantalla completa

### Accesibilidad
- Todo elemento interactivo tiene `aria-label` si no tiene texto visible
- Los nodos del constructor tienen `role="button"` y son navegables con teclado
- Los colores de estado (verde/rojo/amber) siempre tienen un indicador adicional (ícono o texto), nunca solo color

---

## ENDPOINTS DE LA API (referencia rápida)

```
POST   /auth/token                      → login, retorna access_token + refresh_token
POST   /auth/refresh                    → renovar access_token

GET    /agents                          → lista de agentes de la org
POST   /agents                          → crear agente
GET    /agents/:id                      → detalle de agente
PATCH  /agents/:id                      → actualizar agente
POST   /agents/:id/publish              → publicar agente (draft → active)
POST   /agents/:id/pause                → pausar agente
POST   /agents/:id/resume               → reanudar agente
POST   /agents/:id/trigger              → disparar manualmente

GET    /executions                      → historial de ejecuciones
GET    /executions/:id                  → detalle de ejecución
GET    /executions/:id/steps            → pasos con input/output
POST   /executions/:id/rollback         → hacer rollback

GET    /integrations/catalog            → integraciones disponibles
GET    /integrations/connected          → integraciones conectadas en esta org
POST   /integrations/connect            → conectar integración
DELETE /integrations/:id                → desconectar

GET    /billing/usage/current           → uso del mes actual
GET    /agents/:id/metrics?period=7d    → métricas de un agente

WS     /ws/:execution_id                → stream de pasos en tiempo real
```

---

## LO QUE NO HACER

- No usar `useEffect` para fetch de datos del servidor — usar TanStack Query
- No importar shadcn/ui de `@shadcn/ui` — los componentes se copian a `components/ui/`
- No usar colores fuera de la paleta definida — no azules, no grises claros, no blancos
- No usar `border-radius` mayor a `4px` — la estética es angular, no redondeada
- No usar sombras de colores — solo `rgba(0,0,0, x)`
- No usar uppercase con letter-spacing agresivo para labels — usar monospace lowercase con opacidad
- No crear stores de Zustand para datos del servidor — eso es trabajo de TanStack Query
- No hacer llamadas a la API directo en componentes — siempre a través de hooks en `hooks/`
- No usar `any` en TypeScript — si no sabes el tipo, usar `unknown` y narrowing
