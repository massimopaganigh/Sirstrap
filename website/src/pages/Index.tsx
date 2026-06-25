import { useState, useEffect, useRef } from "react";
import { Megaphone } from "lucide-react";
import WavyBackground from "@/components/WavyBackground";
import { useIsMobile } from "@/hooks/use-mobile";
import iconCleaner from "../../../src/SirHurt.Cleaner.CLI/Assets/favicon.ico";
import iconCli from "../../../src/Sirstrap.CLI/Assets/favicon.ico";
import iconUi from "../../../src/Sirstrap.UI/Assets/favicon.ico";
import iconKnee from "@/assets/kneesurgery.ico";

const REPO = "https://github.com/massimopaganigh/Sirstrap";
const MAIN_REPO = "massimopaganigh/Sirstrap";
const ANNOUNCEMENT_URL = "https://raw.githubusercontent.com/massimopaganigh/Sirstrap/main/announcements.txt";

type ExternalDownload = { repo: string; asset?: string; assetPrefix?: string };

type Product = {
  name: string;
  description: string;
  asset: string;
  variants: string[];
  externalDownloads: ExternalDownload[];
  recommended: boolean;
  icon: string;
  source: string;
  accent: "blue" | "green" | "purple" | "red";
  repo?: string;
  downloadUrl?: string;
};

const products: Product[] = [
  {
    name: "SirHurt.Cleaner.CLI",
    description: "A complete cleanup utility that wipes Roblox, SirHurt, and Sirstrap from your filesystem and registry — built by exploiters, for exploiters.",
    asset: "SirHurt.Cleaner.CLI.zip",
    variants: ["SirHurt.Cleaner.CLI_fat.zip", "SirHurt.Cleaner.CLI.cab", "SirHurt.Cleaner.CLI_fat.cab"],
    externalDownloads: [{ repo: "massimopaganigh/sirhurt.cleaner", asset: "SirHurt.Cleaner.exe" }],
    recommended: false,
    icon: iconCleaner,
    source: "https://github.com/massimopaganigh/Sirstrap/tree/main/src/SirHurt.Cleaner.CLI",
    accent: "blue",
  },
  {
    name: "Sirstrap.CLI",
    description:
      "An alternative Roblox bootstrapper CLI packed with additional features — built by exploiters, for exploiters.",
    asset: "Sirstrap.CLI.zip",
    variants: ["Sirstrap.CLI_fat.zip", "Sirstrap.CLI.cab", "Sirstrap.CLI_fat.cab", "Sirstrap.exe"],
    externalDownloads: [],
    recommended: false,
    icon: iconCli,
    source: "https://github.com/massimopaganigh/Sirstrap/tree/main/src/Sirstrap.CLI",
    accent: "green",
  },
  {
    name: "Sirstrap.UI",
    description:
      "An alternative Roblox bootstrapper UI packed with additional features — built by exploiters, for exploiters.",
    asset: "Sirstrap.UI.zip",
    variants: ["Sirstrap.UI_fat.zip", "Sirstrap.UI.cab", "Sirstrap.UI_fat.cab"],
    externalDownloads: [],
    recommended: true,
    icon: iconUi,
    source: "https://github.com/massimopaganigh/Sirstrap/tree/main/src/Sirstrap.UI",
    accent: "purple",
  },
  {
    name: "KneeSurgery",
    description:
      "A DLL for building custom UIs for SirHurt — built by exploiters, for exploiters.",
    asset: "KneeSurgery",
    variants: [],
    externalDownloads: [{ repo: "massimopaganigh/KneeSurgery", assetPrefix: "KneeSurgery_" }],
    recommended: false,
    icon: iconKnee,
    source: "https://github.com/massimopaganigh/KneeSurgery",
    accent: "red",
    repo: "massimopaganigh/KneeSurgery",
    downloadUrl: "https://github.com/massimopaganigh/KneeSurgery/releases/latest",
  },
];

const accentLine: Record<string, string> = {
  blue: "bg-glow-blue",
  green: "bg-glow-green",
  purple: "bg-glow-purple",
  red: "bg-glow-red",
};

const accentText: Record<string, string> = {
  blue: "text-glow-blue",
  green: "text-glow-green",
  purple: "text-glow-purple",
  red: "text-glow-red",
};

const accentBorder: Record<string, string> = {
  blue: "border-glow-blue/40 hover:border-glow-blue",
  green: "border-glow-green/40 hover:border-glow-green",
  purple: "border-glow-purple/40 hover:border-glow-purple",
  red: "border-glow-red/40 hover:border-glow-red",
};


