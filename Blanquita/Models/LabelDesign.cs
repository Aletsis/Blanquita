namespace Blanquita.Models
{
    public class LabelDesign
    {
        public int WidthInDots { get; set; } = 607; //Ancho estandar (2 pulgadas a 300dpi) 203
        public int HeightInDots { get; set; } = 199; //Alto estandar
        public int MarginTop { get; set; } = 20;
        public int MarginLeft { get; set; } = 20;
        public string Orientation { get; set; } = "N"; //N= normal, R= rotado 90°
    }
}
