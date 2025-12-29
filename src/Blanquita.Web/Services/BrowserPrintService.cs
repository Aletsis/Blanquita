using Blanquita.Application.DTOs;
using System.Text.RegularExpressions;

namespace Blanquita.Web.Services;

public class BrowserPrintService
{
    public string ConvertZplToHtml(string zpl, LabelDesignDto design)
    {
        //convertir medidas de puntos a pixeles(aproximacion)
        int scaleFactor = 3; //Ajustar a necesidad
        int widthPx = design.GetWidthInDots() / scaleFactor;
        int heightPx = design.GetHeightInDots() / scaleFactor;
        int marginTopPx = design.GetMarginTopInDots() / scaleFactor;
        int marginLeftPx = design.GetMarginLeftInDots() / scaleFactor;

        //Extraer contenido de zpl
        var title = ExtractField(zpl, @"^FO\d+,\d+\^A\w+,\d+,\d+\^FD(.+?)\^FS");
        var barcode = ExtractField(zpl, @"^FO\d+,\d+\^FD(.+?)\^FS");
        var footer = ExtractField(zpl, @"^FO\d+,\d+\^A\w+,\d+,\d+^FD(.+?)\^FS");

        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        @media print {{ 
            body {{ 
                margin: 0;
                padding: 0; 
            }}
            .label {{ 
                width: {widthPx}px;
                height: {heightPx}px;
                border: 1px dotted #ccc;
                position: relative;
                page-break-after: always;
                overflow: hidden;
                font-family: Arial, sans-serif;
            }}
            .title {{
                position: absolute;
                top: {marginTopPx}px;
                left: {marginLeftPx}px;
                font-size: 24px;
                font-weight: bold;
            }}
            .barcode {{
                position: absolute;
                top: {marginTopPx + 40}px;
                left: {marginLeftPx}px;
                font-family: 'Libre Barcode 128', cursive;
                font-size: 54px;
            }}
            .footer {{
                position: absolute;
                bottom: {marginTopPx}px;
                left: {marginLeftPx}px;
                font-size: 18px;
            }}
        }}
    </style>
    <link href='https://fonts.googleapis.com/css?family=Libre+Barcode+128' rel='stylesheet'>
</head>
<body>
    <div class='label'>
        <div class='title'>
            {title}
        </div>
        <div class='barcode'>
            {barcode}
        </div>
        <div class='footer'>
            {footer}
        </div>
    </div>
</body>
</html>
";
    }
    private string ExtractField(string zpl, string pattern)
    {
        var match = Regex.Match(zpl, pattern);
        return match.Success ? match.Groups[1].Value : "";
    }
}
