using ClaimFlow.Application.Features.Policies.Commands.CreatePolicy;
using ClaimFlow.Application.Features.Policies.Queries.GetPolicies;
using ClaimFlow.Application.Features.Policies.Queries.GetPoliciesById;
using ClaimFlow.Domain.Entities;
using MediatR;

namespace ClaimFlow.Api.Extensions
{
    public static class PolicyEndpoint
    {
       public static void MapPolicyEndpoint(this IEndpointRouteBuilder routes)
        {

            var group = routes.MapGroup("/api/policies");

            group.MapPost("/", async (CreatePolicyCommand command, IMediator mediator) =>
            {

                var id = await mediator.Send(command);

                return Results.Created($"/api/policies/{id}", new { id });

            });

            group.MapGet("/", async (IMediator mediator) =>
            {
                var policies = await mediator.Send(new GetPoliciesQuery());

                return Results.Ok(policies);

            });

            group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var policies = await mediator.Send(new GetPoliciesByIdQuery(id));

                return Results.Ok(policies);
            });

        }
    }
}
