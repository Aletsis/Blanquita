
namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Common;

public interface IFoxProReaderFactory
{
    IFoxProDataReader CreateReader(string filePath);
}
