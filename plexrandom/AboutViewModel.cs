using System.Reflection;

namespace plexrandom;

public class AboutViewModel : BaseViewModel
{
    public string AppName { get; } =
        Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>()?.Product
        ?? "Plex Randomizer";

    public string AppVersion { get; } =
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    public string Copyright { get; } = "© PolzeSoft 2026";

    public string Website { get; } = "https://polze.net";

    public string ContactEmail { get; } = "plexrandom@polze.net";
}
