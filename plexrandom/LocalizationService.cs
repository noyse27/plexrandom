using System;
using System.Linq;
using System.Windows;

namespace plexrandom;

public static class LocalizationService
{
    public static string Language { get; private set; } = "de";

    public static event EventHandler? LanguageChanged;

    public static void SetLanguage(string lang)
    {
        Language = lang;
        var dict = new ResourceDictionary
        {
            Source = new Uri($"/plexrandom;component/Localization/Strings.{lang}.xaml", UriKind.Relative)
        };

        var existing = Application.Current.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source?.OriginalString.Contains("/Localization/Strings.") == true);
        if (existing != null)
            Application.Current.Resources.MergedDictionaries.Remove(existing);

        Application.Current.Resources.MergedDictionaries.Add(dict);
        LanguageChanged?.Invoke(null, EventArgs.Empty);
    }

    public static string Get(string key)
        => Application.Current.TryFindResource(key) as string ?? $"[{key}]";

    public static string Format(string key, params object[] args)
        => string.Format(Get(key), args);
}
