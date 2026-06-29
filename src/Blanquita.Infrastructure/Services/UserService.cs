using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Blanquita.Domain.Repositories;
using Blanquita.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Blanquita.Infrastructure.Services;

/// <summary>
/// Implementación limpia del servicio de administración de usuarios utilizando ASP.NET Core Identity
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IBranchRepository _branchRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IBranchRepository branchRepository,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _branchRepository = branchRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<UserDto>> GetAllAsync()
    {
        try
        {
            var users = await _userManager.Users.ToListAsync();
            var list = new List<UserDto>();

            var branches = await _branchRepository.GetAllAsync();
            var branchDict = branches.ToDictionary(b => b.Id, b => b.Name);

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                list.Add(new UserDto
                {
                    Id = u.Id,
                    Username = u.UserName ?? string.Empty,
                    FullName = u.FullName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    PhoneNumber = u.PhoneNumber ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? "Ninguno",
                    BranchId = u.BranchId,
                    BranchName = u.BranchId.HasValue && branchDict.TryGetValue(u.BranchId.Value, out var name) ? name : "Ninguna",
                    EmployeeNumber = u.EmployeeNumber
                });
            }

            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los usuarios del sistema.");
            return new List<UserDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<UserDto?> GetByIdAsync(string id)
    {
        try
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return null;

            var roles = await _userManager.GetRolesAsync(u);
            string? branchName = null;
            if (u.BranchId.HasValue)
            {
                var branch = await _branchRepository.GetByIdAsync(u.BranchId.Value);
                branchName = branch?.Name;
            }

            return new UserDto
            {
                Id = u.Id,
                Username = u.UserName ?? string.Empty,
                FullName = u.FullName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "Ninguno",
                BranchId = u.BranchId,
                BranchName = branchName ?? "Ninguna",
                EmployeeNumber = u.EmployeeNumber
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el usuario con Id {UserId}", id);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        try
        {
            var u = await _userManager.FindByNameAsync(username);
            if (u == null) return null;

            var roles = await _userManager.GetRolesAsync(u);
            string? branchName = null;
            if (u.BranchId.HasValue)
            {
                var branch = await _branchRepository.GetByIdAsync(u.BranchId.Value);
                branchName = branch?.Name;
            }

            return new UserDto
            {
                Id = u.Id,
                Username = u.UserName ?? string.Empty,
                FullName = u.FullName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "Ninguno",
                BranchId = u.BranchId,
                BranchName = branchName ?? "Ninguna",
                EmployeeNumber = u.EmployeeNumber
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el usuario con nombre de usuario {Username}", username);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> CreateAsync(CreateUserDto dto)
    {
        try
        {
            // Validación de sucursal obligatoria para roles no administradores
            if (dto.Role != "Admin" && (!dto.BranchId.HasValue || dto.BranchId.Value <= 0))
            {
                throw new ArgumentException("La sucursal es obligatoria para usuarios que no son administradores.");
            }

            // Valida el número de teléfono con la validación de dominio antes de crear el usuario
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                Blanquita.Domain.ValueObjects.PhoneNumber.Create(dto.PhoneNumber);
            }

            var user = new ApplicationUser
            {
                UserName = dto.Username,
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                EmailConfirmed = true,
                BranchId = dto.Role == "Admin" ? null : dto.BranchId,
                EmployeeNumber = dto.EmployeeNumber
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Fallo al crear usuario {Username}: {Errors}", dto.Username, errors);
                throw new Exception($"Error de creación: {errors}");
            }

            if (!string.IsNullOrEmpty(dto.Role))
            {
                await _userManager.AddToRoleAsync(user, dto.Role);
            }

            _logger.LogInformation("Usuario {Username} creado exitosamente.", dto.Username);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción durante la creación del usuario {Username}", dto.Username);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(UpdateUserDto dto)
    {
        try
        {
            // Validación de sucursal obligatoria para roles no administradores
            if (dto.Role != "Admin" && (!dto.BranchId.HasValue || dto.BranchId.Value <= 0))
            {
                throw new ArgumentException("La sucursal es obligatoria para usuarios que no son administradores.");
            }

            // Valida el número de teléfono con la validación de dominio antes de actualizar
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                Blanquita.Domain.ValueObjects.PhoneNumber.Create(dto.PhoneNumber);
            }

            var user = await _userManager.FindByIdAsync(dto.Id);
            if (user == null)
            {
                _logger.LogWarning("No se encontró el usuario con Id {UserId} para actualizar.", dto.Id);
                return false;
            }

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.BranchId = dto.Role == "Admin" ? null : dto.BranchId;
            user.EmployeeNumber = dto.EmployeeNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Fallo al actualizar usuario {Username}: {Errors}", user.UserName, errors);
                throw new Exception($"Error de actualización: {errors}");
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, dto.Password);
                if (!passwordResult.Succeeded)
                {
                    var errors = string.Join(", ", passwordResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Actualizado el perfil de {Username} pero falló el reseteo de contraseña: {Errors}", user.UserName, errors);
                    throw new Exception($"Perfil actualizado, pero falló la contraseña: {errors}");
                }
            }

            if (!string.IsNullOrEmpty(dto.Role))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (!currentRoles.Contains(dto.Role))
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, dto.Role);
                }
            }

            _logger.LogInformation("Usuario {Username} actualizado exitosamente.", user.UserName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción durante la actualización del usuario con Id {UserId}", dto.Id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("No se encontró el usuario con Id {UserId} para eliminar.", id);
                return false;
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario {Username} eliminado exitosamente.", user.UserName);
                return true;
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Fallo al eliminar usuario {Username}: {Errors}", user.UserName, errors);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción durante la eliminación del usuario con Id {UserId}", id);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetRolesAsync()
    {
        try
        {
            return await _roleManager.Roles
                .Select(r => r.Name ?? string.Empty)
                .Where(n => n != string.Empty)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la lista de roles del sistema.");
            return new List<string>();
        }
    }

    /// <inheritdoc/>
    public async Task<PagedResult<UserDto>> GetPagedAsync(SearchUserRequest request)
    {
        try
        {
            request.Validate();

            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(u =>
                    (u.UserName != null && u.UserName.ToLower().Contains(term)) ||
                    (u.FullName != null && u.FullName.ToLower().Contains(term)) ||
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.EmployeeNumber.HasValue && u.EmployeeNumber.Value.ToString().Contains(term))
                );
            }

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(request.SortColumn))
            {
                query = request.SortColumn.ToLower() switch
                {
                    "username" => request.SortAscending ? query.OrderBy(u => u.UserName) : query.OrderByDescending(u => u.UserName),
                    "fullname" => request.SortAscending ? query.OrderBy(u => u.FullName) : query.OrderByDescending(u => u.FullName),
                    "email" => request.SortAscending ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
                    "phonenumber" => request.SortAscending ? query.OrderBy(u => u.PhoneNumber) : query.OrderByDescending(u => u.PhoneNumber),
                    "branchid" => request.SortAscending ? query.OrderBy(u => u.BranchId) : query.OrderByDescending(u => u.BranchId),
                    "employeenumber" => request.SortAscending ? query.OrderBy(u => u.EmployeeNumber) : query.OrderByDescending(u => u.EmployeeNumber),
                    _ => request.SortAscending ? query.OrderBy(u => u.Id) : query.OrderByDescending(u => u.Id)
                };
            }
            else
            {
                query = query.OrderBy(u => u.UserName);
            }

            var pagedUsers = await query
                .Skip(request.GetSkip())
                .Take(request.PageSize)
                .ToListAsync();

            var list = new List<UserDto>();
            var branches = await _branchRepository.GetAllAsync();
            var branchDict = branches.ToDictionary(b => b.Id, b => b.Name);

            foreach (var u in pagedUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                list.Add(new UserDto
                {
                    Id = u.Id,
                    Username = u.UserName ?? string.Empty,
                    FullName = u.FullName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    PhoneNumber = u.PhoneNumber ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? "Ninguno",
                    BranchId = u.BranchId,
                    BranchName = u.BranchId.HasValue && branchDict.TryGetValue(u.BranchId.Value, out var name) ? name : "Ninguna",
                    EmployeeNumber = u.EmployeeNumber
                });
            }

            if (!string.IsNullOrWhiteSpace(request.SortColumn) && request.SortColumn.ToLower() == "role")
            {
                list = request.SortAscending 
                    ? list.OrderBy(u => u.Role).ToList() 
                    : list.OrderByDescending(u => u.Role).ToList();
            }

            return PagedResult<UserDto>.Create(list, totalCount, request.Page, request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios paginados.");
            return PagedResult<UserDto>.Empty(request.Page, request.PageSize);
        }
    }
}
