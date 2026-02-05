using Microsoft.Extensions.ObjectPool;
using SPTarkov.Server.Core.Models.Utils;

namespace gekosbetterprogression;

public interface ILoggerWrapper
{
    void Success(string message);
    void Error(string message, Exception? ex = null);
    void Warning(string message);
    void Info(string message);
    void Debug(string message);
}

public class LoggerWrapper<T> : ILoggerWrapper
{
    private readonly ISptLogger<T> _logger;
    private readonly string prefix;

    public LoggerWrapper(ISptLogger<T> logger)
    {
        _logger = logger;

        var metadata = new ModMetadata();

        prefix = $"[{metadata.Name}-{metadata.Version}]";
    }

    public void Success(string message)
        => _logger.Success(prefix + message);

    public void Error(string message, Exception? ex = null)
        => _logger.Error(prefix + message, ex);

    public void Warning(string message)
        => _logger.Warning(prefix + message);

    public void Info(string message)
        => _logger.Info(prefix + message);

    public void Debug(string message)
        => _logger.Debug(prefix + message);
}