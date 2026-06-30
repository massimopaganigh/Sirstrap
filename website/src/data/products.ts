import type { Product } from "@/domain/product";
import iconCleaner from "../../../src/SirHurt.Cleaner.CLI/Assets/favicon.ico";
import iconCli from "../../../src/Sirstrap.CLI/Assets/favicon.ico";
import iconUi from "../../../src/Sirstrap.UI/Assets/favicon.ico";
import iconKnee from "@/assets/kneesurgery.ico";

export const products: Product[] = [
  {
    name: "SirHurt.Cleaner.CLI",
    description:
      "A complete cleanup utility that wipes Roblox, SirHurt, and Sirstrap from your filesystem and registry — built by exploiters, for exploiters.",
    asset: "SirHurt.Cleaner.CLI.zip",
    variants: ["SirHurt.Cleaner.CLI_fat.zip", "SirHurt.Cleaner.CLI.cab", "SirHurt.Cleaner.CLI_fat.cab"],
    externalDownloads: [{ repo: "massimopaganigh/sirhurt.cleaner", match: { type: "exact", asset: "SirHurt.Cleaner.exe" } }],
    recommended: false,
    icon: iconCleaner,
    source: "https://github.com/massimopaganigh/Sirstrap/tree/main/src/SirHurt.Cleaner.CLI",
    accent: "red",
    title: { kind: "typewriter", head: "SirHurt.Cleaner", tail: ".CLI" },
    core: true,
  },
  {
    name: "Sirstrap.CLI",
    description:
      "An alternative Roblox bootstrapper CLI packed with additional features — built by exploiters, for exploiters.",
    asset: "Sirstrap.CLI.zip",
    variants: ["Sirstrap.CLI_fat.zip", "Sirstrap.CLI.cab", "Sirstrap.CLI_fat.cab", "Sirstrap.exe"],
    externalDownloads: [],
    recommended: false,
    icon: iconCli,
    source: "https://github.com/massimopaganigh/Sirstrap/tree/main/src/Sirstrap.CLI",
    accent: "teal",
    title: { kind: "typewriter", head: "Sirstrap", tail: ".CLI" },
    core: true,
  },
  {
    name: "Sirstrap.UI",
    description:
      "An alternative Roblox bootstrapper UI packed with additional features — built by exploiters, for exploiters.",
    asset: "Sirstrap.UI.zip",
    variants: ["Sirstrap.UI_fat.zip", "Sirstrap.UI.cab", "Sirstrap.UI_fat.cab"],
    externalDownloads: [],
    recommended: true,
    icon: iconUi,
    source: "https://github.com/massimopaganigh/Sirstrap/tree/main/src/Sirstrap.UI",
    accent: "amber",
    title: { kind: "shimmer", head: "Sirstrap", tail: ".UI" },
    core: true,
  },
  {
    name: "KneeSurgery",
    description: "A DLL for building custom UIs for SirHurt — built by exploiters, for exploiters.",
    asset: "KneeSurgery",
    variants: [],
    externalDownloads: [{ repo: "massimopaganigh/KneeSurgery", match: { type: "prefix", prefix: "KneeSurgery_" } }],
    recommended: false,
    icon: iconKnee,
    source: "https://github.com/massimopaganigh/KneeSurgery",
    accent: "mint",
    title: { kind: "rapid", head: "Knee", tail: "Surgery", glyph: "e", max: 15 },
    repo: "massimopaganigh/KneeSurgery",
    downloadUrl: "https://github.com/massimopaganigh/KneeSurgery/releases/latest",
  },
];
