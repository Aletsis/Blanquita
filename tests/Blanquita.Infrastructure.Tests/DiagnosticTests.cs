using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Blanquita.Infrastructure.Persistence.Context;
using Blanquita.Domain.Entities;
using Xunit;
using Xunit.Abstractions;
using Moq;
using Microsoft.Extensions.Logging;
using Blanquita.Application.Interfaces;
using Blanquita.Application.Interfaces.Repositories;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Common;
using Blanquita.Infrastructure.ExternalServices.FoxPro.Repositories;
using Blanquita.Application.DTOs;
using System.Threading.Tasks;

namespace Blanquita.Infrastructure.Tests
{
    public class DiagnosticTests
    {
        private readonly ITestOutputHelper _output;

        public DiagnosticTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestPOS10008FullScan()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var mockReaderFactoryLogger = new Mock<ILogger<FoxProReaderFactory>>();
            var readerFactory = new FoxProReaderFactory(mockReaderFactoryLogger.Object);

            var filePath = @"C:\RespaldoResp_15042025_1930\POS10008.dbf";
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var records = new List<(int Index, DateTime Date, string Folio)>();

            using (var reader = readerFactory.CreateReverseReader(filePath))
            {
                int localCount = 0;
                while (reader.Read())
                {
                    localCount++;
                    var fecha = reader.GetDateTimeSafe("CFECHA");
                    var serie = reader.GetStringSafe("CSERIEDO01")?.Trim() ?? "";
                    var folioNum = reader.GetStringSafe("CFOLIO")?.Trim() ?? "";
                    records.Add((localCount, fecha, $"{serie}-{folioNum}"));
                }
            }

            sw.Stop();
            _output.WriteLine($"Full read took {sw.ElapsedMilliseconds} ms. Total records read: {records.Count}");

            // Print every 20,000th record (since we read reverse, index 1 is the last record in the file)
            _output.WriteLine("=== REVERSE READ DISTRIBUTION (Record 1 is end of file) ===");
            for (int i = 0; i < records.Count; i++)
            {
                if (i == 0 || i % 20000 == 0 || i == records.Count - 1)
                {
                    var r = records[i];
                    _output.WriteLine($"Reverse Read Count {r.Index}/{records.Count}: Date={r.Date:yyyy-MM-dd}, Folio={r.Folio}");
                }
            }
        }
    }
}
