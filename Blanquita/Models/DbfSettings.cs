namespace Blanquita.Models
{
    public class DbfSettings
    {
        public string DefaultDbfPath { get; set; } = "";
        public string DefaultDbfPathForDocs { get; set; } = "";// Ruta predeterminada
        // Impresora 1
        public string PrinterName { get; set; }
        public string PrinterIp { get; set; }
        public string PrinterPort { get; set; } = "9100";

        // Impresora 2
        public string Printer2Name { get; set; }
        public string Printer2Ip { get; set; }
        public string Printer2Port { get; set; } = "9100";
    }
}
