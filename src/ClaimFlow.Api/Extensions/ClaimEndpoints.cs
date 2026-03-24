using ClaimFlow.Application.Features.Claims.Queries.GetClaimByIdQuery;
using ClaimFlow.Application.Features.Claims.Commands;
using ClaimFlow.Application.Features.Claims.TransitionClaimCmd;
using MediatR;

namespace ClaimFlow.Api.Extensions
{
    public static class ClaimEndpoints
    {
        public static void MapClaimEndpoint(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/claims");

            group.MapPost("/", async (SubmitClaimCommand command, IMediator mediator) =>
            {
                var id = await mediator.Send(command);
                return Results.Created($"/api/claims/{id}", new { id });
            });

            group.MapPatch("/{id:guid}/transition", async (Guid id, TransitionClaimRequest request, IMediator mediator) =>
            {
                var command = new TransitionClaimCommand(id, request.Trigger, request.ChangedBy, request.Notes);
                await mediator.Send(command);
                return Results.NoContent();
            });

            group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var claim = await mediator.Send(new GetClaimByIdQuery(id));
                return claim is null ? Results.NotFound() : Results.Ok(claim);
            });
        }
    }

    public record TransitionClaimRequest(Domain.Enums.ClaimTrigger Trigger, string ChangedBy, string? Notes);
}
