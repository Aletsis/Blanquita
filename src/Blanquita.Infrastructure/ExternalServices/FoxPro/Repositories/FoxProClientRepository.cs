using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.ExternalServices.FoxPro.Repositories;

public class FoxProClientRepository : IClientCatalogRepository
{
    private readonly IConfiguracionService _configService;
    private readonly IFoxProReaderFactory _readerFactory;
    private readonly ILogger<FoxProClientRepository> _logger;

    public FoxProClientRepository(
        IConfiguracionService configService,
        IFoxProReaderFactory readerFactory,
        ILogger<FoxProClientRepository> logger)
    {
        _configService = configService;
        _readerFactory = readerFactory;
        _logger = logger;
    }

    public async Task<ClientSearchDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var results = await SearchInternalAsync(code, isExactMatch: true, cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<IEnumerable<ClientSearchDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await SearchInternalAsync(searchTerm, isExactMatch: false, cancellationToken);
    }

    private async Task<IEnumerable<ClientSearchDto>> SearchInternalAsync(string searchTerm, bool isExactMatch, CancellationToken cancellationToken)
    {
        var config = await _configService.ObtenerConfiguracionAsync();
        var clientPath = config.Mgw10002Path;
        var addressPath = config.Mgw10011Path;

        if (string.IsNullOrEmpty(clientPath) || !File.Exists(clientPath))
        {
            _logger.LogWarning("Archivo MGW10002 no configurado o no existe");
            return Enumerable.Empty<ClientSearchDto>();
        }

        var clients = new List<ClientSearchDto>();
        var term = searchTerm?.Trim();
        if (string.IsNullOrEmpty(term)) return clients;

        bool isNumeric = term.All(char.IsDigit);

            // 1. Buscar clientes
            using (var reader = _readerFactory.CreateReader(clientPath))
            {
                while (reader.Read())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Using UPPERCASE for column names to ensure compatibility
                    var code = reader.GetStringSafe("CCODIGOC01");
                    var name = reader.GetStringSafe("CRAZONSO01");
                    var rfc = reader.GetStringSafe("CRFC");

                    bool match = false;
                    if (isExactMatch)
                    {
                        match = string.Equals(code, term, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        match = IsMatch(term, code, false) ||
                                IsMatch(term, name, false) ||
                                IsMatch(term, rfc, false);
                    }

                    if (match)
                    {
                        var client = new ClientSearchDto
                        {
                            Id = reader.GetInt32Safe("CIDCLIEN01"),
                            Code = code,
                            Name = name,
                            Rfc = rfc,
                            Email = reader.GetStringSafe("CEMAIL1"),
                            PaymentMethod = reader.GetStringSafe("CMETODOPAG"),
                            CfdiUse = reader.GetStringSafe("CUSOCFDI"),
                            FiscalRegime = reader.GetStringSafe("CREGIMFISC"),
                            RegistrationDate = reader.GetDateTimeSafe("CFECHAALTA")
                        };
                        clients.Add(client);

                        if (!isExactMatch && clients.Count >= 50) break; // Limit results
                    }
                }
            }

            // 2. Si encontramos clientes y tenemos ruta de direcciones, buscar sus direcciones
            if (clients.Any() && !string.IsNullOrEmpty(addressPath) && File.Exists(addressPath))
            {
                var clientIds = clients.ToDictionary(c => c.Id);
                
                using (var reader = _readerFactory.CreateReader(addressPath))
                {
                    while (reader.Read())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // CIDCATAL01 en MGW10011 es el FK hacia CIDCLIEN01 de MGW10002
                        var clientId = reader.GetInt32Safe("CIDCATAL01");

                        if (clientIds.TryGetValue(clientId, out var client))
                        {
                            var address = new ClientAddressDto
                            {
                                Street = reader.GetStringSafe("CNOMBREC01"),
                                ExteriorNumber = reader.GetStringSafe("CNUMEROE01"),
                                InteriorNumber = reader.GetStringSafe("CNUMEROI01"),
                                Colony = reader.GetStringSafe("CCOLONIA"),
                                ZipCode = reader.GetStringSafe("CCODIGOP01"),
                                Phone = reader.GetStringSafe("CTELEFONO1"),
                                Email = reader.GetStringSafe("CEMAIL"),
                                Country = reader.GetStringSafe("CPAIS"),
                                State = reader.GetStringSafe("CESTADO"),
                                City = reader.GetStringSafe("CCIUDAD"),
                                Municipality = reader.GetStringSafe("CMUNICIPIO")
                            };
                            
                            client.Addresses.Add(address);
                        }
                    }
                }
            }

            return clients;
    }

    private bool IsMatch(string term, string value, bool isExact)
    {
        if (string.IsNullOrEmpty(value)) return false;
        
        if (isExact)
            return string.Equals(value.Trim(), term, StringComparison.OrdinalIgnoreCase);
        
        return value.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
