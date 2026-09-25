using Player.Application.Abstractions;
using Player.Infrastructure.Persistence;
using MediatR;

namespace Player.Infrastructure.PipelineBehavior;

public sealed class CommandTransactionBehavior<TRequest, TResponse>(PlayerDbContext dbContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        TResponse response = await next(cancellationToken);

        if (request is ICommand)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
