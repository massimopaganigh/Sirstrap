import type { AssetMatcher, Product } from "@/domain/product";
import type { Release } from "@/services/release-repository";

export type AssetCounts = Record<string, number>;

export function countAssets(releases: Release[]): AssetCounts {
  const counts: AssetCounts = {};
  for (const release of releases) {
    for (const asset of release.assets ?? []) {
      counts[asset.name] = (counts[asset.name] ?? 0) + asset.download_count;
    }
  }
  return counts;
}

function matchedCount(matcher: AssetMatcher, counts: AssetCounts): number {
  if (matcher.type === "exact") return counts[matcher.asset] ?? 0;
  return Object.entries(counts)
    .filter(([name]) => name.startsWith(matcher.prefix))
    .reduce((sum, [, count]) => sum + count, 0);
}

export function aggregateDownloads(
  products: Product[],
  mainCounts: AssetCounts,
  externalCountsByRepo: Record<string, AssetCounts>,
): AssetCounts {
  const totals: AssetCounts = { ...mainCounts };

  const add = (asset: string, amount: number) => {
    if (amount > 0) totals[asset] = (totals[asset] ?? 0) + amount;
  };

  for (const product of products) {
    for (const variant of product.variants) {
      const count = mainCounts[variant];
      if (count) {
        add(product.asset, count);
        delete totals[variant];
      }
    }
    for (const external of product.externalDownloads) {
      add(product.asset, matchedCount(external.match, externalCountsByRepo[external.repo] ?? {}));
    }
  }

  return totals;
}

export function mostDownloadedAsset(products: Product[], downloads: AssetCounts): string | null {
  if (Object.keys(downloads).length === 0) return null;
  return products.reduce((best, product) =>
    (downloads[product.asset] ?? 0) > (downloads[best.asset] ?? 0) ? product : best,
  ).asset;
}
