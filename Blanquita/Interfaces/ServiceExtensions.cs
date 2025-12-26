using Blanquita.Interfaces;
using Blanquita.Services;
using Blanquita.Services.Parsing;
using CurrieTechnologies.Razor.SweetAlert2;

namespace Blanquita.Interfaces
{
    public static class ServiceExtensions
    {
        public static void AddAppServices(this IServiceCollection services)
        {
            services.AddScoped<ICajaService, CajaService>();
            services.AddScoped<ICajeraService, CajeraService>();
            services.AddScoped<IEncargadaService, EncargadaService>();
            services.AddScoped<IRecoService, RecoService>();
            services.AddScoped<ICorteService, CorteService>();
            services.AddScoped<IPrinterRepository, PrinterRepository>();
            services.AddScoped<IPrinterCommandBuilder, PrinterCommandBuilder>();
            services.AddScoped<IPrinterNetworkServiceFactory, PrinterNetworkServiceFactory>();
            services.AddScoped<IZebraPrinterService, ZebraPrinterService>();
            services.AddScoped<ISearchInDbfFileService, SearchInDbfFileService>();
            services.AddScoped<IFoxProService, FoxProService>();
            services.AddScoped<IExportService, ExportService>();
            services.AddScoped<IReporteService, ReporteService>();
            services.AddScoped<PrintJobService>();
            services.AddSingleton<IDbfStringParser, DbfStringParser>();
            services.AddSingleton<FileConfigService>();
            services.AddSingleton<BrowserPrintService>();
            services.AddLogging();
            services.AddSweetAlert2();
        }
    }
}
