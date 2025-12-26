using Blanquita.Models;

namespace Blanquita.Interfaces
{
    public interface IPrinterCommandBuilder
    {
        byte[] InitializePrinter();
        byte[] CutPaper();
        byte[] Text(string text);
        byte[] BoldOn();
        byte[] BoldOff();
        byte[] AlignLeft();
        byte[] AlignCenter();
        byte[] AlignRight();

        List<byte> BuildTicket(PrintTicketData data);
        List<byte> BuildCorteCaja(PrintCorteData data);
    }
}
