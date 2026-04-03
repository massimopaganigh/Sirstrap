import { Download, Terminal, Monitor, Shield } from "lucide-react";

type ProductCardProps = {
  name: string;
  description: string;
  icon: "terminal" | "monitor" | "shield";
  glowColor: "green" | "purple" | "blue";
  downloadUrl: string;
  features: string[];
  delay: number;
};

const icons = {
  terminal: Terminal,
  monitor: Monitor,
  shield: Shield,
};

const glowStyles = {
  green: "shadow-[0_0_40px_-10px_hsl(var(--glow-green)/0.4)] hover:shadow-[0_0_60px_-10px_hsl(var(--glow-green)/0.6)]",
  purple: "shadow-[0_0_40px_-10px_hsl(var(--glow-purple)/0.4)] hover:shadow-[0_0_60px_-10px_hsl(var(--glow-purple)/0.6)]",
  blue: "shadow-[0_0_40px_-10px_hsl(var(--glow-blue)/0.4)] hover:shadow-[0_0_60px_-10px_hsl(var(--glow-blue)/0.6)]",
};

const iconColors = {
  green: "text-glow-green",
  purple: "text-glow-purple",
  blue: "text-glow-blue",
};

const borderColors = {
  green: "border-glow-green/20 hover:border-glow-green/40",
  purple: "border-glow-purple/20 hover:border-glow-purple/40",
  blue: "border-glow-blue/20 hover:border-glow-blue/40",
};

const btnStyles = {
  green: "bg-glow-green/10 border-glow-green/30 text-glow-green hover:bg-glow-green hover:text-primary-foreground",
  purple: "bg-glow-purple/10 border-glow-purple/30 text-glow-purple hover:bg-glow-purple hover:text-accent-foreground",
  blue: "bg-glow-blue/10 border-glow-blue/30 text-glow-blue hover:bg-glow-blue hover:text-primary-foreground",
};

const ProductCard = ({ name, description, icon, glowColor, downloadUrl, features, delay }: ProductCardProps) => {
  const Icon = icons[icon];

  return (
    <div
      className={`opacity-0 animate-fade-up flex flex-col rounded-2xl border bg-card p-8 transition-all duration-500 ${glowStyles[glowColor]} ${borderColors[glowColor]}`}
      style={{ animationDelay: `${delay}ms` }}
    >
      <div className="mb-6 flex items-center gap-4">
        <div className={`rounded-xl bg-muted p-3 ${iconColors[glowColor]}`}>
          <Icon className="h-7 w-7" />
        </div>
        <h2 className="text-xl font-bold tracking-tight text-foreground">{name}</h2>
      </div>

      <p className="mb-6 text-sm leading-relaxed text-muted-foreground">{description}</p>

      <ul className="mb-8 flex-1 space-y-3">
        {features.map((f) => (
          <li key={f} className="flex items-start gap-2 text-sm text-secondary-foreground">
            <span className={`mt-1 h-1.5 w-1.5 shrink-0 rounded-full ${iconColors[glowColor].replace("text-", "bg-")}`} />
            {f}
          </li>
        ))}
      </ul>

      <a
        href={downloadUrl}
        target="_blank"
        rel="noopener noreferrer"
        className={`inline-flex items-center justify-center gap-2 rounded-lg border px-5 py-3 text-sm font-semibold transition-all duration-300 ${btnStyles[glowColor]}`}
      >
        <Download className="h-4 w-4" />
        Download
      </a>
    </div>
  );
};

export default ProductCard;
