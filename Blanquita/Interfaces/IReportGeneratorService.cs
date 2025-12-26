using Blanquita.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blanquita.Interfaces
{
    public interface IReportGeneratorService
    {
        Task<List<ReportRow>> GenerarReportDataAsync(string sucursal, DateTime fecha);
    }
}
