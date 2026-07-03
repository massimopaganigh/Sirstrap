import type { Accent } from "@/config/accents";

export type AssetMatcher =
  | { type: "exact"; asset: string }
  | { type: "prefix"; prefix: string };

export interface ExternalDownload {
  repo: string;
  match: AssetMatcher;
}

export type TitleAnimation =
  | { kind: "plain"; head: string }
  | { kind: "typewriter"; head: string; tail: string }
  | { kind: "shimmer"; head: string; tail: string }
  | { kind: "rapid"; head: string; tail: string; glyph: string; max: number };

export interface ProductScreenshot {
  src: string;
  alt: string;
}

export interface Product {
  name: string;
  description: string;
  asset: string;
  variants: string[];
  externalDownloads: ExternalDownload[];
  recommended: boolean;
  icon: string;
  source: string;
  accent: Accent;
  title: TitleAnimation;
  core?: boolean;
  repo?: string;
  downloadUrl?: string;
  screenshots?: ProductScreenshot[];
}
