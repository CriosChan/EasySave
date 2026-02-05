using EasyLog;
using EasySave.Application.Abstractions;

namespace EasySave.Infrastructure.Logging;

/// <summary>
/// Adaptateur de log JSON base sur EasyLog.
/// </summary>
public sealed class JsonLogWriter<T> : ILogWriter<T>
{
    private readonly AbstractLogger<T> _logger;

    /// <summary>
    /// Construit un writer JSON base sur un dossier de logs.
    /// </summary>
    /// <param name="logDirectory">Dossier de destination des logs.</param>
    public JsonLogWriter(string logDirectory)
        : this(new JsonLogger<T>(logDirectory))
    {
    }

    /// <summary>
    /// Construit un writer JSON a partir d'un logger EasyLog.
    /// </summary>
    /// <param name="logger">Logger concret.</param>
    public JsonLogWriter(AbstractLogger<T> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Ecrit une entree de log.
    /// </summary>
    /// <param name="entry">Entree a enregistrer.</param>
    public void Log(T entry) => _logger.Log(entry);
}
