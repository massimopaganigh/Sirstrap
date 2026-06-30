import { ANNOUNCEMENT_URL } from "@/config/site.config";

export interface AnnouncementRepository {
  fetch(): Promise<string | null>;
}

export class RemoteAnnouncementRepository implements AnnouncementRepository {
  constructor(private readonly url: string) {}

  async fetch(): Promise<string | null> {
    try {
      const response = await fetch(this.url);
      if (!response.ok) return null;
      const text = (await response.text()).trim();
      return text ? text : null;
    } catch {
      return null;
    }
  }
}

export const announcementRepository: AnnouncementRepository = new RemoteAnnouncementRepository(ANNOUNCEMENT_URL);
