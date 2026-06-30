export type Accent = "red" | "teal" | "amber" | "mint";

export const ACCENT_LINE: Record<Accent, string> = {
  red: "bg-glow-red",
  teal: "bg-glow-teal",
  amber: "bg-glow-amber",
  mint: "bg-glow-mint",
};

export const ACCENT_TEXT: Record<Accent, string> = {
  red: "text-glow-red",
  teal: "text-glow-teal",
  amber: "text-glow-amber",
  mint: "text-glow-mint",
};

export const ACCENT_BORDER: Record<Accent, string> = {
  red: "border-glow-red/40 hover:border-glow-red",
  teal: "border-glow-teal/40 hover:border-glow-teal",
  amber: "border-glow-amber/40 hover:border-glow-amber",
  mint: "border-glow-mint/40 hover:border-glow-mint",
};

export const ACCENT_WAVE_HSL: Record<Accent, [number, number, number]> = {
  red: [5, 81, 48],
  teal: [181, 80, 44],
  amber: [39, 100, 52],
  mint: [160, 45, 72],
};

export const ACCENT_TEXT_HOVER: Record<Accent, string> = {
  red: "group-hover:text-glow-red",
  teal: "group-hover:text-glow-teal",
  amber: "group-hover:text-glow-amber",
  mint: "group-hover:text-glow-mint",
};

export const ACCENT_BORDER_HOVER: Record<Accent, string> = {
  red: "group-hover:border-glow-red group-hover:shadow-[0_0_16px_-4px_hsl(var(--glow-red))]",
  teal: "group-hover:border-glow-teal group-hover:shadow-[0_0_16px_-4px_hsl(var(--glow-teal))]",
  amber: "group-hover:border-glow-amber group-hover:shadow-[0_0_16px_-4px_hsl(var(--glow-amber))]",
  mint: "group-hover:border-glow-mint group-hover:shadow-[0_0_16px_-4px_hsl(var(--glow-mint))]",
};
