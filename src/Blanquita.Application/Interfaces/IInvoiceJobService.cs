using System.Threading.Tasks;

namespace Blanquita.Application.Interfaces;

public interface IInvoiceJobService
{
    /// <summary>
    /// Ejecuta el proceso de búsqueda y envío de facturas pendientes a clientes.
    /// </summary>
    Task ProcessAndSendInvoicesAsync();
}
