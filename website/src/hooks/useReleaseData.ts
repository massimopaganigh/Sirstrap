import { useEffect, useState } from "react";
import { MAIN_REPO } from "@/config/site.config";
import type { Product } from "@/domain/product";
import { githubReleaseRepository, type ReleaseRepository } from "@/services/release-repository";
import { aggregateDownloads, countAssets, type AssetCounts } from "@/services/download-aggregator";

const unique = <T,>(values: T[]): T[] => [...new Set(values)];

export interface ReleaseData {
  versions: Record<string, string>;
  downloads: AssetCounts;
  loading: boolean;
}

export function useReleaseData(
  products: Product[],
  repository: ReleaseRepository = githubReleaseRepository,
): ReleaseData {
  const [versions, setVersions] = useState<Record<string, string>>({});
  const [downloads, setDownloads] = useState<AssetCounts>({});
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      const productRepos = unique(products.map(product => product.repo ?? MAIN_REPO));
      const externalRepos = unique(products.flatMap(product => product.externalDownloads.map(external => external.repo)));

      const [tags, mainReleases, externalReleases] = await Promise.all([
        Promise.all(productRepos.map(async repo => [repo, await repository.fetchLatestTag(repo)] as const)),
        repository.fetchAllReleases(MAIN_REPO),
        Promise.all(externalRepos.map(async repo => [repo, await repository.fetchAllReleases(repo)] as const)),
      ]);

      if (cancelled) return;

      const resolvedVersions: Record<string, string> = {};
      for (const [repo, tag] of tags) {
        if (tag) resolvedVersions[repo] = tag;
      }
      setVersions(resolvedVersions);

      const externalCountsByRepo: Record<string, AssetCounts> = {};
      for (const [repo, releases] of externalReleases) {
        externalCountsByRepo[repo] = countAssets(releases);
      }
      setDownloads(aggregateDownloads(products, countAssets(mainReleases), externalCountsByRepo));
    };

    load()
      .catch(() => {})
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [products, repository]);

  return { versions, downloads, loading };
}
