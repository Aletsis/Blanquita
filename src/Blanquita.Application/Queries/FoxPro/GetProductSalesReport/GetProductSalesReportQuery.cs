using System;
using System.Collections.Generic;
using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Queries.FoxPro.GetProductSalesReport;

public record GetProductSalesReportQuery(
    DateTime StartDate,
    DateTime EndDate,
    IEnumerable<string> ProductCodes,
    int BranchId) : IRequest<IEnumerable<ProductSalesReportDto>>;