const useTypewriter = (text: string, enabled: boolean) => {
  const [displayed, setDisplayed] = useState(text);
  const [resetKey, setResetKey] = useState(`${text}|${enabled}`);
  const phase = useRef<'typing' | 'wait' | 'deleting' | 'wait2'>('wait');

  // Reset state during render when text/enabled change, instead of in the effect.
  const key = `${text}|${enabled}`;
  if (key !== resetKey) {
    setResetKey(key);
    setDisplayed(text);
  }

  useEffect(() => {
    if (!enabled) {
      return;
    }

    phase.current = 'wait';

    let t: ReturnType<typeof setTimeout>;

    const tick = () => {
      if (phase.current === 'typing') {
        setDisplayed(prev => {
          const next = text.slice(0, prev.length + 1);
          if (next === text) {
            phase.current = 'wait';
            t = setTimeout(tick, 5000);
          } else {
            t = setTimeout(tick, 125 + Math.random() * 25);
          }
          return next;
        });
      } else if (phase.current === 'wait') {
        phase.current = 'deleting';
        t = setTimeout(tick, 0);
      } else if (phase.current === 'deleting') {
        setDisplayed(prev => {
          const next = prev.slice(0, -1);
          if (next === '') {
            phase.current = 'wait2';
            t = setTimeout(tick, 2500);
          } else {
            t = setTimeout(tick, 25 + Math.random() * 25);
          }
          return next;
        });
      } else {
        phase.current = 'typing';
        t = setTimeout(tick, 0);
      }
    };

    t = setTimeout(tick, 5000);
    return () => clearTimeout(t);
  }, [enabled, text]);

  return displayed;
};

const TypewriterSpan = ({ text, className, enabled }: { text: string; className?: string; enabled: boolean }) => {
  const displayed = useTypewriter(text, enabled);
  return (
    <span className={className}>
      {displayed}{enabled && <span className="title-caret">▌</span>}
    </span>
  );
};

const useRapidEee = (count: number, enabled: boolean) => {
  const [n, setN] = useState(0);
  const [resetKey, setResetKey] = useState(`${count}|${enabled}`);

  // Reset state during render when count/enabled change, instead of in the effect.
  const key = `${count}|${enabled}`;
  if (key !== resetKey) {
    setResetKey(key);
    setN(0);
  }

  useEffect(() => {
    if (!enabled) return;

    let cancelled = false;
    let t: ReturnType<typeof setTimeout>;

    const wait = (ms: number) =>
      new Promise<void>(resolve => {
        t = setTimeout(resolve, ms);
      });

    const cycle = async () => {
      await wait(5000);
      while (!cancelled) {
        for (let i = 1; i <= count; i++) {
          if (cancelled) return;
          setN(i);
          await wait(30 + Math.random() * 25);
        }
        await wait(5000);
        for (let i = count - 1; i >= 0; i--) {
          if (cancelled) return;
          setN(i);
          await wait(25 + Math.random() * 25);
        }
        await wait(2500);
      }
    };
    cycle();

    return () => {
      cancelled = true;
      clearTimeout(t);
    };
  }, [count, enabled]);

  return 'e'.repeat(n);
};

const RapidEeeSpan = ({ className, enabled }: { className?: string; enabled: boolean }) => {
  const eee = useRapidEee(15, enabled);
  return <span className={className}>{eee}</span>;
};

const FadingTitle = ({ children }: { children: React.ReactNode }) => {
  const ref = useRef<HTMLDivElement>(null);
  const [overflows, setOverflows] = useState(false);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    const check = () => setOverflows(el.scrollWidth > el.clientWidth);
    check();
    const ro = new ResizeObserver(check);
    ro.observe(el);
    return () => ro.disconnect();
  }, []);

  return (
    <div ref={ref} className="relative overflow-hidden min-w-0">
      {children}
      <div
        className="absolute inset-y-0 right-0 w-14 pointer-events-none"
        style={{
          background: 'linear-gradient(to right, transparent, hsl(220 20% 6%))',
          opacity: overflows ? 1 : 0,
          transition: 'opacity 0.5s ease',
        }}
      />
    </div>
  );
};

const iconProps = {
  fill: "none" as const,
  viewBox: "0 0 24 24",
  stroke: "currentColor" as const,
  strokeWidth: 2,
  className: "h-3.5 w-3.5 shrink-0",
};

