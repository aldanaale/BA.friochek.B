"use client";

import { useAppStore } from "@/store/useAppStore";

const PLANS = [
  {
    name: "Builder",
    price: "$500",
    period: "/mes",
    desc: "Para empezar con agentes reales",
    features: ["3 agentes activos", "10.000 acciones al mes", "5 integraciones", "Auditoría básica"],
    cta: "Comenzar gratis",
    ctaVariant: "outline" as const,
    featured: false,
  },
  {
    name: "Team",
    price: "$2.000",
    period: "/mes",
    desc: "Para equipos de operaciones",
    features: ["15 agentes activos", "100.000 acciones al mes", "Integraciones ilimitadas", "Auditoría completa + rollback", "Soporte prioritario"],
    cta: "Empezar ahora",
    ctaVariant: "solid" as const,
    featured: true,
  },
  {
    name: "Enterprise",
    price: "$10k+",
    period: "/mes",
    desc: "Para grandes organizaciones",
    features: ["Agentes ilimitados", "On-premise disponible", "SLA + RBAC + SSO", "SOC 2 Type II"],
    cta: "Hablar con ventas",
    ctaVariant: "outline" as const,
    featured: false,
  },
];

export function PricingSection() {
  const setView = useAppStore((s) => s.setView);

  return (
    <section className="border-t border-[rgba(255,255,255,0.07)] px-14 py-20 bg-[#0d1219] text-[#c8d8e8]">
      <div className="font-mono text-[11px] font-normal text-[#e05a22] mb-3.5 tracking-[0.3px]">
        // precios
      </div>
      <h2
        className="text-[44px] leading-[1.05] font-bold mb-[52px] text-[#e8f0f8] tracking-[-1px]"
        style={{ fontFamily: "var(--font-roboto-mono), monospace" }}
      >
        Simple.<br />Sin sorpresas.
      </h2>
      <div className="grid grid-cols-3 border border-white/[0.08]">
        {PLANS.map((plan) => (
          <div
            key={plan.name}
            className={`p-10 border-r border-white/[0.08] last:border-r-0 flex flex-col ${
              plan.featured ? "bg-[#c84b1a]" : "bg-[#1c2533]"
            }`}
          >
            <div className={`font-mono text-[11px] font-normal tracking-[0.5px] mb-[18px] ${plan.featured ? "text-white/80" : "text-white/40"}`}>
              {plan.name}
            </div>
            <div
              className="text-[48px] font-bold tracking-[-1.5px] leading-none text-[#e8f0f8]"
              style={{ fontFamily: "var(--font-roboto-mono), monospace" }}
            >
              {plan.price}
              <span className="text-[16px] font-light opacity-60 font-mono">{plan.period}</span>
            </div>
            <div className={`text-[13px] my-2.5 mb-6 font-light font-mono ${plan.featured ? "text-white/75" : "text-white/40"}`}>
              {plan.desc}
            </div>
            <hr className="border-none border-t border-white/10 mb-[22px]" />
            <ul className="flex flex-col gap-0">
              {plan.features.map((f) => (
                <li
                  key={f}
                  className={`text-[13px] py-[5px] flex gap-2 font-mono font-light before:content-['//'] before:shrink-0 before:font-normal ${
                    plan.featured
                      ? "text-white/85 before:text-white/60"
                      : "text-white/50 before:text-[#e05a22]"
                  }`}
                >
                  {f}
                </li>
              ))}
            </ul>
            <div className="mt-auto pt-7">
              <button
                onClick={() => plan.ctaVariant === "solid" && setView("dashboard")}
                className={`w-full py-3 font-mono text-[12px] font-medium tracking-[0.3px] cursor-pointer rounded-[2px] transition-all duration-150 ${
                  plan.ctaVariant === "solid"
                    ? "bg-white border-none text-[#c84b1a] font-semibold hover:bg-[#f0f0f0]"
                    : "bg-transparent border border-white/[0.18] text-white/60 hover:border-white/40 hover:text-white"
                }`}
              >
                {plan.cta}
              </button>
            </div>
          </div>
        ))}
      </div>
    </section>
  );
}
