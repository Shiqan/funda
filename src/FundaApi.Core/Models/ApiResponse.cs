namespace FundaApi.Core.Models;

internal record ApiResponse<T>(IEnumerable<T> Objects, int TotaalAantalObjecten, Paging Paging);