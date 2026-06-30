import { REPO } from "@/config/site.config";
import { PANEL_BACKGROUND } from "@/config/theme";
import { transition } from "@/config/motion";
import { ACCENT_BORDER, ACCENT_LINE, ACCENT_TEXT } from "@/config/accents";
import type { Product } from "@/domain/product";
import type { Contributor } from "@/services/contributor-repository";
import WavyBackground from "@/components/WavyBackground";
import { ProductTitle } from "@/components/title/ProductTitle";
import { ProductByline } from "@/components/ProductByline";
import { ProductBadges } from "@/components/ProductBadges";
import { ProductActions } from "@/components/ProductActions";

interface ProductPanelProps {
  product: Product;
  index: number;
  isMobile: boolean;
  active: boolean;
  hasActive: boolean;
  flex: number;
  opacity: number;
  contributors: Contributor[];
  releasesLoading: boolean;
  contributorsLoading: boolean;
  downloadCount?: number;
  version?: string;
  mostPopular: boolean;
  onEnter: () => void;
  onLeave: () => void;
  onClick: () => void;
}

export function ProductPanel({
  product,
  index,
  isMobile,
  active,
  hasActive,
  flex,
  opacity,
  contributors,
  releasesLoading,
  contributorsLoading,
  downloadCount,
  version,
  mostPopular,
  onEnter,
  onLeave,
  onClick,
}: ProductPanelProps) {
  const downloadHref = product.downloadUrl ?? `${REPO}/releases/latest/download/${product.asset}`;
  const dimmed = !isMobile && hasActive && !active;

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
      <div className={`absolute inset-x-0 top-0 h-[2px] ${ACCENT_LINE[product.accent]} opacity-60`} />

      <WavyBackground accent={product.accent} active={active} />

      {!isMobile && (
        <div
          className="pointer-events-none absolute inset-y-0 right-0 w-16 z-10"
          style={{
            backgroundImage: `linear-gradient(to right, transparent, ${PANEL_BACKGROUND})`,
            opacity: dimmed ? 1 : 0,
            transition: transition(["opacity"]),
          }}
        />
      )}

      <div className="relative flex-1 min-h-0 overflow-hidden min-w-0 flex flex-col justify-center">
        <div
          className="min-w-0 flex flex-col gap-4 animate-in fade-in-0 slide-in-from-bottom-3 duration-500"
          style={{ animationDelay: `${index * 80}ms`, animationFillMode: "both" }}
        >
          <span className="block font-body text-xs font-medium uppercase tracking-[0.12em] text-muted-foreground whitespace-nowrap">
            0{index + 1}
          </span>

          {isMobile && product.core && (
            <span className="-mt-2 inline-flex w-fit items-center gap-2 font-body text-[0.65rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
              <span className="h-2 w-3 border-l border-t border-border/70" />
              powered by Sirstrap.Core
            </span>
          )}

          <div className="grid gap-x-3" style={{ gridTemplateColumns: "auto minmax(0, 1fr)" }}>
            <img src={product.icon} alt="" className="h-8 w-8 lg:h-9 lg:w-9 shrink-0 row-span-2 self-center" />
            <ProductTitle animation={product.title} active={active} accentClassName={ACCENT_TEXT[product.accent]} />
            <ProductByline accent={product.accent} contributors={contributors} loading={contributorsLoading} />
          </div>

          <ProductBadges
            downloadCount={downloadCount}
            version={version}
            mostPopular={mostPopular}
            recommended={product.recommended}
            loading={releasesLoading}
          />

          {product.description && (
            <p className="font-body text-[0.85rem] leading-[1.55] text-muted-foreground max-w-xs">
              {product.description}
            </p>
          )}
        </div>
      </div>

      <div
        className="flex-shrink-0 flex flex-col gap-4 overflow-hidden"
        style={{
          maxHeight: active ? "200px" : "0",
          marginTop: active ? "2rem" : "0",
          opacity: active ? 1 : 0,
          transition: transition(["max-height", "margin-top", "opacity"]),
          pointerEvents: active ? "auto" : "none",
        }}
      >
        <ProductActions
          downloadHref={downloadHref}
          sourceHref={product.source}
          accentBorderClassName={ACCENT_BORDER[product.accent]}
        />
      </div>
    </section>
  );
}
