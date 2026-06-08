using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Blanquita.Application.Queries.Cashiers;

/// <summary>
/// Handler para el query GetCashierByContpaqIdQuery.
/// </summary>
public class GetCashierByContpaqIdQueryHandler : IRequestHandler<GetCashierByContpaqIdQuery, CashierDto?>
{
    private readonly ICashierService _cashierService;
    private readonly ILogger<GetCashierByContpaqIdQueryHandler> _logger;

    public GetCashierByContpaqIdQueryHandler(
        ICashierService cashierService,
        ILogger<GetCashierByContpaqIdQueryHandler> logger)
    {
        _cashierService = cashierService;
        _logger = logger;
    }

    public async Task<CashierDto?> Handle(GetCashierByContpaqIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando cajera en sucursal {BranchId} con IDContpaq {ContpaqId}", request.BranchId, request.ContpaqId);
        
        CashierDto? cashier = null;
        
        if (request.ContpaqId > 0)
        {
            var cashiers = await _cashierService.GetByBranchAsync(request.BranchId, cancellationToken);
            cashier = cashiers.FirstOrDefault(c => c.IDContpaq == request.ContpaqId);
        }
        
        if (cashier == null)
        {
            _logger.LogWarning("No se pudo auto-resolver cajera para IDContpaq {ContpaqId} en la sucursal {BranchId}. Usando fallback 'Cajero no validado'.", request.ContpaqId, request.BranchId);
            
            var cashiers = await _cashierService.GetByBranchAsync(request.BranchId, cancellationToken);
            cashier = cashiers.FirstOrDefault(c => c.Name == "Cajero no validado");
            if (cashier == null)
            {
                try
                {
                    var randomEmployeeNumber = 999900 + request.BranchId;
                    var createDto = new CreateCashierDto
                    {
                        EmployeeNumber = randomEmployeeNumber,
                        Name = "Cajero no validado",
                        BranchId = request.BranchId,
                        IDContpaq = 0,
                        IsActive = true
                    };
                    cashier = await _cashierService.CreateAsync(createDto, cancellationToken);
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Error al crear cajera fallback 'Cajero no validado' en sucursal {BranchId}", request.BranchId);
                    var fallbackCashier = await _cashierService.GetByEmployeeNumberAsync(999900 + request.BranchId, cancellationToken);
                    if (fallbackCashier != null)
                    {
                        cashier = fallbackCashier;
                    }
                }
            }
        }
        
        return cashier;
    }
}
