using Npgsql;
using Microsoft.EntityFrameworkCore;

string login = "Host=localhost;Username=postgres;Password=BigGay420;Database=Election";
await using var conn = new NpgsqlConnection(login);
await conn.OpenAsync();


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

//Gotta Make the DB prior
//UID: BigInt, VotedTeamID: BigInt, CaptainVote1: BigInt, CaptainVote2: BigInt 

app.MapGet("/Count/", async(int val) =>
{
    await using (NpgsqlCommand test = (new NpgsqlCommand("SELECT count(UID) FROM votes", conn)))
    await using (var read = await test.ExecuteReaderAsync()){
        
        while(await read.ReadAsync()) { 
            return Results.Accepted("/Count/", read.GetInt64(0));
        }
    }
    return Results.BadRequest("/Count/");
});

app.MapPost("/AddVote/", async (long UID,long Captain) =>
{
    await using (NpgsqlCommand test = (new NpgsqlCommand($"INSERT INTO votes (UID,ChosenCaptain) VALUES ({UID},{Captain})", conn)))
    {
        int result = await test.ExecuteNonQueryAsync();
        return Results.Created("/AddVote/",result)
    }
});
app.Run();
