import { create } from 'zustand';

export type TabId = 'v-land' | 'v-dash' | 'v-build' | 'v-audit';

interface NavigationState {
  activeTab: TabId;
  setActiveTab: (tab: TabId) => void;
}

export const useNavigationStore = create<NavigationState>((set) => ({
  activeTab: 'v-land',
  setActiveTab: (tab) => set({ activeTab: tab }),
}));
