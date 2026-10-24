using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Belsoft.Api.Models;

namespace Belsoft.Api.Services;

public sealed class CreateOrderResult
{
    private CreateOrderResult(Order? order, IReadOnlyDictionary<string, string> errors)   // ①
    {
        Order = order;
        Errors = errors;
    }

    public Order? Order { get; }

    // Keys use the request's shape ("Items[0].ProductCode") so the controller can pass them to ModelState.
    public IReadOnlyDictionary<string, string> Errors { get; }                          // ②

    [MemberNotNullWhen(true, nameof(Order))]                                             // ③
    public bool Succeeded => Order is not null;

    public static CreateOrderResult Success(Order order) =>                              // ④
        new(order, ReadOnlyDictionary<string, string>.Empty);

    public static CreateOrderResult Failure(IReadOnlyDictionary<string, string> errors) =>
        new(null, errors);
}