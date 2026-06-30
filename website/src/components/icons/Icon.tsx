import { BadgeCheck, Code, Download, Lock, Megaphone, Star, type LucideIcon } from "lucide-react";

const ICONS = {
  download: Download,
  source: Code,
  star: Star,
  check: BadgeCheck,
  megaphone: Megaphone,
  lock: Lock,
} satisfies Record<string, LucideIcon>;

export type IconName = keyof typeof ICONS;

interface IconProps {
  name: IconName;
  className?: string;
}

export function Icon({ name, className = "h-3.5 w-3.5 shrink-0" }: IconProps) {
  const Glyph = ICONS[name];
  return <Glyph className={className} strokeWidth={2} aria-hidden="true" />;
}
