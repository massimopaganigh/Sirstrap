import { transition } from "@/config/motion";
import { ACCENT_LINE, ACCENT_TEXT } from "@/config/accents";
import { Icon } from "@/components/icons/Icon";
import WavyBackground from "@/components/WavyBackground";

const ACCENT = "orange" as const;
const FUTURE_PROJECTS = ["Magenta", "Monsoon"];

interface FuturePanelProps {
  index: number;
  isMobile: boolean;
  active: boolean;
  flex: number;
  opacity: number;
  onEnter: () => void;
  onLeave: () => void;
  onClick: () => void;
}

export function FuturePanel({ index, isMobile, active, flex, opacity, onEnter, onLeave, onClick }: FuturePanelProps) {
  return (
    <section
      onMouseEnter={onEnter}
      onMouseLeave={onLeave}
      onClick={onClick}
      style={{
        flex: isMobile ? undefined : flex,
        opacity: isMobile ? 1 : opacity,
        transition: transition(["flex", "opacity"]),
      }}
      className="relative flex flex-col min-h-[70vh] md:min-h-0 border-b border-border last:border-b-0 md:border-b-0 md:border-r md:last:border-r-0 bg-background px-8 py-12 lg:px-12 overflow-hidden"
    >
      <div className={`absolute inset-x-0 top-0 h-[2px] ${ACCENT_LINE[ACCENT]} opacity-60`} />

      <WavyBackground accent={ACCENT} active={active} />

      <div className="relative flex-1 min-h-0 overflow-hidden min-w-0 flex flex-col justify-center">
        <div
          className="min-w-0 flex flex-col gap-4 animate-in fade-in-0 slide-in-from-bottom-3 duration-500"
          style={{ animationDelay: `${index * 80}ms`, animationFillMode: "both" }}
        >
          <span className="block font-body text-xs font-medium uppercase tracking-[0.12em] text-muted-foreground whitespace-nowrap">
            0{index + 1}
          </span>

          <h2 className={`font-display text-2xl font-extrabold tracking-[-0.035em] lg:text-3xl ${ACCENT_TEXT[ACCENT]}`}>
            Coming soon
          </h2>

          <ul className="flex flex-col gap-2.5">
            {FUTURE_PROJECTS.map(name => (
              <li
                key={name}
                className="flex items-center gap-2.5 font-display text-lg font-bold tracking-[-0.02em] text-foreground/55 select-none"
              >
                <Icon name="lock" className="h-4 w-4 shrink-0 text-muted-foreground" />
                {name}
              </li>
            ))}
            <li className="font-body text-[0.85rem] leading-[1.55] text-muted-foreground/70 select-none">…and more</li>
          </ul>
        </div>
      </div>
    </section>
  );
}
