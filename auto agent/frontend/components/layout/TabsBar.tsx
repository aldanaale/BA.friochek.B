"use client";

import { useNavigationStore, TabId } from '@/store/navigation';

export function TabsBar() {
  const { activeTab, setActiveTab } = useNavigationStore();
  const handleTab = (tab: TabId) => setActiveTab(tab);

  return (
    <div
      className="fixed top-0 left-0 right-0 z-[200] flex items-stretch border-b-2 border-[#c84b1a]"
      style={{ height: '48px', background: '#0c0b09' }}
    >
      {/* Logo */}
      <div
        className="flex items-center gap-[10px] px-6 border-r border-[#ffffff1a]"
        style={{ fontFamily: 'var(--font-barlow-condensed), sans-serif', fontWeight: 800, fontSize: '17px', letterSpacing: '0.5px', color: '#fff' }}
      >
        <div
          className="flex items-center justify-center text-white rounded-[2px]"
          style={{ width: '22px', height: '22px', background: '#c84b1a', fontSize: '11px', fontWeight: 900 }}
        >
          A
        </div>
        AutoAgent
      </div>

      {/* Tabs */}
      <div className="flex flex-1">
        {([
          { id: 'v-land',  label: 'Landing' },
          { id: 'v-dash',  label: 'Dashboard' },
          { id: 'v-build', label: 'Constructor' },
          { id: 'v-audit', label: 'Auditoría' },
        ] as { id: TabId; label: string }[]).map((tab) => (
          <div
            key={tab.id}
            onClick={() => handleTab(tab.id)}
            className={`flex items-center px-[22px] border-r border-[#ffffff12] cursor-pointer transition-all duration-150 select-none ${
              activeTab === tab.id
                ? 'text-white bg-[#c84b1a33] border-b-2 border-b-[#c84b1a] -mb-[2px]'
                : 'text-[#ffffff66] hover:text-[#ffffffbf] hover:bg-[#ffffff0a]'
            }`}
            style={{
              fontFamily: 'var(--font-barlow-condensed), sans-serif',
              fontSize: '13px',
              fontWeight: 600,
              letterSpacing: '0.8px',
              textTransform: 'uppercase',
            }}
          >
            {tab.label}
          </div>
        ))}
      </div>

      {/* Right actions */}
      <div className="flex items-center gap-2 px-4 ml-auto">
        <button
          style={{ fontFamily: 'var(--font-barlow-condensed), sans-serif', fontSize: '12px', fontWeight: 600, letterSpacing: '0.5px', textTransform: 'uppercase' }}
          className="py-[5px] px-[14px] rounded-[2px] border border-[#ffffff40] text-[#ffffffb3] bg-transparent hover:border-white hover:text-white transition-all cursor-pointer"
        >
          Docs
        </button>
        <button
          onClick={() => handleTab('v-dash')}
          style={{ fontFamily: 'var(--font-barlow-condensed), sans-serif', fontSize: '12px', fontWeight: 600, letterSpacing: '0.5px', textTransform: 'uppercase' }}
          className="py-[5px] px-[14px] rounded-[2px] border-none bg-[#c84b1a] text-white hover:bg-[#e05a22] transition-colors cursor-pointer"
        >
          Entrar →
        </button>
      </div>
    </div>
  );
}
