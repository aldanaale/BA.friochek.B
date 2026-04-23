"use client";

import { Handle, Position } from "@xyflow/react";
import { FlowNodeData } from "@/types";

export function TriggerNode({ data, selected }: { data: FlowNodeData; selected?: boolean }) {
  return (
    <div
      className={`bg-[rgba(99,102,241,0.07)] border rounded-[3px] px-[15px] py-[13px] min-w-[145px] cursor-pointer shadow-[0_4px_16px_rgba(0,0,0,0.3)] transition-all duration-150 ${
        selected
          ? "border-[#c84b1a] shadow-[0_0_0_1px_rgba(200,75,26,0.3),0_6px_20px_rgba(0,0,0,0.4)]"
          : "border-[rgba(99,102,241,0.4)] hover:border-white/[0.18] hover:-translate-y-px hover:shadow-[0_6px_20px_rgba(0,0,0,0.4)]"
      }`}
    >
      <div className="font-mono text-[8px] font-normal text-[rgba(129,140,248,0.8)] mb-[5px] tracking-[0.3px]">
        {data.tag}
      </div>
      <div className="font-mono text-[12px] font-semibold leading-[1.3] text-[#e8f0f8]">
        {data.title}
      </div>
      {data.sub && (
        <div className="font-mono text-[10px] text-[#4a5c70] mt-0.5">{data.sub}</div>
      )}
      {data.running && (
        <div className="flex items-center gap-[5px] font-mono text-[9px] text-[#4ade80] mt-1.5">
          <span
            className="w-[7px] h-[7px] border-[1.5px] border-[rgba(74,222,128,0.2)] border-t-[#4ade80] rounded-full"
            style={{ animation: "spin 0.7s linear infinite" }}
          />
          procesando...
        </div>
      )}
      <Handle type="source" position={Position.Right} className="!bg-[#4a5c70] !w-2 !h-2 !border-[#141920]" />
    </div>
  );
}
