import { useEffect, useState } from "react";
import { announcementRepository, type AnnouncementRepository } from "@/services/announcement-repository";

export function useAnnouncement(repository: AnnouncementRepository = announcementRepository): string | null {
  const [announcement, setAnnouncement] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    repository.fetch().then(value => {
      if (!cancelled && value) setAnnouncement(value);
    });

    return () => {
      cancelled = true;
    };
  }, [repository]);

  return announcement;
}
