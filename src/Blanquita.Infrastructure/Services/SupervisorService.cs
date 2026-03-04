using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Domain.Exceptions;
using Blanquita.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blanquita.Infrastructure.Services;

public class SupervisorService : ISupervisorService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public SupervisorService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    private SupervisorDto MapToDto(ApplicationUser user)
    {
        return new SupervisorDto
        {
            Id = user.Id,
            Name = user.FullName ?? user.UserName ?? string.Empty,
            Username = user.UserName ?? string.Empty,
            BranchId = user.BranchId ?? 0,
            IsActive = user.IsActive
        };
    }

    public async Task<SupervisorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || !await _userManager.IsInRoleAsync(user, "Supervisor"))
            return null;

        return MapToDto(user);
    }

    public async Task<SupervisorDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
        var user = supervisors.FirstOrDefault(u => u.FullName == name);
        if (user == null)
            return null;

        return MapToDto(user);
    }

    public async Task<IEnumerable<SupervisorDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
        return supervisors.Select(MapToDto);
    }

    public async Task<IEnumerable<SupervisorDto>> GetByBranchAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
        return supervisors.Where(u => u.BranchId == branchId).Select(MapToDto);
    }

    public async Task<IEnumerable<SupervisorDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
        return supervisors.Where(u => u.IsActive).Select(MapToDto);
    }

    public async Task<SupervisorDto> CreateAsync(CreateSupervisorDto dto, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByNameAsync(dto.Username);
        if (existingUser != null)
        {
            throw new Exception($"El usuario {dto.Username} ya está en uso.");
        }

        var newUser = new ApplicationUser
        {
            UserName = dto.Username,
            FullName = dto.Name,
            BranchId = dto.BranchId,
            IsActive = dto.IsActive,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(newUser, dto.Password);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Error al crear usuario: {errors}");
        }

        await _userManager.AddToRoleAsync(newUser, "Supervisor");

        return MapToDto(newUser);
    }

    public async Task<SupervisorDto> UpdateAsync(UpdateSupervisorDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(dto.Id.ToString());
        if (user == null)
        {
            throw new EntityNotFoundException("Supervisor", dto.Id);
        }

        user.FullName = dto.Name;
        user.BranchId = dto.BranchId;
        user.IsActive = dto.IsActive;

        if (!string.IsNullOrWhiteSpace(dto.Username) && dto.Username != user.UserName)
        {
            user.UserName = dto.Username;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
             var errors = string.Join(", ", result.Errors.Select(e => e.Description));
             throw new Exception($"Error al actualizar usuario: {errors}");
        }

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passResult = await _userManager.ResetPasswordAsync(user, token, dto.Password);
            if (!passResult.Succeeded)
            {
                 var errors = string.Join(", ", passResult.Errors.Select(e => e.Description));
                 throw new Exception($"Error al actualizar contraseña: {errors}");
            }
        }

        return MapToDto(user);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            throw new EntityNotFoundException("Supervisor", id);
        }

        await _userManager.DeleteAsync(user);
    }

    public async Task<PagedResult<SupervisorDto>> GetPagedAsync(SearchSupervisorRequest request, CancellationToken cancellationToken = default)
    {
        request.Validate();

        var supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
        IEnumerable<ApplicationUser> query = supervisors;

        if (request.HasSearchTerm())
        {
            var searchLower = request.SearchTerm!.ToLower();
            query = query.Where(s =>
                s.FullName != null && s.FullName.ToLower().Contains(searchLower));
        }

        if (request.HasBranchFilter())
        {
            query = query.Where(s => s.BranchId == request.BranchId!.Value);
        }

        if (request.HasActiveFilter())
        {
            query = query.Where(s => s.IsActive == request.IsActive!.Value);
        }

        var totalCount = query.Count();

        if (request.HasSorting())
        {
            query = request.SortColumn?.ToLower() switch
            {
                "name" => request.SortAscending
                    ? query.OrderBy(s => s.FullName)
                    : query.OrderByDescending(s => s.FullName),
                "branchid" => request.SortAscending
                    ? query.OrderBy(s => s.BranchId)
                    : query.OrderByDescending(s => s.BranchId),
                _ => query.OrderBy(s => s.FullName)
            };
        }

        var items = query
            .Skip(request.GetSkip())
            .Take(request.PageSize);

        return PagedResult<SupervisorDto>.Create(
            items.Select(MapToDto),
            totalCount,
            request.Page,
            request.PageSize);
    }
}
