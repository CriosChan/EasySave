using EasySave.Translations;

namespace EasySave.ViewModels.Services;

/// <summary>
///     Tlumach-based implementation of <see cref="IUiTextService" />.
/// </summary>
public sealed class TlumachUiTextService : IUiTextService
{
    /// <inheritdoc />
    public string Get(string resourceKey, string fallback)
    {
        var entry = Strings.TranslationManager.GetValue(resourceKey);
        return string.IsNullOrEmpty(entry.Text) ? fallback : entry.Text;
    }

    /// <inheritdoc />
    public string Format(string resourceKey, string fallback, params object[] args)
    {
        return string.Format(Get(resourceKey, fallback), args);
    }
}
