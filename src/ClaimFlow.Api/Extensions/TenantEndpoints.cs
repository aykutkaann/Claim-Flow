using ClaimFlow.Application.Features.Tenants.Commands.CreateTenant;
using ClaimFlow.Application.Features.Tenants.Queries.GetTenantById;
using ClaimFlow.Application.Features.Tenants.Queries.GetTenants;
using MediatR;

namespace ClaimFlow.Api.Extensions
{
    public static class TenantEndpoints
    {

        public static void MapTenantEndpoint(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/tenants");


            group.MapPost("/", async (CreateTenantCommand command, IMediator mediator) =>
            {

                var id = await mediator.Send(command);
                return Results.Created($"/api/tenants/{id}", new { id });

            });

            group.MapGet("/", async (IMediator mediator) =>
            {
                var tenants = await mediator.Send(new GetTenantsQuery());

                return Results.Ok(tenants);

            });

            group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var tenant = await mediator.Send(new GetTenantByIdQuery(id));

                return tenant is null ? Results.NotFound() : Results.Ok(tenant); 

            });

        }
    }
}
