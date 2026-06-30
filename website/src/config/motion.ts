export const MOTION = {
  duration: { fast: 200, base: 300, slow: 500 },
  easing: "cubic-bezier(0.4, 0, 0.2, 1)",
} as const;

export const transition = (properties: string[], duration: number = MOTION.duration.slow) =>
  properties.map(property => `${property} ${duration}ms ${MOTION.easing}`).join(", ");

export const TITLE_TIMING = {
  startDelay: 5000,
  holdFull: 5000,
  holdEmpty: 2500,
  type: { min: 125, jitter: 25 },
  rapid: { min: 30, jitter: 25 },
  erase: { min: 25, jitter: 25 },
} as const;

export const jitter = (range: { min: number; jitter: number }) => range.min + Math.random() * range.jitter;
