using static Blanquita.Services.SearchInDbfFileService;
using Blanquita.Models;
using System.Data;

namespace Blanquita.Interfaces
{
    public interface ISearchInDbfFileService
    {
        Task<SearchResult> SearchInDbfFileAsync(
    string filepath,
    string fieldName,
    string searchValue,
    int chunkSize = 1000,
    bool exactMatch = true);

    }
}
