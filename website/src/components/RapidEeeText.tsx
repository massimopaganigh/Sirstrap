import { useRapidEee } from "@/hooks/useRapidEee";

interface RapidEeeTextProps {
  glyph: string;
  max: number;
  enabled: boolean;
  className?: string;
}

export function RapidEeeText({ glyph, max, enabled, className }: RapidEeeTextProps) {
  const count = useRapidEee(max, enabled);
  return <span className={className}>{glyph.repeat(count)}</span>;
}