const IconDownload = () => (
  <svg {...iconProps}>
    <path strokeLinecap="round" strokeLinejoin="round" d="M12 3v12m0 0l-4-4m4 4l4-4M4 17v2a1 1 0 001 1h14a1 1 0 001-1v-2" />
  </svg>
);

const IconSource = () => (
  <svg {...iconProps}>
    <path strokeLinecap="round" strokeLinejoin="round" d="M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4" />
  </svg>
);

const IconDownloadCount = () => (
  <svg {...iconProps}>
    <path strokeLinecap="round" strokeLinejoin="round" d="M12 3v12m0 0l-4-4m4 4l4-4M4 17v2a1 1 0 001 1h14a1 1 0 001-1v-2" />
  </svg>
);

const IconStar = () => (
  <svg {...iconProps}>
    <path strokeLinecap="round" strokeLinejoin="round" d="M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.197-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118l-3.976-2.888c-.784-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.674z" />
  </svg>
);

const IconRecommended = () => (
  <svg {...iconProps}>
    <path strokeLinecap="round" strokeLinejoin="round" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
  </svg>
);

const badgeClass =
  "inline-flex items-center gap-1 self-start border border-border/60 bg-background/80 backdrop-blur-sm px-[0.55rem] py-[0.2rem] font-body text-[0.7rem] font-semibold uppercase tracking-[0.08em] text-muted-foreground rounded-[3px] whitespace-nowrap";

