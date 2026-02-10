namespace Blanquita.Application.DTOs;

public class ReturnDto
{
    public string IdDocument { get; set; } = string.Empty;
    public string Series { get; set; } = string.Empty;
    public string Folio { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Time { get; set; } = string.Empty;

    public string FormattedTime
    {
        get
        {
            if (string.IsNullOrEmpty(Time)) return string.Empty;
            
            // Ensure we have at least 6 characters, padding with zeros if necessary
            // Assuming the time is stored as HHMMSS in a string/number format
            var cleanTime = Time.Trim();
            
            // Handle cases where it might be a float string (though unlikely given context)
            if (cleanTime.Contains(".")) cleanTime = cleanTime.Split('.')[0];
            
            if (cleanTime.Length < 6)
                cleanTime = cleanTime.PadLeft(6, '0');
                
            if (cleanTime.Length >= 6)
            {
                return $"{cleanTime.Substring(0, 2)}:{cleanTime.Substring(2, 2)}:{cleanTime.Substring(4, 2)}";
            }
            
            return Time;
        }
    }
    public decimal Net { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public List<ReturnDetailDto> Details { get; set; } = new();
}
