"use client";

import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";

const schema = z.object({
  model:   z.string().min(1),
  prompt:  z.string().min(1),
  schema:  z.string(),
  timeout: z.string(),
});
type FormValues = z.infer<typeof schema>;

const MODELS = [
  "Claude Sonnet 4.6 — recomendado",
  "Claude Haiku 4.5 — rápido",
  "GPT-4o",
  "Gemini 1.5 Pro",
];

export function PropertiesPanel() {
  const { register, handleSubmit } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      model:   MODELS[0],
      prompt:  "Extrae del email: RUT proveedor, monto total, fecha, número de factura. Responde solo en JSON válido.",
      schema:  '{"rut":"string","monto":"number","fecha":"date","numero":"string"}',
      timeout: "30 segundos",
    },
  });

  const onTest = handleSubmit(() => {
    // In production: trigger API call and show result
  });

  return (
    <div className="w-[268px] shrink-0 bg-[#0d1219] border-l border-[rgba(255,255,255,0.07)] flex flex-col overflow-y-auto">
      <div className="px-[18px] py-[15px] border-b border-[rgba(255,255,255,0.07)]">
        <div className="font-mono text-[9px] font-normal text-[#4a5c70] tracking-[0.3px]">Nodo seleccionado</div>
        <div className="font-mono text-[14px] font-semibold mt-0.5 text-[#e8f0f8]">Extraer datos</div>
      </div>

      <form onSubmit={onTest} className="px-[18px] py-4 flex-1 flex flex-col gap-4">
        {/* Model */}
        <div>
          <label className="font-mono text-[9px] font-normal text-[#4a5c70] mb-[5px] block tracking-[0.3px]">
            Modelo LLM
          </label>
          <select
            {...register("model")}
            className="w-full bg-[#1c2533] border border-[rgba(255,255,255,0.07)] rounded-[2px] text-[#e8f0f8] font-mono text-[12px] px-2.5 py-[7px] outline-none focus:border-[#c84b1a] transition-colors"
          >
            {MODELS.map((m) => <option key={m}>{m}</option>)}
          </select>
        </div>

        {/* Prompt */}
        <div>
          <label className="font-mono text-[9px] font-normal text-[#4a5c70] mb-[5px] block tracking-[0.3px]">
            Prompt del sistema
          </label>
          <textarea
            {...register("prompt")}
            rows={4}
            className="w-full bg-[#1c2533] border border-[rgba(255,255,255,0.07)] rounded-[2px] text-[#c8d8e8] font-mono text-[11px] px-2.5 py-[7px] outline-none focus:border-[#c84b1a] transition-colors resize-none leading-[1.6]"
          />
        </div>

        {/* Schema */}
        <div>
          <label className="font-mono text-[9px] font-normal text-[#4a5c70] mb-[5px] block tracking-[0.3px]">
            Output schema
          </label>
          <textarea
            {...register("schema")}
            rows={3}
            className="w-full bg-[#1c2533] border border-[rgba(255,255,255,0.07)] rounded-[2px] text-[#2a7a4b] font-mono text-[11px] px-2.5 py-[7px] outline-none focus:border-[#c84b1a] transition-colors resize-none leading-[1.6]"
          />
        </div>

        {/* Timeout */}
        <div>
          <label className="font-mono text-[9px] font-normal text-[#4a5c70] mb-[5px] block tracking-[0.3px]">
            Timeout
          </label>
          <input
            {...register("timeout")}
            className="w-full bg-[#1c2533] border border-[rgba(255,255,255,0.07)] rounded-[2px] text-[#e8f0f8] font-mono text-[12px] px-2.5 py-[7px] outline-none focus:border-[#c84b1a] transition-colors"
          />
        </div>

        <div className="mt-auto border-t border-[rgba(255,255,255,0.07)] pt-4 -mx-[18px] px-[18px]">
          <button
            type="submit"
            className="w-full py-2.5 bg-[#c84b1a] text-white font-mono text-[11px] font-medium border-none cursor-pointer rounded-[2px] transition-all duration-150 hover:bg-[#e05a22]"
          >
            ▷ Ejecutar prueba
          </button>
          <div className="mt-2.5 bg-[#141920] border border-[rgba(255,255,255,0.07)] rounded-[2px] p-3 font-mono text-[10px] text-[#4ade80] leading-[1.7]">
            {`✓ Resultado (1.2s · 312 tok)\n{\n  "rut": "76.123.456-7",\n  "monto": 2450000,\n  "fecha": "2026-04-17",\n  "numero": "4821"\n}`}
          </div>
        </div>
      </form>
    </div>
  );
}
