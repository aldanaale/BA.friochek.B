"use client";

import { useNavigationStore } from '@/store/navigation';
import { TabsBar } from '@/components/layout/TabsBar';
import { LandingView } from '@/components/landing/LandingView';
import { DashboardView } from '@/components/dashboard/DashboardView';
import { BuilderView } from '@/components/builder/BuilderView';
import { AuditView } from '@/components/audit/AuditView';

export default function AutoAgentPage() {
  const { activeTab } = useNavigationStore();

  return (
    <>
      <TabsBar />
      {/* pt-[48px] = height of the fixed TabsBar */}
      <div style={{ paddingTop: '48px', minHeight: '100vh', background: '#141920' }}>
        {activeTab === 'v-land'  && <LandingView />}
        {activeTab === 'v-dash'  && <DashboardView />}
        {activeTab === 'v-build' && <BuilderView />}
        {activeTab === 'v-audit' && <AuditView />}
      </div>
    </>
  );
}
