namespace EasySave.ViewModels.Services;

/// <summary>
///     No-op implementation of <see cref="IUiTextService" /> that always returns the provided fallback.
///     Used when no real service is injected (e.g. unit tests, design-time instances).
/// </summary>
internal sealed class NullUiTextService : IUiTextService
{
    /// <inheritdoc />
    public string Get(string resourceKey, string fallback) => fallback;

    /// <inheritdoc />
    public string Format(string resourceKey, string fallback, params object[] args)
        => string.Format(fallback, args);
}

