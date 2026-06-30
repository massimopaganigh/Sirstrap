import type { FC } from "react";
import type { TitleAnimation } from "@/domain/product";
import { TypewriterText } from "@/components/TypewriterText";
import { RapidEeeText } from "@/components/RapidEeeText";

const HEADING = "font-display text-2xl font-extrabold tracking-[-0.035em] lg:text-3xl";
const TAIL = `${HEADING} inline-block`;

interface TitleRendererProps<A extends TitleAnimation = TitleAnimation> {
  animation: A;
  active: boolean;
  accentClassName: string;
}

type Animation<K extends TitleAnimation["kind"]> = Extract<TitleAnimation, { kind: K }>;

const breakable = (text: string) =>
  text.split(".").flatMap((segment, index, segments) =>
    index < segments.length - 1 ? [segment, ".", <wbr key={index} />] : [segment],
  );

const PlainTitle: FC<TitleRendererProps<Animation<"plain">>> = ({ animation, accentClassName }) => (
  <h2 className={`${HEADING} ${accentClassName}`}>{breakable(animation.head)}</h2>
);

const TypewriterTitle: FC<TitleRendererProps<Animation<"typewriter">>> = ({ animation, active, accentClassName }) => (
  <h2 className={`${HEADING} ${accentClassName}`}>
    {breakable(animation.head)}
    <TypewriterText text={animation.tail} className={`${TAIL} ${accentClassName}`} enabled={active} />
  </h2>
);

const ShimmerTitle: FC<TitleRendererProps<Animation<"shimmer">>> = ({ animation, active, accentClassName }) => (
  <h2 className={`${HEADING} ${accentClassName}`}>
    {breakable(animation.head)}
    <span className={`${TAIL} ${active ? "title-shimmer" : accentClassName}`}>{animation.tail}</span>
  </h2>
);

const RapidTitle: FC<TitleRendererProps<Animation<"rapid">>> = ({ animation, active, accentClassName }) => (
  <h2 className={`${HEADING} ${accentClassName}`}>
    {breakable(animation.head)}
    <RapidEeeText className={`${HEADING} ${accentClassName}`} enabled={active} glyph={animation.glyph} max={animation.max} />
    <span className={`${TAIL} ${accentClassName}`}>{animation.tail}</span>
  </h2>
);

const RENDERERS: { [K in TitleAnimation["kind"]]: FC<TitleRendererProps<Animation<K>>> } = {
  plain: PlainTitle,
  typewriter: TypewriterTitle,
  shimmer: ShimmerTitle,
  rapid: RapidTitle,
};

export function ProductTitle({ animation, active, accentClassName }: TitleRendererProps) {
  const Renderer = RENDERERS[animation.kind] as FC<TitleRendererProps>;
  return <Renderer animation={animation} active={active} accentClassName={accentClassName} />;
}
