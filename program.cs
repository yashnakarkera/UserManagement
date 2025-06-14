using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseHttpsRedirection();

// In-memory store for users
var users = new ConcurrentDictionary<string, User>();

// Create User (POST)
app.MapPost("/users", ([FromBody] User user) =>
{
    if (string.IsNullOrWhiteSpace(user.Name) || string.IsNullOrWhiteSpace(user.Email) || !user.Email.Contains("@"))
    {
        return Results.BadRequest("Invalid user data.");
    }

    user.Id = Guid.NewGuid().ToString();
    users[user.Id] = user;
    return Results.Created($"/users/{user.Id}", user);
});

// Get all users (GET)
app.MapGet("/users", () =>
{
    return Results.Ok(users.Values);
});

// Update user (PUT)
app.MapPut("/users/{id}", ([FromRoute] string id, [FromBody] User updatedUser) =>
{
    if (!users.ContainsKey(id))
        return Results.NotFound("User not found.");

    if (string.IsNullOrWhiteSpace(updatedUser.Name) || string.IsNullOrWhiteSpace(updatedUser.Email) || !updatedUser.Email.Contains("@"))
        return Results.BadRequest("Invalid user data.");

    updatedUser.Id = id;
    users[id] = updatedUser;
    return Results.Ok(updatedUser);
});

// Delete user (DELETE)
app.MapDelete("/users/{id}", ([FromRoute] string id) =>
{
    if (!users.TryRemove(id, out var removedUser))
        return Results.NotFound("User not found.");

    return Results.Ok($"User {id} deleted.");
});

app.Run();

record User
{
    public string? Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
}
