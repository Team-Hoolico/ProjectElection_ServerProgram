using Npgsql;
using Microsoft.EntityFrameworkCore;

string login = "Host=localhost;Username=postgres;Password=BigGay420;Database=Vote";
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

Console.WriteLine("Please enter in the SQL file position: ");
try { 
    string CreateTable = File.ReadAllText(Console.ReadLine() ?? "C:\\", System.Text.Encoding.UTF8);
    await using (NpgsqlCommand command = new NpgsqlCommand(CreateTable, conn)){
        await command.ExecuteNonQueryAsync();
        Console.WriteLine($"[{DateTime.Now}] Created table if it didn't exist before");
    };
} catch(Exception e) {
    Console.WriteLine($"[{DateTime.Now}] Skipping creating Table\n{e}");
}


//Gotta Make the DB prior
//UID: BigInt, VotedTeamId: BigInt, VotedCaptainIds: BigInt[]

//Still has no use
app.MapGet("/Count/", async(int val) =>
{
    await using (NpgsqlCommand test = new NpgsqlCommand("SELECT count(UID) FROM Votes", conn))
    await using (var read = await test.ExecuteReaderAsync()){
        
        while(await read.ReadAsync()) { 
            return Results.Accepted("/Count/", read.GetInt64(0));
        }
    }
    return Results.BadRequest("/Count/");
});

// Returns 400 if fails, 200 if succeeds to register voter to database
app.MapPost("/RegisterVoter/", async (long UID) =>
{
    await using (NpgsqlCommand test = new NpgsqlCommand($"INSERT INTO Votes (UID) VALUES ({UID})", conn)){
        try {
            await test.ExecuteNonQueryAsync();
            return Results.Created("/RegisterVote/", "success"); //Response Code 201
        }catch(Exception e){
            return Results.BadRequest(e.Message); //Response Code 400
        }
    }
});

// Returns 422 if there is server error, 400 if UID is not in DB and 200 if UID is in DB
app.MapGet("/ValidateVoter/", async (long UID) =>
{
    await using (NpgsqlCommand test = new NpgsqlCommand($"SELECT count(UID) FROM Votes WHERE UID = {UID}", conn))
    await using (var read = await test.ExecuteReaderAsync()){
        try {
            while(await read.ReadAsync()) { 
                if (read.GetInt32(0)==1) {
                    return Results.Ok("success"); //Response Code 200
                }
            }
            return Results.BadRequest("failed"); //Response Code 400
        }
        catch(Exception e){
            return Results.UnprocessableEntity(e.Message); //Response Code 422
        }
    }
});

// FIX THIS
app.MapPost("/CastVote/", async (long UID,long VotedTeamId, long[] VotedCaptainIds) =>
{
    await using (NpgsqlCommand test = new NpgsqlCommand($"INSERT INTO votes (UID,VotedTeamId,VotedCaptainIds) VALUES ({UID},{VotedTeamId},{VotedCaptainIds})", conn)){
        int result = await test.ExecuteNonQueryAsync();
        return Results.Created("/AddVote/", result);
    }
});

// Run
app.Run();
