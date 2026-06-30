import { useState } from "react";

export interface PanelInteraction {
  hasActive: boolean;
  isActive(index: number): boolean;
  flexOf(index: number): number;
  opacityOf(index: number): number;
  open(index: number): void;
  close(): void;
  toggle(index: number): void;
}

export function usePanelInteraction(isMobile: boolean): PanelInteraction {
  const [active, setActive] = useState<number | null>(null);

  return {
    hasActive: active !== null,
    isActive: index => active === index,
    flexOf: index => (active === null ? 1 : active === index ? 2 : 0.5),
    opacityOf: index => (active === null ? 0.7 : active === index ? 1 : 0.5),
    open: index => {
      if (!isMobile) setActive(index);
    },
    close: () => {
      if (!isMobile) setActive(null);
    },
    toggle: index => {
      if (isMobile) setActive(prev => (prev === index ? null : index));
    },
  };
}
