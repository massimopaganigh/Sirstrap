import { useEffect, useState } from "react";
import { MAIN_REPO } from "@/config/site.config";
import type { Product } from "@/domain/product";
import {
  githubContributorRepository,
  type Contributor,
  type ContributorRepository,
} from "@/services/contributor-repository";

const unique = <T,>(values: T[]): T[] => [...new Set(values)];

export interface ContributorsData {
  byRepo: Record<string, Contributor[]>;
  loading: boolean;
}

export function useContributors(
  products: Product[],
  repository: ContributorRepository = githubContributorRepository,
): ContributorsData {
  const [byRepo, setByRepo] = useState<Record<string, Contributor[]>>({});
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    const repos = unique(products.map(product => product.repo ?? MAIN_REPO));

    Promise.all(repos.map(async repo => [repo, await repository.fetchContributors(repo)] as const))
      .then(entries => {
        if (cancelled) return;
        const map: Record<string, Contributor[]> = {};
        for (const [repo, contributors] of entries) map[repo] = contributors;
        setByRepo(map);
      })
      .catch(() => {})
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [products, repository]);

  return { byRepo, loading };
}
