namespace Blanquita.Models
{
    public class PrinterConfiguration
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; } = 9100;
    }
}
