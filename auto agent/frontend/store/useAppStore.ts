import { create } from "zustand";

export type View = "landing" | "dashboard" | "builder" | "audit";

interface AppState {
  view: View;
  setView: (v: View) => void;
  selectedAgentId: string | null;
  setSelectedAgent: (id: string | null) => void;
}

export const useAppStore = create<AppState>((set) => ({
  view: "landing",
  setView: (view) => set({ view }),
  selectedAgentId: null,
  setSelectedAgent: (id) => set({ selectedAgentId: id }),
}));
