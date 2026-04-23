const FEATURES = [
  {
    tag: "Constructor",
    title: "Flujos visuales sin código",
    body: "Arrastra nodos, conecta sistemas y define condiciones. Tu equipo de operaciones lo construye sin tickets a ingeniería.",
  },
  {
    tag: "Auditoría",
    title: "Cada decisión explicada",
    body: "Cada paso logueado con input, output y razonamiento. Cuando algo falla, sabes exactamente dónde, cuándo y por qué.",
  },
  {
    tag: "Resiliencia",
    title: "Rollback automático",
    body: "Error en producción. Un clic. El agente vuelve al último estado estable sin pérdida de datos ni llamada a ingeniería.",
  },
  {
    tag: "Integraciones",
    title: "Tu stack, conectado",
    body: "Salesforce, SAP, HubSpot, Workday, Slack, Gmail y más de 50 conectores via MCP. Listos en minutos, no meses.",
  },
];

export function FeaturesGrid() {
  return (
    <section className="border-t border-[rgba(255,255,255,0.07)] bg-[#141920]">
      <div className="grid grid-cols-[240px_1fr] px-14 py-14 border-b border-[rgba(255,255,255,0.07)] pb-10">
        <div className="font-mono text-[11px] font-normal text-[#e05a22] pt-1.5 tracking-[0.3px]">
          // funcionalidades
        </div>
        <h2
          className="text-[34px] leading-[1.15] font-bold text-[#e8f0f8] tracking-[-0.5px]"
          style={{ fontFamily: "var(--font-roboto-mono), monospace" }}
        >
          Lo que falta en<br />
          <em className="not-italic text-[#e05a22]">las otras plataformas</em>
        </h2>
      </div>
      <div className="grid grid-cols-2 border-t border-[rgba(255,255,255,0.07)]">
        {FEATURES.map((f, i) => (
          <div
            key={i}
            className="px-[52px] py-11 border-r border-b border-[rgba(255,255,255,0.07)] transition-colors duration-200 even:border-r-0 [&:nth-child(3)]:border-b-0 [&:nth-child(4)]:border-b-0 hover:bg-white/[0.025]"
          >
            <div className="font-mono text-[10px] font-normal text-[#4a5c70] mb-3.5 tracking-[0.3px]">
              {f.tag}
            </div>
            <div
              className="text-[22px] leading-[1.25] mb-3 font-semibold text-[#e8f0f8] tracking-[-0.3px]"
              style={{ fontFamily: "var(--font-roboto-mono), monospace" }}
            >
              {f.title}
            </div>
            <p className="text-[14px] text-[#8a9ab0] leading-[1.7] font-light">{f.body}</p>
          </div>
        ))}
      </div>
    </section>
  );
}
