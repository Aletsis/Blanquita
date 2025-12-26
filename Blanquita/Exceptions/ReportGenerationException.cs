using System;

namespace Blanquita.Exceptions
{
    /// <summary>
    /// Excepción que se lanza cuando ocurre un error durante la generación de reportes.
    /// </summary>
    public class ReportGenerationException : Exception
    {
        public ReportGenerationException()
        {
        }

        public ReportGenerationException(string message)
            : base(message)
        {
        }

        public ReportGenerationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
