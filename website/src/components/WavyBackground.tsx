import { useRef, useEffect } from "react";

const accentHSL: Record<string, [number, number, number]> = {
  blue: [210, 90, 55],
  green: [145, 80, 42],
  purple: [270, 60, 58],
  red: [0, 75, 55],
};

interface WavyBackgroundProps {
  accent: string;
  active: boolean;
}

const LINE_COUNT = 18;
const SPEED = 0.0006;

export default function WavyBackground({ accent, active }: WavyBackgroundProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const rafRef = useRef(0);
  const opacityRef = useRef(0);
  const timeRef = useRef(0);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    const resize = () => {
      const rect = canvas.parentElement!.getBoundingClientRect();
      canvas.width = rect.width * devicePixelRatio;
      canvas.height = rect.height * devicePixelRatio;
    };
    resize();

    const ro = new ResizeObserver(resize);
    ro.observe(canvas.parentElement!);

    let running = true;
    let lastTime = performance.now();

    const draw = (now: number) => {
      if (!running) return;

      const dt = now - lastTime;
      lastTime = now;
      timeRef.current += dt * SPEED;

      const targetOpacity = active ? 1 : 0;
      opacityRef.current += (targetOpacity - opacityRef.current) * 0.03;

      const w = canvas.width;
      const h = canvas.height;
      ctx.clearRect(0, 0, w, h);

      if (opacityRef.current < 0.005) {
        rafRef.current = requestAnimationFrame(draw);
        return;
      }

      const [hue, sat, light] = accentHSL[accent] ?? accentHSL.green;
      const t = timeRef.current;

      for (let i = 0; i < LINE_COUNT; i++) {
        const progress = i / (LINE_COUNT - 1);
        const baseY = h * 0.1 + progress * h * 0.8;
        const phase = i * 0.7 + t;
        const amplitude = h * (0.04 + progress * 0.03);

        const lineOpacity = (0.08 + progress * 0.08) * opacityRef.current;

        ctx.beginPath();
        ctx.moveTo(-10, baseY);

        for (let x = 0; x <= w + 10; x += 4) {
          const xNorm = x / w;
          const y =
            baseY +
            Math.sin(xNorm * 3 + phase) * amplitude +
            Math.sin(xNorm * 1.5 + phase * 0.7 + i * 0.3) * amplitude * 0.6;
          ctx.lineTo(x, y);
        }

        ctx.strokeStyle = `hsla(${hue}, ${sat}%, ${light}%, ${lineOpacity})`;
        ctx.lineWidth = 1.5 * devicePixelRatio;
        ctx.stroke();
      }

      rafRef.current = requestAnimationFrame(draw);
    };

    rafRef.current = requestAnimationFrame(draw);

    return () => {
      running = false;
      cancelAnimationFrame(rafRef.current);
      ro.disconnect();
    };
  }, [accent, active]);

  return (
    <canvas
      ref={canvasRef}
      className="pointer-events-none absolute inset-0 z-0"
      style={{ width: "100%", height: "100%" }}
    />
  );
}
