import { useTypewriter } from "@/hooks/useTypewriter";

interface TypewriterTextProps {
  text: string;
  className?: string;
  enabled: boolean;
}

export function TypewriterText({ text, className, enabled }: TypewriterTextProps) {
  const displayed = useTypewriter(text, enabled);
  return (
    <span className={className}>
      {displayed}
      {enabled && <span className="title-caret">▌</span>}
    </span>
  );
}
