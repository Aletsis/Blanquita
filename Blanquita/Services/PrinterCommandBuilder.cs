using Blanquita.Interfaces;
using Blanquita.Models;
using System.Globalization;
using System.Text;

namespace Blanquita.Services
{
    public class PrinterCommandBuilder : IPrinterCommandBuilder
    {
        public byte[] InitializePrinter() => new byte[] { 0x1B, 0x40 };
        public byte[] CutPaper() => new byte[] { 0x1D, 0x56, 66, 0 };
        public byte[] Text(string text) => Encoding.ASCII.GetBytes(text);
        public byte[] BoldOn() => new byte[] { 0x1B, 0x45, 1 };
        public byte[] BoldOff() => new byte[] { 0x1B, 0x45, 0 };
        public byte[] AlignLeft() => new byte[] { 0x1B, 0x61, 0 };
        public byte[] AlignCenter() => new byte[] { 0x1B, 0x61, 1 };
        public byte[] AlignRight() => new byte[] { 0x1B, 0x61, 2 };
        private static string Moneda(decimal valor) =>
            valor.ToString("C", CultureInfo.CreateSpecificCulture("es-MX"));

        private void AddDenomination(List<byte> cmds, int denom, int qty)
        {
            if (qty > 0)
            {
                int total = denom * qty;
                cmds.AddRange(Text($"${denom,-18}{qty,-13}{Moneda(total)}\n"));
            }
        }
        public List<byte> BuildTicket(PrintTicketData data)
        {
            var cmds = new List<byte>();
            int total = 0;

            cmds.AddRange(InitializePrinter());
            cmds.AddRange(AlignCenter());
            cmds.AddRange(BoldOn());
            cmds.AddRange(Text("CARNICERIAS LA BLANQUITA\n"));
            cmds.AddRange(BoldOff());
            cmds.AddRange(Text("RECOLECCION DE EFECTIVO\n"));
            cmds.AddRange(Text($"{data.Sucursal}\n\n"));
            cmds.AddRange(AlignLeft());
            cmds.AddRange(Text("Fecha: " + DateTime.Now.ToString("g") + "\n"));
            cmds.AddRange(Text($"{data.Caja}\n"));
            cmds.AddRange(Text($"Recoleccion {data.Folio}\n"));
            cmds.AddRange(Text("DENOMINACION      CANTIDAD        TOTAL\n"));
            cmds.AddRange(Text("------------------------------------------\n"));

            void AddLine(int denom, int qty)
            {
                if (qty > 0)
                {
                    int lineTotal = denom * qty;
                    total += lineTotal;
                    // Formatea con espacios para alineación simple
                    cmds.AddRange(Text($"${denom,-18}{qty,-13}${lineTotal}.00\n"));
                }
            }
            AddLine(1000, data.CantMil);
            AddLine(500, data.CantQuinientos);
            AddLine(200, data.CantDoscientos);
            AddLine(100, data.CantCien);
            AddLine(50, data.CantCincuenta);
            AddLine(20, data.CantVeinte);

            cmds.AddRange(Text("----------------------------------------\n"));
            cmds.AddRange(BoldOn());
            cmds.AddRange(Text($"TOTAL:            ${total}.00\n\n\n\n"));
            cmds.AddRange(BoldOff());
            cmds.AddRange(Text("---------------             --------------\n"));
            cmds.AddRange(BoldOn());

            string encargada = data.Encargada.Length > 15 ? data.Encargada.Substring(0, 15) : data.Encargada.PadRight(15);
            string cajera = data.Cajera.Length > 14 ? data.Cajera.Substring(0, 14) : data.Cajera.PadRight(14);
            cmds.AddRange(Text($"{encargada}            {cajera}\n\n\n"));
            cmds.AddRange(CutPaper());

            return cmds;
        }
        public List<byte> BuildCorteCaja(PrintCorteData data)
        {
            var cmds = new List<byte>();

            cmds.AddRange(InitializePrinter());
            cmds.AddRange(AlignCenter());
            cmds.AddRange(BoldOn());
            cmds.AddRange(Text("CARNICERIAS LA BLANQUITA\n"));
            cmds.AddRange(BoldOff());
            cmds.AddRange(Text("CORTE DE CAJA\n"));
            cmds.AddRange(Text($"{data.Sucursal}\n\n"));
            cmds.AddRange(AlignLeft());
            cmds.AddRange(Text($"Fecha: {data.FechaHora:g}\n"));
            cmds.AddRange(Text($"{data.Caja}\n"));
            cmds.AddRange(Text("DENOMINACION      CANTIDAD        TOTAL \n"));
            cmds.AddRange(Text("------------------------------------------\n"));

            AddDenomination(cmds, 1000, data.TotalM);
            AddDenomination(cmds, 500, data.TotalQ);
            AddDenomination(cmds, 200, data.TotalD);
            AddDenomination(cmds, 100, data.TotalC);
            AddDenomination(cmds, 50, data.TotalCi);
            AddDenomination(cmds, 20, data.TotalV);

            cmds.AddRange(Text("----------------------------------------\n"));
            cmds.AddRange(BoldOn());
            cmds.AddRange(Text($"TOTAL RECOLECCIONES:          {Moneda(data.GranTotal)}\n"));
            cmds.AddRange(Text($"TOTAL TIRA:                   {Moneda(data.TotalTira)}\n"));
            cmds.AddRange(Text($"TOTAL TARJETAS:               {Moneda(data.TotalTarjetas)}\n\n"));

            decimal efectivo = data.TotalTira - (data.TotalTarjetas + data.GranTotal);
            cmds.AddRange(Text($"EFECTIVO A ENTREGAR:          {Moneda(efectivo)}\n\n\n\n"));

            cmds.AddRange(BoldOff());
            cmds.AddRange(Text("---------------            ---------------\n"));
            cmds.AddRange(BoldOn());

            string encargada = string.IsNullOrWhiteSpace(data.Encargada)
                ? "".PadRight(16)
                : (data.Encargada.Length > 15 ? data.Encargada[..15] : data.Encargada.PadRight(15));
            string cajera = string.IsNullOrWhiteSpace(data.Cajera)
                ? "".PadRight(16)
                : (data.Cajera.Length > 15 ? data.Cajera[..15] : data.Cajera.PadRight(15));
            cmds.AddRange(Text($"{encargada}            {cajera}\n\n\n"));
            cmds.AddRange(CutPaper());

            return cmds;
        }
    }
}
