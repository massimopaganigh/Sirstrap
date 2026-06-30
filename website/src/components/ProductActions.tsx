import { Icon } from "@/components/icons/Icon";

const ACTION_BASE =
  "inline-flex items-center gap-2 border px-6 py-3 font-body text-[0.82rem] font-semibold tracking-[0.03em] rounded-[5px] transition-all duration-300 whitespace-nowrap";

interface ProductActionsProps {
  downloadHref: string;
  sourceHref: string;
  accentBorderClassName: string;
}

export function ProductActions({ downloadHref, sourceHref, accentBorderClassName }: ProductActionsProps) {
  return (
    <>
      <a
        href={downloadHref}
        target="_blank"
        rel="noopener noreferrer"
        className={`${ACTION_BASE} ${accentBorderClassName} text-foreground`}
      >
        <Icon name="download" />
        <span>Download</span>
      </a>
      <a
        href={sourceHref}
        target="_blank"
        rel="noopener noreferrer"
        className={`${ACTION_BASE} border-border/40 hover:border-border text-muted-foreground hover:text-foreground`}
      >
        <Icon name="source" />
        <span>Source</span>
      </a>
    </>
  );
}
