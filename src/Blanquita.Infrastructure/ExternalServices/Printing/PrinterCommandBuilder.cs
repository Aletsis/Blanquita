using Blanquita.Application.DTOs;
using System.Globalization;
using System.Text;

namespace Blanquita.Infrastructure.ExternalServices.Printing;

public class PrinterCommandBuilder
{
    private readonly CultureInfo _culturaMX = new("es-MX");

    public byte[] InitializePrinter() => new byte[] { 0x1B, 0x40 };
    public byte[] CutPaper() => new byte[] { 0x1D, 0x56, 66, 0 };
    public byte[] Text(string text) => Encoding.ASCII.GetBytes(text);
    public byte[] BoldOn() => new byte[] { 0x1B, 0x45, 1 };
    public byte[] BoldOff() => new byte[] { 0x1B, 0x45, 0 };
    public byte[] AlignLeft() => new byte[] { 0x1B, 0x61, 0 };
    public byte[] AlignCenter() => new byte[] { 0x1B, 0x61, 1 };
    public byte[] AlignRight() => new byte[] { 0x1B, 0x61, 2 };

    private string FormatMoney(decimal value) =>
        value.ToString("C", _culturaMX);

    private void AddDenomination(List<byte> cmds, int denomination, int quantity)
    {
        if (quantity > 0)
        {
            int total = denomination * quantity;
            cmds.AddRange(Text($"${denomination,-18}{quantity,-13}{FormatMoney(total)}\n"));
        }
    }

    public List<byte> BuildCashCollectionTicket(CashCollectionDto collection)
    {
        var cmds = new List<byte>();
        int total = 0;

        cmds.AddRange(InitializePrinter());
        cmds.AddRange(AlignCenter());
        cmds.AddRange(BoldOn());
        cmds.AddRange(Text("CARNICERIAS LA BLANQUITA\n"));
        cmds.AddRange(BoldOff());
        cmds.AddRange(Text("RECOLECCION DE EFECTIVO\n\n"));
        cmds.AddRange(AlignLeft());
        cmds.AddRange(Text("Fecha: " + collection.CollectionDateTime.ToString("g") + "\n"));
        cmds.AddRange(Text($"{collection.CashRegisterName}\n"));
        cmds.AddRange(Text($"Recoleccion {collection.Folio}\n"));
        cmds.AddRange(Text("DENOMINACION      CANTIDAD        TOTAL\n"));
        cmds.AddRange(Text("------------------------------------------\n"));

        void AddLine(int denom, int qty)
        {
            if (qty > 0)
            {
                int lineTotal = denom * qty;
                total += lineTotal;
                cmds.AddRange(Text($"${denom,-18}{qty,-13}${lineTotal}.00\n"));
            }
        }

        AddLine(1000, collection.Thousands);
        AddLine(500, collection.FiveHundreds);
        AddLine(200, collection.TwoHundreds);
        AddLine(100, collection.Hundreds);
        AddLine(50, collection.Fifties);
        AddLine(20, collection.Twenties);

        cmds.AddRange(Text("----------------------------------------\n"));
        cmds.AddRange(BoldOn());
        cmds.AddRange(Text($"TOTAL:            ${total}.00\n\n\n\n"));
        cmds.AddRange(BoldOff());
        cmds.AddRange(Text("---------------             --------------\n"));
        cmds.AddRange(BoldOn());

        string supervisor = collection.SupervisorName.Length > 15 
            ? collection.SupervisorName[..15] 
            : collection.SupervisorName.PadRight(15);
        string cashier = collection.CashierName.Length > 14 
            ? collection.CashierName[..14] 
            : collection.CashierName.PadRight(14);
        
        cmds.AddRange(Text($"{supervisor}            {cashier}\n\n\n"));
        cmds.AddRange(CutPaper());

        return cmds;
    }

    public List<byte> BuildCashCutTicket(CashCutDto cashCut)
    {
        var cmds = new List<byte>();

        cmds.AddRange(InitializePrinter());
        cmds.AddRange(AlignCenter());
        cmds.AddRange(BoldOn());
        cmds.AddRange(Text("CARNICERIAS LA BLANQUITA\n"));
        cmds.AddRange(BoldOff());
        cmds.AddRange(Text("CORTE DE CAJA\n"));
        cmds.AddRange(Text($"{cashCut.BranchName}\n\n"));
        cmds.AddRange(AlignLeft());
        cmds.AddRange(Text($"Fecha: {cashCut.CutDateTime:g}\n"));
        cmds.AddRange(Text($"{cashCut.CashRegisterName}\n"));
        cmds.AddRange(Text("DENOMINACION      CANTIDAD        TOTAL \n"));
        cmds.AddRange(Text("------------------------------------------\n"));

        AddDenomination(cmds, 1000, cashCut.TotalThousands);
        AddDenomination(cmds, 500, cashCut.TotalFiveHundreds);
        AddDenomination(cmds, 200, cashCut.TotalTwoHundreds);
        AddDenomination(cmds, 100, cashCut.TotalHundreds);
        AddDenomination(cmds, 50, cashCut.TotalFifties);
        AddDenomination(cmds, 20, cashCut.TotalTwenties);

        cmds.AddRange(Text("----------------------------------------\n"));
        cmds.AddRange(BoldOn());
        cmds.AddRange(Text($"TOTAL RECOLECCIONES:          {FormatMoney(cashCut.GrandTotal)}\n"));
        cmds.AddRange(Text($"TOTAL TIRA:                   {FormatMoney(cashCut.TotalSlips)}\n"));
        cmds.AddRange(Text($"TOTAL TARJETAS:               {FormatMoney(cashCut.TotalCards)}\n\n"));

        decimal efectivo = cashCut.TotalSlips - cashCut.TotalCards - cashCut.GrandTotal;
        cmds.AddRange(Text($"EFECTIVO A ENTREGAR:          {FormatMoney(efectivo)}\n\n\n\n"));

        cmds.AddRange(BoldOff());
        cmds.AddRange(Text("---------------            ---------------\n"));
        cmds.AddRange(BoldOn());

        string supervisor = string.IsNullOrWhiteSpace(cashCut.SupervisorName)
            ? "".PadRight(16)
            : (cashCut.SupervisorName.Length > 15 ? cashCut.SupervisorName[..15] : cashCut.SupervisorName.PadRight(15));
        string cashier = string.IsNullOrWhiteSpace(cashCut.CashierName)
            ? "".PadRight(16)
            : (cashCut.CashierName.Length > 15 ? cashCut.CashierName[..15] : cashCut.CashierName.PadRight(15));
        
        cmds.AddRange(Text($"{supervisor}            {cashier}\n\n\n"));
        cmds.AddRange(CutPaper());

        return cmds;
    }

    public List<byte> BuildCustomTicket(TicketDto ticket)
    {
        var cmds = new List<byte>();

        cmds.AddRange(InitializePrinter());
        cmds.AddRange(AlignCenter());
        cmds.AddRange(BoldOn());
        cmds.AddRange(Text(ticket.Title + "\n"));
        cmds.AddRange(BoldOff());
        cmds.AddRange(AlignLeft());

        foreach (var line in ticket.Lines)
        {
            cmds.AddRange(Text(line + "\n"));
        }

        cmds.AddRange(Text("\n\n"));
        cmds.AddRange(CutPaper());

        return cmds;
    }
}
