using ClaimFlow.Application.Features.Customers.Commands.CreateCustomer;
using ClaimFlow.Application.Features.Customers.Queries.GetCustomerById;
using ClaimFlow.Application.Features.Customers.Queries.GetCustomers;
using MediatR;

namespace ClaimFlow.Api.Extensions
{
    public static class CustomerEndpoint
    {

        public static void MapCustomerEndpoint(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/customers");


            group.MapPost("/api/customers", async (CreateCustomerCommand command, IMediator mediator) =>
            {

                var id = await mediator.Send(command);

                return Results.Created($"/api/customers/{id}", new { id });

            });

            group.MapGet("/api/customers", async (IMediator mediator) =>
            {
                var customers =  await mediator.Send(new GetCustomersQuery());

                return Results.Ok(customers);

            });

            group.MapGet("/api/customers/{id}", async (Guid id, IMediator mediator) =>
            {
                var customer = await mediator.Send(new GetCustomerByIdQuery(id));


                return customer is null ? Results.NotFound() : Results.Ok(customer);

            });
        }
    }
}
