import { transition } from "@/config/motion";

interface CoreBracketProps {
  count: number;
  total: number;
  dimmed: boolean;
}

export function CoreBracket({ count, total, dimmed }: CoreBracketProps) {
  if (count < 2 || count >= total) return null;

  return (
    <div
      className="pointer-events-none absolute left-0 top-0 z-20 hidden md:block"
      style={{ width: `${(count / total) * 100}%`, opacity: dimmed ? 0 : 1, transition: transition(["opacity"]) }}
    >
      <div className="relative mx-8 mt-6 h-2 border-x border-t border-border/70">
        <div className="absolute left-1/2 top-0 -translate-x-1/2 -translate-y-1/2 bg-background px-2.5">
          <span className="whitespace-nowrap font-body text-[0.68rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
            powered by <span className="text-foreground/90">Sirstrap.Core</span>
          </span>
        </div>
      </div>
    </div>
  );
}
