using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseInMemoryDatabase("Todos"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Json(new { message = "API minimal ASP.NET Core lista" }))
   .WithName("GetRoot")
   .WithOpenApi();

app.MapGet("/todos", async (TodoDbContext db) =>
        await db.Todos.ToListAsync())
   .WithName("GetTodos")
   .WithOpenApi();

app.MapPost("/todos", async (TodoDbContext db, TodoItem todo) =>
    {
        db.Todos.Add(todo);
        await db.SaveChangesAsync();
        return Results.Created($"/todos/{todo.Id}", todo);
    })
   .WithName("CreateTodo")
   .WithOpenApi();

app.MapPut("/todos/{id}", async (TodoDbContext db, int id, TodoItem input) =>
    {
        var todo = await db.Todos.FindAsync(id);
        if (todo is null)
        {
            return Results.NotFound();
        }

        todo.Title = input.Title;
        todo.IsComplete = input.IsComplete;
        await db.SaveChangesAsync();

        return Results.NoContent();
    })
   .WithName("UpdateTodo")
   .WithOpenApi();

app.MapDelete("/todos/{id}", async (TodoDbContext db, int id) =>
    {
        var todo = await db.Todos.FindAsync(id);
        if (todo is null)
        {
            return Results.NotFound();
        }

        db.Todos.Remove(todo);
        await db.SaveChangesAsync();

        return Results.NoContent();
    })
   .WithName("DeleteTodo")
   .WithOpenApi();

app.Run();

class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options)
        : base(options)
    {
    }

    public DbSet<TodoItem> Todos => Set<TodoItem>();
}

class TodoItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool IsComplete { get; set; }
}
