export type AgentStatus = "active" | "paused" | "error";

export interface Agent {
  id: string;
  name: string;
  pipeline: string;
  status: AgentStatus;
  todayCount: number;
  lastActivity: string;
}

export interface Metric {
  label: string;
  value: string;
  sub: string;
  delta?: string;
  deltaUp?: boolean;
  barPct?: number;
  variant?: "ok" | "err" | "default";
}

export interface Alert {
  id: string;
  level: "error" | "warning" | "success";
  message: string;
  meta: string;
  gotoAudit?: boolean;
}

export interface ConsoleEntry {
  ts: string;
  type: "ok" | "warn" | "err" | "dim" | "ember";
  msg: string;
}

export interface ExecutionStep {
  n: number;
  status: "ok" | "fail";
  title: string;
  detail?: string;
  io?: { tag: string; content: string; tagVariant?: "error" }[];
  dur: string;
  canRollback?: boolean;
}

export interface Execution {
  id: string;
  agent: string;
  pipeline: string;
  status: "ok" | "error";
  statusLabel: string;
  ts: string;
  steps: ExecutionStep[];
}

export type NodeType = "trigger" | "action" | "condition";

export interface FlowNodeData {
  tag: string;
  title: string;
  sub?: string;
  running?: boolean;
  type: NodeType;
}
