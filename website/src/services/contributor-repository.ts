import { GITHUB_API_BASE } from "@/config/site.config";

export interface Contributor {
  login: string;
  avatarUrl: string;
  htmlUrl: string;
}

export interface ContributorRepository {
  fetchContributors(repo: string): Promise<Contributor[]>;
}

interface GithubContributor {
  login: string;
  avatar_url: string;
  html_url: string;
  type: string;
}

export class GithubContributorRepository implements ContributorRepository {
  constructor(private readonly apiBase: string = GITHUB_API_BASE) {}

  async fetchContributors(repo: string): Promise<Contributor[]> {
    try {
      const response = await fetch(`${this.apiBase}/repos/${repo}/contributors?per_page=100`);
      const json = await response.json();
      if (!Array.isArray(json)) return [];
      return (json as GithubContributor[])
        .filter(contributor => contributor.type === "User" && !contributor.login.toLowerCase().endsWith("[bot]"))
        .map(contributor => ({
          login: contributor.login,
          avatarUrl: contributor.avatar_url,
          htmlUrl: contributor.html_url,
        }));
    } catch {
      return [];
    }
  }
}

export const githubContributorRepository: ContributorRepository = new GithubContributorRepository();
