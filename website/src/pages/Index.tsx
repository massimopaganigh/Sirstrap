import { useState } from "react";
import WavyBackground from "@/components/WavyBackground";

const REPO = "https://github.com/massimopaganigh/Sirstrap";

const products = [
  {
    name: "SirHurt.Cleaner.CLI",
    description: "",
    asset: "SirHurt.Cleaner.CLI.zip",
    source: "https://github.com/massimopaganigh/Sirstrap/tree/main/src/SirHurt.Cleaner.CLI",
    accent: "blue" as const,
  },
  {
    name: "Sirstrap.CLI",
    description:
      "An alternative Roblox bootstrapper CLI packed with additional features — built by exploiters, for exploiters.",
    asset: "Sirstrap.CLI.zip",
    source: "https://github.com/massimopaganigh/Sirstrap/tree/main/src/Sirstrap.CLI",
    accent: "green" as const,
  },
  {
    name: "Sirstrap.UI",
    description:
      "An alternative Roblox bootstrapper UI packed with additional features — built by exploiters, for exploiters.",
    asset: "Sirstrap.UI.zip",
    source: "https://github.com/massimopaganigh/Sirstrap/tree/main/src/Sirstrap.UI",
    accent: "purple" as const,
  },
];

const accentLine: Record<string, string> = {
  blue: "bg-glow-blue",
  green: "bg-glow-green",
  purple: "bg-glow-purple",
};

const accentText: Record<string, string> = {
  blue: "text-glow-blue",
  green: "text-glow-green",
  purple: "text-glow-purple",
};

const accentBorder: Record<string, string> = {
  blue: "border-glow-blue/40 hover:border-glow-blue",
  green: "border-glow-green/40 hover:border-glow-green",
  purple: "border-glow-purple/40 hover:border-glow-purple",
};

const Index = () => {
  const [hovered, setHovered] = useState<number | null>(null);

  const getFlex = (i: number) => {
    if (hovered === null) return 1;
    return hovered === i ? 2 : 0.5;
  };

  const isCollapsed = (i: number) => hovered !== null && hovered !== i;

  return (
    <div className="flex h-screen w-screen overflow-hidden">
      {products.map((p, i) => (
        <section
          key={p.name}
          onMouseEnter={() => setHovered(i)}
          onMouseLeave={() => setHovered(null)}
          style={{
            flex: getFlex(i),
            opacity: isCollapsed(i) ? 0.5 : 1,
            transition: "flex 0.5s cubic-bezier(0.4, 0, 0.2, 1), opacity 0.5s ease",
          }}
          className="relative flex flex-col justify-between border-r border-border last:border-r-0 bg-background px-8 py-12 lg:px-12 overflow-hidden"
        >
          {/* Top accent line */}
          <div className={`absolute inset-x-0 top-0 h-[2px] ${accentLine[p.accent]} opacity-60`} />

          {/* Animated wavy lines background */}
          <WavyBackground accent={p.accent} active={hovered === i} />

          {/* Fade overlay on right edge for collapsed panels */}
          <div
            className="pointer-events-none absolute inset-y-0 right-0 w-16 z-10"
            style={{
              background: isCollapsed(i)
                ? "linear-gradient(to right, transparent, hsl(220 20% 6%))"
                : "none",
              transition: "background 0.5s ease",
            }}
          />

          {/* Content — no text resize, clip with fade */}
          <div className="min-w-0">
            <span className="mb-6 block font-body text-xs font-medium uppercase tracking-[0.12em] text-muted-foreground whitespace-nowrap">
              0{i + 1}
            </span>

            <h2 className={`mb-4 font-display text-2xl font-extrabold tracking-[-0.035em] lg:text-3xl whitespace-nowrap ${accentText[p.accent]}`}>
              {p.name}
            </h2>

            {p.description && (
              <p className="font-body text-[0.85rem] leading-[1.55] text-muted-foreground max-w-xs">
                {p.description}
              </p>
            )}
          </div>

          {/* Download & Source buttons */}
          <div className="mt-12 flex flex-col gap-3">
            <a
              href={`${REPO}/releases/latest/download/${p.asset}`}
              target="_blank"
              rel="noopener noreferrer"
              className={`inline-flex items-center gap-2 border px-6 py-3 font-body text-[0.82rem] font-semibold tracking-[0.03em] rounded-[5px] transition-all duration-300 whitespace-nowrap ${accentBorder[p.accent]} text-foreground`}
            >
              <span>Download</span>
              <svg className="h-3.5 w-3.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M17 7l-10 10m0 0h8m-8 0V9" />
              </svg>
            </a>
            <a
              href={p.source}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-2 border border-border/40 hover:border-border px-6 py-3 font-body text-[0.82rem] font-semibold tracking-[0.03em] rounded-[5px] transition-all duration-300 whitespace-nowrap text-muted-foreground hover:text-foreground"
            >
              <span>Source</span>
              <svg className="h-3.5 w-3.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4" />
              </svg>
            </a>
          </div>
        </section>
      ))}
    </div>
  );
};

export default Index;