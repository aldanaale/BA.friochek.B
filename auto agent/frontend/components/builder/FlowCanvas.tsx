"use client";

import { useCallback } from "react";
import {
  ReactFlow,
  Background,
  Controls,
  MiniMap,
  BackgroundVariant,
  type Node,
  type Edge,
  useNodesState,
  useEdgesState,
  addEdge,
  type Connection,
} from "@xyflow/react";
import "@xyflow/react/dist/style.css";
import { TriggerNode }   from "./nodes/TriggerNode";
import { ActionNode }    from "./nodes/ActionNode";
import { ConditionNode } from "./nodes/ConditionNode";
import { FlowNodeData }  from "@/types";

const nodeTypes = {
  trigger:   TriggerNode,
  action:    ActionNode,
  condition: ConditionNode,
};

const INITIAL_NODES: Node<FlowNodeData>[] = [
  {
    id: "1", type: "trigger", position: { x: 58, y: 118 },
    data: { tag: "Trigger", title: "Email recibido", sub: "facturas@empresa.com", running: true, type: "trigger" },
  },
  {
    id: "2", type: "action", position: { x: 278, y: 118 },
    data: { tag: "Acción IA", title: "Extraer datos", sub: "Claude Sonnet 4.6", type: "action" },
  },
  {
    id: "3", type: "condition", position: { x: 488, y: 118 },
    data: { tag: "Condición", title: "¿Monto > $1M?", sub: "campo: monto", type: "condition" },
  },
  {
    id: "4", type: "action", position: { x: 696, y: 74 },
    data: { tag: "Acción", title: "Slack", sub: "#aprobaciones", type: "action" },
  },
  {
    id: "5", type: "action", position: { x: 696, y: 168 },
    data: { tag: "Acción", title: "SAP B1", sub: "registro automático", type: "action" },
  },
];

const INITIAL_EDGES: Edge[] = [
  { id: "e1-2", source: "1", target: "2", animated: false, style: { stroke: "rgba(255,255,255,0.18)", strokeWidth: 1.5 } },
  { id: "e2-3", source: "2", target: "3", animated: false, style: { stroke: "rgba(255,255,255,0.18)", strokeWidth: 1.5 } },
  { id: "e3-4", source: "3", sourceHandle: "yes", target: "4", animated: true, label: "Sí >$1M", labelStyle: { fill: "rgba(160,106,16,0.8)", fontSize: 9, fontFamily: "IBM Plex Mono" }, style: { stroke: "rgba(42,122,75,0.5)", strokeWidth: 1.5, strokeDasharray: "5 3" } },
  { id: "e3-5", source: "3", sourceHandle: "no",  target: "5", animated: true, label: "No",       labelStyle: { fill: "rgba(160,106,16,0.8)", fontSize: 9, fontFamily: "IBM Plex Mono" }, style: { stroke: "rgba(42,122,75,0.5)", strokeWidth: 1.5, strokeDasharray: "5 3" } },
];

export function FlowCanvas() {
  const [nodes, , onNodesChange] = useNodesState(INITIAL_NODES);
  const [edges, setEdges, onEdgesChange] = useEdgesState(INITIAL_EDGES);

  const onConnect = useCallback(
    (params: Connection) => setEdges((eds) => addEdge({ ...params, style: { stroke: "rgba(255,255,255,0.18)", strokeWidth: 1.5 } }, eds)),
    [setEdges]
  );

  return (
    <div className="flex-1 relative overflow-hidden">
      <ReactFlow
        nodes={nodes}
        edges={edges}
        nodeTypes={nodeTypes}
        onNodesChange={onNodesChange}
        onEdgesChange={onEdgesChange}
        onConnect={onConnect}
        fitView
        fitViewOptions={{ padding: 0.3 }}
        deleteKeyCode="Delete"
        proOptions={{ hideAttribution: true }}
      >
        <Background variant={BackgroundVariant.Dots} gap={22} size={1} color="rgba(255,255,255,0.06)" />
        <Controls showInteractive={false} />
        <MiniMap nodeColor={() => "#253041"} maskColor="rgba(13,18,25,0.6)" />
      </ReactFlow>
    </div>
  );
}
