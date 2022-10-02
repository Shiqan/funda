namespace FundaApi.LiteDb.Models;

public class AggregateResult<T>
{
    public string Id { get; set; }
    public T Data { get; set; }
}