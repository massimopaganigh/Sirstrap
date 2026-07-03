import { transition } from "@/config/motion";
import type { Accent } from "@/config/accents";
import type { ProductScreenshot } from "@/domain/product";

const PAN_DURATION = 2400;

interface ProductScreenshotsProps {
  screenshots: ProductScreenshot[];
  accent: Accent;
  active: boolean;
}

export function ProductScreenshots({ screenshots, accent, active }: ProductScreenshotsProps) {
  if (screenshots.length === 0) return null;

  return (
    <div className="pointer-events-none flex w-full max-w-xs select-none flex-col gap-2">
      <span className="inline-flex items-center gap-2 font-body text-[0.65rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
        <span className="h-2 w-3 border-l border-t border-border/70" />
        preview
      </span>

      {screenshots.map(screenshot => (
        <div
          key={screenshot.src}
          className="relative h-28 overflow-hidden rounded-lg border bg-background"
          style={{
            borderColor: active ? `hsl(var(--glow-${accent}) / 0.45)` : "hsl(var(--border) / 0.8)",
            boxShadow: active
              ? `0 0 24px -8px hsl(var(--glow-${accent}) / 0.35), 0 12px 32px -16px hsl(197 100% 2% / 0.9)`
              : "0 12px 32px -16px hsl(197 100% 2% / 0.9)",
            transition: transition(["border-color", "box-shadow"]),
          }}
        >
          <img
            src={screenshot.src}
            alt={screenshot.alt}
            className="absolute inset-0 h-full w-full object-cover"
            style={{
              objectPosition: active ? "100% 100%" : "0% 0%",
              transition: transition(["object-position"], PAN_DURATION),
            }}
          />
        </div>
      ))}
    </div>
  );
}
