import { GITHUB_API_BASE } from "@/config/site.config";

export interface ReleaseAsset {
  name: string;
  download_count: number;
}

export interface Release {
  assets?: ReleaseAsset[];
}

export interface ReleaseRepository {
  fetchLatestTag(repo: string): Promise<string | undefined>;
  fetchAllReleases(repo: string): Promise<Release[]>;
}

const NEXT_PAGE = /<([^>]+)>;\s*rel="next"/;

export class GithubReleaseRepository implements ReleaseRepository {
  constructor(private readonly apiBase: string = GITHUB_API_BASE) {}

  async fetchLatestTag(repo: string): Promise<string | undefined> {
    try {
      const response = await fetch(`${this.apiBase}/repos/${repo}/releases/latest`);
      const json = await response.json();
      return json.tag_name as string | undefined;
    } catch {
      return undefined;
    }
  }

  async fetchAllReleases(repo: string): Promise<Release[]> {
    const releases: Release[] = [];
    let url: string | null = `${this.apiBase}/repos/${repo}/releases?per_page=100`;

    while (url) {
      const response = await fetch(url);
      const page = await response.json();
      if (Array.isArray(page)) releases.push(...page);
      const next = (response.headers.get("Link") ?? "").match(NEXT_PAGE);
      url = next ? next[1] : null;
    }

    return releases;
  }
}

export const githubReleaseRepository: ReleaseRepository = new GithubReleaseRepository();
