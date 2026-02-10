using Blanquita.Application.DTOs;
using MediatR;

namespace Blanquita.Application.Commands.Printing;

public record PrintReturnTicketCommand(ReturnDto ReturnDto, int CashRegisterId = 0) : IRequest<bool>;
