import { ACCENT_BORDER_HOVER, ACCENT_TEXT_HOVER, type Accent } from "@/config/accents";
import type { Contributor } from "@/services/contributor-repository";

const AUTHOR = "massimopaganigh";
const AUTHOR_URL = `https://github.com/${AUTHOR}`;
const AUTHOR_AVATAR = `https://github.com/${AUTHOR}.png`;
const MAX_AVATARS = 8;
const SKELETON_COUNT = 3;
const AVATAR = "h-5 w-5 rounded-full border-2 border-background bg-background object-cover";

interface ProductBylineProps {
  accent: Accent;
  contributors: Contributor[];
  loading: boolean;
}

const stopPropagation = (event: { stopPropagation: () => void }) => event.stopPropagation();

export function ProductByline({ accent, contributors, loading }: ProductBylineProps) {
  const others = contributors.filter(contributor => contributor.login.toLowerCase() !== AUTHOR).slice(0, MAX_AVATARS - 1);
  const trailing = others.length + (loading ? SKELETON_COUNT : 0);

  return (
    <div className="group flex w-fit items-center gap-2" onClick={stopPropagation}>
      <a
        href={AUTHOR_URL}
        target="_blank"
        rel="noopener noreferrer"
        className="font-body text-[0.85rem] leading-[1.55] text-muted-foreground transition-colors duration-200 group-hover:text-foreground"
      >
        made by <span className={`transition-colors duration-200 ${ACCENT_TEXT_HOVER[accent]}`}>ギャップ</span>
      </a>
      <div className="flex items-center">
        <a
          href={AUTHOR_URL}
          target="_blank"
          rel="noopener noreferrer"
          aria-label={AUTHOR}
          style={{ zIndex: trailing + 1 }}
          className="relative transition-all duration-300 ease-out hover:!z-20 hover:-translate-y-1"
        >
          <img
            src={AUTHOR_AVATAR}
            alt={AUTHOR}
            className={`${AVATAR} transition-all duration-300 group-hover:scale-110 ${ACCENT_BORDER_HOVER[accent]}`}
          />
        </a>

        {loading
          ? Array.from({ length: SKELETON_COUNT }).map((_, index) => (
              <span
                key={`skeleton-${index}`}
                style={{ zIndex: trailing - index, animationDelay: `${index * 150}ms` }}
                className={`relative -ml-2 ${AVATAR} bg-muted/60 animate-pulse`}
              />
            ))
          : others.map((contributor, index) => (
              <a
                key={contributor.login}
                href={contributor.htmlUrl}
                target="_blank"
                rel="noopener noreferrer"
                aria-label={contributor.login}
                style={{ zIndex: trailing - index, transitionDelay: `${index * 40}ms` }}
                className="relative -ml-2 transition-all duration-300 ease-out hover:!z-20 hover:-translate-y-1 group-hover:-ml-1"
              >
                <img
                  src={contributor.avatarUrl}
                  alt={contributor.login}
                  loading="lazy"
                  className={`${AVATAR} transition-all duration-300 group-hover:border-border`}
                />
              </a>
            ))}
      </div>
    </div>
  );
}
