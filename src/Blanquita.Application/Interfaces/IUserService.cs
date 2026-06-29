using System.Collections.Generic;
using System.Threading.Tasks;
using Blanquita.Application.DTOs;

namespace Blanquita.Application.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(string id);
    Task<UserDto?> GetByUsernameAsync(string username);
    Task<bool> CreateAsync(CreateUserDto dto);
    Task<bool> UpdateAsync(UpdateUserDto dto);
    Task<bool> DeleteAsync(string id);
    Task<List<string>> GetRolesAsync();
    Task<PagedResult<UserDto>> GetPagedAsync(SearchUserRequest request);
}
