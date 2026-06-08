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
    public byte[] FontA() => new byte[] { 0x1B, 0x4D, 0x00 };
    public byte[] FontB() => new byte[] { 0x1B, 0x4D, 0x01 };
    public byte[] NormalSize() => new byte[] { 0x1B, 0x21, 0x00 };
    public byte[] DoubleHeight() => new byte[] { 0x1B, 0x21, 0x10 };
    public byte[] DoubleWidth() => new byte[] { 0x1B, 0x21, 0x20 };
    public byte[] LargeSize() => new byte[] { 0x1B, 0x21, 0x30 };

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
        cmds.AddRange(Text($"TOTAL BANBAJIO:               {FormatMoney(cashCut.TotalBanbajio)}\n"));
        cmds.AddRange(Text($"TOTAL BANREGIO:               {FormatMoney(cashCut.TotalBanregio)}\n"));
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
    public List<byte> BuildPedidoTicket(PedidoDto pedidoDto)
    {
        var cmds = new List<byte>();

        cmds.AddRange(InitializePrinter());
        cmds.AddRange(AlignCenter());
        cmds.AddRange(BoldOn());
        cmds.AddRange(Text("CARNICERIAS LA BLANQUITA\n"));
        cmds.AddRange(BoldOff());
        cmds.AddRange(FontB());
        cmds.AddRange(BoldOn());
        cmds.AddRange(AlignCenter());
        cmds.AddRange(Text("Maria Irene Meade Garfias\n"));
        cmds.AddRange(Text("RFC: MEGI520203G2A\n"));
        cmds.AddRange(BoldOff());
        cmds.AddRange(Text("Negrete #108,Zona Centro\n"));
        cmds.AddRange(Text("Soledad de Graciano Sanchez, CP: 78433\n"));
        cmds.AddRange(Text("mail: blanquita8soledad@outlook.com\n"));
        cmds.AddRange(Text("Tel: 4448310535-4448316184-4448310193\n")); 
        cmds.AddRange(Text("\n"));

        cmds.AddRange(Text($"Fecha: {pedidoDto.Fecha.ToShortDateString()}\n"));
        cmds.AddRange(Text("\n"));
        cmds.AddRange(AlignLeft());
        cmds.AddRange(FontA());
        cmds.AddRange(BoldOn());
        cmds.AddRange(Text($"Pedido No: {pedidoDto.Folio}\n"));
        cmds.AddRange(Text($"Codigo: {pedidoDto.ClienteCodigo}\n"));
        cmds.AddRange(Text($"Nombre: {pedidoDto.Cliente}\n"));
        cmds.AddRange(Text($"Domicilio: {pedidoDto.Domicilio}\n"));
        cmds.AddRange(Text($"Colonia: {pedidoDto.Colonia}\n"));
        cmds.AddRange(Text("\n"));

        cmds.AddRange(FontB());
        cmds.AddRange(Text("Producto                 Kilos   Precio       Importe\n"));
        cmds.AddRange(BoldOff());
        cmds.AddRange(Text("-------------------------------------------------------\n"));
        

        if (pedidoDto.Items != null)
        {
            foreach (var detail in pedidoDto.Items)
            {
                string desc = string.IsNullOrEmpty(detail.Descripcion) ? detail.Codigo : detail.Descripcion;
                if (desc.Length > 20) desc = desc.Substring(0, 20);

                var Price = detail.Cantidad > 0 ? (detail.Total / (decimal)detail.Cantidad) : detail.Precio;
                cmds.AddRange(Text($"{desc,-20} {detail.Cantidad,5:0.##} {FormatMoney(Price),10} {FormatMoney(detail.Total),14}\n"));
            }
        }

        cmds.AddRange(Text("-----------------------------------------------------\n"));
        cmds.AddRange(AlignRight());
        cmds.AddRange(BoldOn());
        cmds.AddRange(Text($"SUBTOTAL:              {FormatMoney(pedidoDto.NetAmount),11}\n"));
        cmds.AddRange(Text($"IVA:                   {FormatMoney(pedidoDto.TaxAmount),11}\n"));
        cmds.AddRange(Text($"TOTAL:                 {FormatMoney(pedidoDto.Total),11}\n"));
        cmds.AddRange(BoldOff());
        cmds.AddRange(Text("\n\n\n"));
        cmds.AddRange(AlignLeft());
        cmds.AddRange(FontA());
        cmds.AddRange(Text("    Salida           Firma Cliente\n"));
        cmds.AddRange(Text("\n\n"));
        cmds.AddRange(Text("----------------------------------------\n"));
        cmds.AddRange(FontB());
        cmds.AddRange(Text($"Hora: {pedidoDto.Fecha.ToShortTimeString()}\n"));
        cmds.AddRange(Text($"Repartidor: Vendedor de Piso PV\n"));
        cmds.AddRange(Text("\n\n\n"));
        cmds.AddRange(AlignCenter());
        cmds.AddRange(BoldOn());
        cmds.AddRange(Text("***MANTENGASE EN REFRIGERACION***\n"));
        cmds.AddRange(Text("\n"));
        cmds.AddRange(BoldOff());
        cmds.AddRange(Text("Favor de revisar su mercancia antes de que se retire\n"));
        cmds.AddRange(Text("el repartidor, no se aceptan devoluciones de\n"));
        cmds.AddRange(Text("producto una vez que se ha recibido de\n"));
        cmds.AddRange(Text("conformidad su mercancia.\n"));
        cmds.AddRange(Text("\n"));
        cmds.AddRange(BoldOn());
        cmds.AddRange(Text("RECUERDE QUE NUESTROS PRODUCTOS SON\n"));
        cmds.AddRange(Text("PEDECEDEROS Y REQUIEREN REFRIGERACION\n"));
        cmds.AddRange(Text("Y MANEJO ADECUADO.\n"));


        cmds.AddRange(Text("\n\n\n\n"));
        cmds.AddRange(CutPaper());

        return cmds;
    }
    public List<byte> BuildReturnTicket(ReturnDto returnDto)
    {
        var cmds = new List<byte>();

        cmds.AddRange(InitializePrinter());
        cmds.AddRange(AlignCenter());
        cmds.AddRange(BoldOn());
        cmds.AddRange(Text("CARNICERIAS LA BLANQUITA\n"));
        cmds.AddRange(BoldOff());
        cmds.AddRange(FontB());
        cmds.AddRange(BoldOn());
        cmds.AddRange(AlignCenter());
        cmds.AddRange(Text("Maria Irene Meade Garfias\n"));
        cmds.AddRange(Text("RFC: MEGI520203G2A\n"));
        cmds.AddRange(BoldOff());
        cmds.AddRange(Text("Negrete #108,Zona Centro\n"));
        cmds.AddRange(Text("Soledad de Graciano Sanchez, CP: 78433\n"));
        cmds.AddRange(Text("mail: blanquita8soledad@outlook.com\n"));
        cmds.AddRange(Text("Tel: 4448310535-4448316184-4448310193\n"));
        //cmds.AddRange(Text("TICKET DE DEVOLUCION\n\n"));
        //cmds.AddRange(AlignLeft());

        cmds.AddRange(Text($"Fecha: {returnDto.Date.ToShortDateString()}\n"));
        //cmds.AddRange(Text($"Serie: {returnDto.Series}   Folio: {returnDto.Folio}\n"));
        cmds.AddRange(AlignLeft());
        cmds.AddRange(FontA());
        cmds.AddRange(BoldOn());
        cmds.AddRange(Text($"Devolucion No: {returnDto.Series}{returnDto.Folio}\n"));
        //cmds.AddRange(Text($"Fecha: {returnDto.Date.ToShortDateString()}   Hora: {returnDto.FormattedTime}\n\n"));

        //cmds.AddRange(Text("CANT  DESCRIPCION                IMPORTE\n"));
        cmds.AddRange(FontB());
        cmds.AddRange(Text("Producto                   Kilos      Precio    Importe\n"));
        cmds.AddRange(BoldOff());
        cmds.AddRange(Text("-----------------------------------------------------\n"));


        foreach (var detail in returnDto.Details)
        {
            string desc = string.IsNullOrEmpty(detail.ProductName) ? detail.ProductId : detail.ProductName;
            if (desc.Length > 20) desc = desc.Substring(0, 20);

            // Format: Qty (5 chars) + Desc (22 chars) + Total (13 chars)
            // Example: 1.00  COCA COLA 600ML        $25.00

            //cmds.AddRange(Text($"{detail.Units,-5:0.##} {desc,-20} {FormatMoney(detail.Total),11}\n"));
            var Price = detail.Total / (decimal)detail.Units;
            cmds.AddRange(Text($"{desc,-20} {detail.Units,3:0.##} {FormatMoney(Price),12} {FormatMoney(detail.Total),16}\n"));
        }

        cmds.AddRange(Text("-----------------------------------------------------\n"));
        cmds.AddRange(AlignRight());
        cmds.AddRange(BoldOn());
        cmds.AddRange(Text($"SUBTOTAL:              {FormatMoney(returnDto.Net),11}\n"));
        cmds.AddRange(Text($"IVA:                   {FormatMoney(returnDto.Tax),11}\n"));
        cmds.AddRange(Text($"TOTAL:                 {FormatMoney(returnDto.Total),11}\n"));
        cmds.AddRange(BoldOff());
        cmds.AddRange(Text("\n\n\n"));
        cmds.AddRange(AlignLeft());
        cmds.AddRange(FontA());
        cmds.AddRange(Text("    Salida           Firma Cliente\n"));
        cmds.AddRange(Text("\n\n"));
        cmds.AddRange(Text("----------------------------------------\n"));
        cmds.AddRange(FontB());
        cmds.AddRange(Text($"Hora: {returnDto.FormattedTime}\n"));
        cmds.AddRange(Text($"Repartidor: Vendedor de Piso PV\n"));
        cmds.AddRange(Text("\n\n\n"));
        cmds.AddRange(AlignCenter());
        cmds.AddRange(BoldOn());
        cmds.AddRange(Text("***MANTENGASE EN REFRIGERACION***\n"));
        cmds.AddRange(Text("\n"));
        cmds.AddRange(BoldOff());
        cmds.AddRange(Text("Favor de revisar su mercancia antes de que se retire\n"));
        cmds.AddRange(Text("el repartidor, no se aceptan devoluciones de\n"));
        cmds.AddRange(Text("producto una vez que se ha recibido de\n"));
        cmds.AddRange(Text("conformidad su mercancia.\n"));
        cmds.AddRange(Text("\n"));
        cmds.AddRange(BoldOn());
        cmds.AddRange(Text("RECUERDE QUE NUESTROS PRODUCTOS SON\n"));
        cmds.AddRange(Text("PEDECEDEROS Y REQUIEREN REFRIGERACION\n"));
        cmds.AddRange(Text("Y MANEJO ADECUADO.\n"));


        cmds.AddRange(Text("\n\n\n\n"));
        cmds.AddRange(CutPaper());

        return cmds;
    }
}