const Index = () => {
  const isMobile = useIsMobile();
  const [active, setActive] = useState<number | null>(null);
  const [announcement, setAnnouncement] = useState<string | null>(null);
  const [versions, setVersions] = useState<Record<string, string>>({});
  const [downloads, setDownloads] = useState<Record<string, number>>({});

  useEffect(() => {
    let cancelled = false;

    fetch(ANNOUNCEMENT_URL)
      .then(response => response.ok ? response.text() : "")
      .then(text => {
        const trimmed = text.trim();
        if (!cancelled && trimmed) {
          setAnnouncement(trimmed);
        }
      })
      .catch(() => {});

    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    const fetchAllReleases = async (repo: string) => {
      const all: unknown[] = [];
      let url: string | null = `https://api.github.com/repos/${repo}/releases?per_page=100`;
      while (url) {
        const res = await fetch(url);
        const page = await res.json();
        if (Array.isArray(page)) all.push(...page);
        const link = res.headers.get("Link") ?? "";
        const next = link.match(/<([^>]+)>;\s*rel="next"/);
        url = next ? next[1] : null;
      }
      return all;
    };

    const productRepos = [...new Set(products.map(p => p.repo ?? MAIN_REPO))];
    const externalRepos = [...new Set(products.flatMap(p => p.externalDownloads.map(e => e.repo)))];

    Promise.all([
      Promise.all(
        productRepos.map(repo =>
          fetch(`https://api.github.com/repos/${repo}/releases/latest`)
            .then(r => r.json())
            .then(j => [repo, j.tag_name as string | undefined] as const)
            .catch(() => [repo, undefined] as const)
        )
      ),
      fetchAllReleases(MAIN_REPO),
      ...externalRepos.map(repo => fetchAllReleases(repo).then(releases => ({ repo, releases }))),
    ])
      .then(([versionEntries, all, ...externalResults]) => {
        const versionMap: Record<string, string> = {};
        for (const [repo, tag] of versionEntries) {
          if (tag) versionMap[repo] = tag;
        }
        setVersions(versionMap);

        const raw: Record<string, number> = {};
        for (const release of all as { assets?: { name: string; download_count: number }[] }[]) {
          if (!Array.isArray(release.assets)) continue;
          for (const a of release.assets) {
            raw[a.name] = (raw[a.name] ?? 0) + a.download_count;
          }
        }

        const externalCounts: Record<string, Record<string, number>> = {};
        for (const ext of externalResults as { repo: string; releases: { assets?: { name: string; download_count: number }[] }[] }[]) {
          externalCounts[ext.repo] = {};
          for (const release of ext.releases) {
            if (!Array.isArray(release.assets)) continue;
            for (const a of release.assets) {
              externalCounts[ext.repo][a.name] = (externalCounts[ext.repo][a.name] ?? 0) + a.download_count;
            }
          }
        }

        const map: Record<string, number> = { ...raw };
        for (const p of products) {
          for (const variant of p.variants) {
            if (raw[variant]) {
              map[p.asset] = (map[p.asset] ?? 0) + raw[variant];
              delete map[variant];
            }
          }
          for (const ext of p.externalDownloads) {
            const repoAssets = externalCounts[ext.repo] ?? {};
            if (ext.assetPrefix) {
              for (const [name, count] of Object.entries(repoAssets)) {
                if (name.startsWith(ext.assetPrefix)) {
                  map[p.asset] = (map[p.asset] ?? 0) + count;
                }
              }
            } else if (ext.asset) {
              const count = repoAssets[ext.asset] ?? 0;
              if (count > 0) {
                map[p.asset] = (map[p.asset] ?? 0) + count;
              }
            }
          }
        }
        setDownloads(map);
      })
      .catch(() => {});
  }, []);

  const mostPopularAsset = Object.keys(downloads).length > 0
    ? products.reduce((best, p) =>
        (downloads[p.asset] ?? 0) > (downloads[best.asset] ?? 0) ? p : best
      ).asset
    : null;

  const getFlex = (i: number) => {
    if (active === null) return 1;
    return active === i ? 2 : 0.5;
  };

  const handleMouseEnter = (i: number) => { if (!isMobile) setActive(i); };
  const handleMouseLeave = () => { if (!isMobile) setActive(null); };
  const handleClick = (i: number) => { if (isMobile) setActive(prev => prev === i ? null : i); };

  return (
    <div className="relative flex flex-col md:flex-row h-screen w-screen overflow-hidden">
      {announcement && (
        <div
          aria-label="Announcement"
          aria-live="polite"
          role="status"
          className="pointer-events-none absolute inset-x-0 bottom-0 z-30 flex justify-center md:left-1/2 md:right-auto md:w-[42rem] md:max-w-[calc(100vw-2rem)] md:-translate-x-1/2"
        >
          <div className="flex h-8 w-full min-w-0 items-center justify-center gap-2 overflow-hidden rounded-t-[5px] rounded-b-none border border-b-0 border-glow-purple/40 bg-background/75 px-3 font-body text-[0.75rem] leading-none text-muted-foreground shadow-[0_10px_30px_rgba(0,0,0,0.28)] backdrop-blur-md">
            <Megaphone className="h-3.5 w-3.5 shrink-0 text-glow-purple" aria-hidden="true" />
            <span className="truncate">{announcement}</span>
          </div>
        </div>
      )}
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
          className="relative flex flex-col border-b border-border last:border-b-0 md:border-b-0 md:border-r md:last:border-r-0 bg-background px-8 py-12 lg:px-12 overflow-hidden"
        >
          <div className={`absolute inset-x-0 top-0 h-[2px] ${accentLine[p.accent]} opacity-60`} />

          <WavyBackground accent={p.accent} active={active === i} />

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

          <div className="relative flex-1 min-h-0 overflow-hidden min-w-0">
            <div
              className="pointer-events-none absolute inset-x-0 bottom-0 h-12 z-10"
              style={{
                background: "linear-gradient(to bottom, transparent, hsl(220 20% 6%))",
                opacity: active === i ? 1 : 0,
                transition: "opacity 0.5s ease",
              }}
            />
            <div className="min-w-0 flex flex-col gap-4">
              <span className="block font-body text-xs font-medium uppercase tracking-[0.12em] text-muted-foreground whitespace-nowrap">
                0{i + 1}
              </span>

              <div className="grid gap-x-3" style={{ gridTemplateColumns: 'auto minmax(0, 1fr)' }}>
                <img src={p.icon} alt="" className="h-8 w-8 lg:h-9 lg:w-9 shrink-0 row-span-2 self-center" />
                <FadingTitle>
                  {p.accent === 'purple' && p.name.endsWith('.UI') ? (
                    <h2 className={`font-display text-2xl font-extrabold tracking-[-0.035em] lg:text-3xl whitespace-nowrap ${accentText[p.accent]}`}>
                      {p.name.slice(0, -3)}
                      <span className={`font-display text-2xl font-extrabold tracking-[-0.035em] lg:text-3xl ${active === i ? 'title-shimmer' : accentText[p.accent]}`}>.UI</span>
                    </h2>
                  ) : p.name.endsWith('.CLI') ? (
                    <h2 className={`font-display text-2xl font-extrabold tracking-[-0.035em] lg:text-3xl whitespace-nowrap ${accentText[p.accent]}`}>
                      {p.name.slice(0, -4)}
                      <TypewriterSpan text=".CLI" className={`font-display text-2xl font-extrabold tracking-[-0.035em] lg:text-3xl ${accentText[p.accent]}`} enabled={active === i} />
                    </h2>
                  ) : p.name === 'KneeSurgery' ? (
                    <h2 className={`font-display text-2xl font-extrabold tracking-[-0.035em] lg:text-3xl whitespace-nowrap ${accentText[p.accent]}`}>
                      Knee<RapidEeeSpan className={`font-display text-2xl font-extrabold tracking-[-0.035em] lg:text-3xl ${accentText[p.accent]}`} enabled={active === i} />Surgery
                    </h2>
                  ) : (
                    <h2 className={`font-display text-2xl font-extrabold tracking-[-0.035em] lg:text-3xl whitespace-nowrap ${accentText[p.accent]}`}>
                      {p.name}
                    </h2>
                  )}
                </FadingTitle>
                <FadingTitle>
                  <a
                    href="https://github.com/massimopaganigh"
                    target="_blank"
                    rel="noopener noreferrer"
                    className="flex items-center gap-1.5 whitespace-nowrap"
                    onClick={e => e.stopPropagation()}
                  >
                    <span className="font-body text-[0.85rem] leading-[1.55] text-muted-foreground">made by ギャップ</span>
                    <img
                      src="https://github.com/massimopaganigh.png"
                      alt="massimopaganigh"
                      className="h-5 w-5 rounded-full border border-border/60 hover:border-border transition-colors duration-200"
                    />
                  </a>
                </FadingTitle>
              </div>

              <div className="flex flex-wrap items-center gap-2">
                {downloads[p.asset] != null && (
                  <span className={badgeClass}>
                    <IconDownloadCount />
                    {downloads[p.asset].toLocaleString()}
                  </span>
                )}
                {versions[p.repo ?? MAIN_REPO] && (
                  <span className={badgeClass}>
                    {versions[p.repo ?? MAIN_REPO]}
                  </span>
                )}
                {mostPopularAsset === p.asset && (
                  <span className="inline-flex items-center gap-1 self-start border border-amber-500/40 bg-amber-500/10 backdrop-blur-sm px-[0.55rem] py-[0.2rem] font-body text-[0.7rem] font-semibold uppercase tracking-[0.08em] text-amber-400 rounded-[3px] whitespace-nowrap">
                    <IconStar />
                    Most popular
                  </span>
                )}
                {p.recommended && (
                  <span className="inline-flex items-center gap-1 self-start border border-emerald-500/40 bg-emerald-500/10 backdrop-blur-sm px-[0.55rem] py-[0.2rem] font-body text-[0.7rem] font-semibold uppercase tracking-[0.08em] text-emerald-400 rounded-[3px] whitespace-nowrap">
                    <IconRecommended />
                    Recommended
                  </span>
                )}
              </div>

              {p.description && (
                <p className="font-body text-[0.85rem] leading-[1.55] text-muted-foreground max-w-xs">
                  {p.description}
                </p>
              )}
            </div>
          </div>

          <div
            className="flex-shrink-0 flex flex-col gap-4 overflow-hidden"
            style={{
              maxHeight: active === i ? "200px" : "0",
              marginTop: active === i ? "2rem" : "0",
              opacity: active === i ? 1 : 0,
              transition: "max-height 0.5s cubic-bezier(0.4,0,0.2,1), margin-top 0.5s ease, opacity 0.5s ease",
              pointerEvents: active === i ? "auto" : "none",
            }}
          >
            <a
              href={p.downloadUrl ?? `${REPO}/releases/latest/download/${p.asset}`}
              target="_blank"
              rel="noopener noreferrer"
              className={`inline-flex items-center gap-2 border px-6 py-3 font-body text-[0.82rem] font-semibold tracking-[0.03em] rounded-[5px] transition-all duration-300 whitespace-nowrap ${accentBorder[p.accent]} text-foreground`}
            >
              <IconDownload />
              <span>Download</span>
            </a>
            <a
              href={p.source}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-2 border border-border/40 hover:border-border px-6 py-3 font-body text-[0.82rem] font-semibold tracking-[0.03em] rounded-[5px] transition-all duration-300 whitespace-nowrap text-muted-foreground hover:text-foreground"
            >
              <IconSource />
              <span>Source</span>
            </a>
          </div>
        </section>
      ))}
    </div>
  );
};

export default Index;
