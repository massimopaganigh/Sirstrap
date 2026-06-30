import { MAIN_REPO } from "@/config/site.config";
import { products } from "@/data/products";
import { useIsMobile } from "@/hooks/use-mobile";
import { useAnnouncement } from "@/hooks/useAnnouncement";
import { useReleaseData } from "@/hooks/useReleaseData";
import { useContributors } from "@/hooks/useContributors";
import { usePanelInteraction } from "@/hooks/usePanelInteraction";
import { mostDownloadedAsset } from "@/services/download-aggregator";
import { AnnouncementBanner } from "@/components/AnnouncementBanner";
import { CoreBracket } from "@/components/CoreBracket";
import { ProductPanel } from "@/components/ProductPanel";

const leadingCoreCount = (() => {
  let count = 0;
  for (const product of products) {
    if (!product.core) break;
    count++;
  }
  return count;
})();

const Index = () => {
  const isMobile = useIsMobile();
  const announcement = useAnnouncement();
  const { versions, downloads, loading: releasesLoading } = useReleaseData(products);
  const { byRepo: contributorsByRepo, loading: contributorsLoading } = useContributors(products);
  const panel = usePanelInteraction(isMobile);
  const mostPopular = mostDownloadedAsset(products, downloads);

  return (
    <div className="relative flex flex-col md:flex-row min-h-screen md:h-screen w-screen overflow-y-auto md:overflow-hidden">
      {announcement && <AnnouncementBanner message={announcement} />}
      <CoreBracket count={leadingCoreCount} total={products.length} dimmed={panel.hasActive} />
      {products.map((product, index) => (
        <ProductPanel
          key={product.name}
          product={product}
          index={index}
          isMobile={isMobile}
          active={panel.isActive(index)}
          hasActive={panel.hasActive}
          flex={panel.flexOf(index)}
          opacity={panel.opacityOf(index)}
          contributors={contributorsByRepo[product.repo ?? MAIN_REPO] ?? []}
          releasesLoading={releasesLoading}
          contributorsLoading={contributorsLoading}
          downloadCount={downloads[product.asset]}
          version={versions[product.repo ?? MAIN_REPO]}
          mostPopular={mostPopular === product.asset}
          onEnter={() => panel.open(index)}
          onLeave={panel.close}
          onClick={() => panel.toggle(index)}
        />
      ))}
    </div>
  );
};

export default Index;
