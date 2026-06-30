import { useEffect, useRef, useState } from "react";
import { TITLE_TIMING, jitter } from "@/config/motion";

export function useTypewriter(text: string, enabled: boolean): string {
  const [displayed, setDisplayed] = useState(text);
  const [resetKey, setResetKey] = useState(`${text}|${enabled}`);
  const phase = useRef<"typing" | "wait" | "deleting" | "wait2">("wait");

  const key = `${text}|${enabled}`;
  if (key !== resetKey) {
    setResetKey(key);
    setDisplayed(text);
  }

  useEffect(() => {
    if (!enabled) return;

    phase.current = "wait";
    let timer: ReturnType<typeof setTimeout>;

    const tick = () => {
      if (phase.current === "typing") {
        setDisplayed(prev => {
          const next = text.slice(0, prev.length + 1);
          if (next === text) {
            phase.current = "wait";
            timer = setTimeout(tick, TITLE_TIMING.holdFull);
          } else {
            timer = setTimeout(tick, jitter(TITLE_TIMING.type));
          }
          return next;
        });
      } else if (phase.current === "wait") {
        phase.current = "deleting";
        timer = setTimeout(tick, 0);
      } else if (phase.current === "deleting") {
        setDisplayed(prev => {
          const next = prev.slice(0, -1);
          if (next === "") {
            phase.current = "wait2";
            timer = setTimeout(tick, TITLE_TIMING.holdEmpty);
          } else {
            timer = setTimeout(tick, jitter(TITLE_TIMING.erase));
          }
          return next;
        });
      } else {
        phase.current = "typing";
        timer = setTimeout(tick, 0);
      }
    };

    timer = setTimeout(tick, TITLE_TIMING.startDelay);
    return () => clearTimeout(timer);
  }, [enabled, text]);

  return displayed;
}
