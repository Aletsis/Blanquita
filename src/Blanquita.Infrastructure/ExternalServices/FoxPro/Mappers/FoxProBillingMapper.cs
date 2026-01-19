using Blanquita.Application.DTOs;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Mappers;

public static class FoxProBillingMapper
{
    public static BillingReportItemDto MapFromInvoice(IFoxProDataReader reader)
    {
        return new BillingReportItemDto
        {
            FechaEmision = reader.GetDateTimeSafe("CFECHAEMI"),
            HoraEmision = reader.GetStringSafe("CHORAEMI"),
            Serie = reader.GetStringSafe("CSERIE"),
            Folio = reader.GetStringSafe("CFOLIO"),
            Rfc = reader.GetStringSafe("CRFC"),
            RazonSocial = reader.GetStringSafe("CRAZON"),
            Uuid = reader.GetStringSafe("CUUID"),
            IdDocumento = reader.GetStringSafe("CIDDOCTO")
        };
    }

    public static BillingReportItemDto MapFromDocument(BillingReportItemDto item, IFoxProDataReader reader)
    {
        return item with
        {
            Neto = reader.GetDecimalSafe("CNETO"),
            Total = reader.GetDecimalSafe("CTOTAL"),
            Impuesto1 = reader.GetDecimalSafe("CIMPUESTO1"), 
            Cancelado = reader.GetInt32Safe("CCANCELADO"),
            Estado = reader.GetInt32Safe("CESTADO"),
            Entregado = reader.GetInt32Safe("CENTREGADO"),
            Cautusba01 = reader.GetStringSafe("CAUTUSBA01"),
            Fecha = reader.GetDateTimeSafe("CFECHA"),
            TextoExtra3 = reader.GetStringSafe("CTEXTOEX03"),
            ImporteExtra3 = reader.GetDecimalSafe("CIMPORTE03")
        };
    }
}
