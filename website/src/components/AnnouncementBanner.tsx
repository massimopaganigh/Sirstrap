import { Icon } from "@/components/icons/Icon";

interface AnnouncementBannerProps {
  message: string;
}

export function AnnouncementBanner({ message }: AnnouncementBannerProps) {
  return (
    <div
      aria-label="Announcement"
      aria-live="polite"
      role="status"
      className="pointer-events-none fixed inset-x-0 bottom-0 z-30 flex justify-center md:left-1/2 md:right-auto md:w-[42rem] md:max-w-[calc(100vw-2rem)] md:-translate-x-1/2"
    >
      <div className="flex h-8 w-full min-w-0 items-center justify-center gap-2 overflow-hidden rounded-t-[5px] rounded-b-none border border-b-0 border-glow-teal/40 bg-background/75 px-3 font-body text-[0.75rem] leading-none text-muted-foreground shadow-[0_10px_30px_rgba(0,0,0,0.28)] backdrop-blur-md animate-in fade-in-0 slide-in-from-bottom-4 duration-500">
        <Icon name="megaphone" className="h-3.5 w-3.5 shrink-0 text-glow-teal" />
        <span className="truncate">{message}</span>
      </div>
    </div>
  );
}
