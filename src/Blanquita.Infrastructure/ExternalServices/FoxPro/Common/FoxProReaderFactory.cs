using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Common;

public class FoxProReaderFactory : IFoxProReaderFactory
{
    private readonly ILogger<FoxProReaderFactory> _logger;

    public FoxProReaderFactory(ILogger<FoxProReaderFactory> logger)
    {
        _logger = logger;
    }

    public IFoxProDataReader CreateReader(string filePath)
    {
        // Usa la clase estática existente para crear el DbfDataReader real
        var reader = DbfReaderFactory.CreateReader(filePath);
        return new FoxProDataReaderWrapper(reader);
    }
}
