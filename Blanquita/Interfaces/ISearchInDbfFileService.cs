using static Blanquita.Services.SearchInDbfFileService;
using Blanquita.Models;
using System.Data;

namespace Blanquita.Interfaces
{
    public interface ISearchInDbfFileService
    {
        // Existing methods with CancellationToken support
        Task<SearchResult> SearchInDbfFileAsync(
            string filepath,
            string fieldName,
            string searchValue,
            int chunkSize = 1000,
            bool exactMatch = true,
            int maxMemoryMB = 500,
            CancellationToken cancellationToken = default);

        Task<SearchResult> SearchDocsInDbfFile(
            string filepath,
            string[] fieldNames,
            string[] searchValues,
            Action<int> progressCallback = null,
            int chunkSize = 1000,
            bool exactMatch = true,
            int maxMemoryMB = 500,
            CancellationToken cancellationToken = default);

        // New streaming methods
        IAsyncEnumerable<DataRow> SearchInDbfFileStreamAsync(
            string filepath,
            string fieldName,
            string searchValue,
            bool exactMatch = true,
            CancellationToken cancellationToken = default);

        IAsyncEnumerable<DataRow> SearchDocsInDbfFileStreamAsync(
            string filepath,
            string[] fieldNames,
            string[] searchValues,
            bool exactMatch = true,
            CancellationToken cancellationToken = default);
    }
}
