import { useEffect, useState } from "react";
import { TITLE_TIMING, jitter } from "@/config/motion";

export function useRapidEee(count: number, enabled: boolean): number {
  const [value, setValue] = useState(0);
  const [resetKey, setResetKey] = useState(`${count}|${enabled}`);

  const key = `${count}|${enabled}`;
  if (key !== resetKey) {
    setResetKey(key);
    setValue(0);
  }

  useEffect(() => {
    if (!enabled) return;

    let cancelled = false;
    let timer: ReturnType<typeof setTimeout>;

    const wait = (ms: number) =>
      new Promise<void>(resolve => {
        timer = setTimeout(resolve, ms);
      });

    const cycle = async () => {
      await wait(TITLE_TIMING.startDelay);
      while (!cancelled) {
        for (let i = 1; i <= count; i++) {
          if (cancelled) return;
          setValue(i);
          await wait(jitter(TITLE_TIMING.rapid));
        }
        await wait(TITLE_TIMING.holdFull);
        for (let i = count - 1; i >= 0; i--) {
          if (cancelled) return;
          setValue(i);
          await wait(jitter(TITLE_TIMING.erase));
        }
        await wait(TITLE_TIMING.holdEmpty);
      }
    };
    cycle();

    return () => {
      cancelled = true;
      clearTimeout(timer);
    };
  }, [count, enabled]);

  return value;
}
