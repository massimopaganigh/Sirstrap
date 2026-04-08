import { useState } from "react";
import WavyBackground from "@/components/WavyBackground";
import { useIsMobile } from "@/hooks/use-mobile";

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
  const isMobile = useIsMobile();
  const [active, setActive] = useState<number | null>(null);

  const getFlex = (i: number) => {
    if (active === null) return 1;
    return active === i ? 2 : 0.5;
  };

  const isCollapsed = (i: number) => active !== null && active !== i;

  const handleMouseEnter = (i: number) => { if (!isMobile) setActive(i); };
  const handleMouseLeave = () => { if (!isMobile) setActive(null); };
  const handleClick = (i: number) => { if (isMobile) setActive(prev => prev === i ? null : i); };

  return (
    <div className="flex flex-col md:flex-row h-screen w-screen overflow-hidden">
      {products.map((p, i) => (
        <section
          key={p.name}
          onMouseEnter={() => handleMouseEnter(i)}
          onMouseLeave={handleMouseLeave}
          onClick={() => handleClick(i)}
          style={{
            flex: getFlex(i),
            opacity: active === null ? 0.7 : active === i ? 1 : 0.5,
            transition: "flex 0.5s cubic-bezier(0.4, 0, 0.2, 1), opacity 0.5s ease",
          }}
          className="relative flex flex-col justify-between border-b border-border last:border-b-0 md:border-b-0 md:border-r md:last:border-r-0 bg-background px-8 py-12 lg:px-12 overflow-hidden"
        >
          {/* Top accent line */}
          <div className={`absolute inset-x-0 top-0 h-[2px] ${accentLine[p.accent]} opacity-60`} />

          {/* Animated wavy lines background */}
          <WavyBackground accent={p.accent} active={active === i} />

          {/* Fade overlay — visible on all when idle, hidden on expanded */}
          <div
            className={`pointer-events-none absolute z-10 ${isMobile ? "inset-x-0 bottom-0 h-16" : "inset-y-0 right-0 w-16"}`}
            style={{
              background: active !== i
                ? isMobile
                  ? "linear-gradient(to bottom, transparent, hsl(220 20% 6%))"
                  : "linear-gradient(to right, transparent, hsl(220 20% 6%))"
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

          {/* Download & Source buttons — visible only in expanded section */}
          <div
            className="mt-12 flex flex-col gap-3 transition-all duration-300"
            style={{
              opacity: active === i ? 1 : 0,
              transform: active === i ? "translateY(0)" : "translateY(8px)",
              pointerEvents: active === i ? "auto" : "none",
            }}
          >
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