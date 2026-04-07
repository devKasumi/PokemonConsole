using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using MySql.Data.MySqlClient;

public class MySqlUserRepository : IUserRepository
{
    private readonly string _connection;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IPokedexRepository _pokedex;

    public MySqlUserRepository(IPokedexRepository pokedex, string connection) 
    { 
        _connection = connection;
        _pokedex = pokedex;
        _jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() } 
        };
    }

    /// <summary>
    /// Retrieves all users from the database.
    /// </summary>
    public List<User> GetAll() 
    {
        var users = new List<User>();
        using var conn = new MySqlConnection(_connection); 
        conn.Open();
        
        // Step 1: Get all usernames
        using var cmd = new MySqlCommand("SELECT Username FROM users", conn);
        using var reader = cmd.ExecuteReader();
        var names = new List<string>();
        while (reader.Read()) names.Add(reader.GetString(0));
        reader.Close();
        
        // Step 2: Rehydrate full user objects
        foreach (var name in names) 
        { 
            var u = GetByUsername(name); 
            if (u != null) users.Add(u); 
        }
        return users;
    }

    /// <summary>
    /// Loads a specific user and all their relational data (Player, Inventory, Pokemon Team).
    /// </summary>
    public User? GetByUsername(string username) 
    {
        using var conn = new MySqlConnection(_connection); 
        conn.Open();
        
        // 1. Fetch User and Player data using a LEFT JOIN
        string sqlUser = @"
            SELECT u.Id, u.Username, u.Password, p.Name, p.CurrentPhase, p.CurrentLocation, p.Badges, p.InventoryData 
            FROM users u 
            LEFT JOIN players p ON u.Id = p.UserId 
            WHERE u.Username = @n";
            
        using var cmdUser = new MySqlCommand(sqlUser, conn); 
        cmdUser.Parameters.AddWithValue("@n", username);
        using var rUser = cmdUser.ExecuteReader();
        
        if (!rUser.Read()) return null; // User not found

        var user = new User { Username = rUser.GetString("Username"), Password = rUser.GetString("Password"), PlayerData = new Player() };
        int userId = rUser.GetInt32("Id");
        
        // Load PlayerData if it exists
        if (!rUser.IsDBNull(rUser.GetOrdinal("CurrentPhase"))) 
        {
            if (!rUser.IsDBNull(rUser.GetOrdinal("Name"))) user.PlayerData.Name = rUser.GetString("Name");
            user.PlayerData.CurrentPhase = rUser.GetInt32("CurrentPhase");
            user.PlayerData.CurrentLocation = rUser.GetString("CurrentLocation");
            
            user.PlayerData.Badges = JsonSerializer.Deserialize<List<string>>(rUser.GetString("Badges"), _jsonOptions) ?? new();
            user.PlayerData.Inventory = JsonSerializer.Deserialize<Dictionary<int, Dictionary<string, List<Item>>>>(rUser.GetString("InventoryData"), _jsonOptions) ?? new();
        }
        rUser.Close(); // Close reader before executing the next query

        // 2. Fetch the Pokemon Team
        string sqlPkm = "SELECT * FROM pokemons WHERE PlayerId = @id";
        using var cmdPkm = new MySqlCommand(sqlPkm, conn); 
        cmdPkm.Parameters.AddWithValue("@id", userId);
        using var rPkm = cmdPkm.ExecuteReader();
        
        while (rPkm.Read()) 
        {
            var species = _pokedex.GetSpeciesById(rPkm.GetInt32("SpeciesId"));
            if (species != null) 
            {
                var pkm = new Pokemon(species, rPkm.GetInt32("Level"), rPkm.GetInt32("CurrentHP"), rPkm.GetInt32("MaxHP")) 
                {
                    TotalExp = rPkm.GetInt32("TotalExp"), 
                    CurrentExp = rPkm.GetInt32("CurrentExp"), 
                    MaxExpForNextLevel = rPkm.GetInt32("MaxExpForNextLevel"),
                    IsFainted = rPkm.GetBoolean("IsFainted"), 
                    Status = (PokemonStatus)rPkm.GetInt32("Status"),
                    Moves = JsonSerializer.Deserialize<List<PokemonMove>>(rPkm.GetString("MovesData"), _jsonOptions) ?? new()
                };
                user.PlayerData.PokemonTeam.Add(pkm);
            }
        }

        if (user.PlayerData.PokemonTeam.Count > 0)
        {
            user.PlayerData.CurrentPokemon = user.PlayerData.PokemonTeam[0];
        }

        return user;
    }

    /// <summary>
    /// Saves the user's progress. Updates existing users or inserts new ones safely via Transactions.
    /// </summary>
    public void Save(User user) 
    {
        using var conn = new MySqlConnection(_connection); 
        conn.Open();
        using var trans = conn.BeginTransaction(); // Ensure ACID compliance
        
        try 
        {
            // 1. Save Base User Account
            string sqlInsertUser = @"
                INSERT INTO users (Username, Password) VALUES (@n, @p) 
                ON DUPLICATE KEY UPDATE Password=@p; 
                SELECT Id FROM users WHERE Username=@n";
            using var cmdUser = new MySqlCommand(sqlInsertUser, conn, trans);
            cmdUser.Parameters.AddWithValue("@n", user.Username); 
            cmdUser.Parameters.AddWithValue("@p", user.Password);
            int userId = Convert.ToInt32(cmdUser.ExecuteScalar());

            if (user.PlayerData != null) 
            {
                // 2. Save Player Progress
                string sqlInsertPlayer = @"
                    INSERT INTO players 
                    (UserId, Name, CurrentPhase, CurrentLocation, Badges, InventoryData, IsChampion, PvPWins, PvPLosses) 
                    VALUES 
                    (@id, @name, @ph, @loc, @b, @i, @isChamp, @wins, @losses) 
                    ON DUPLICATE KEY UPDATE 
                    Name=@name, CurrentPhase=@ph, CurrentLocation=@loc, Badges=@b, InventoryData=@i, 
                    IsChampion=@isChamp, PvPWins=@wins, PvPLosses=@losses";

                using var cmdPlayer = new MySqlCommand(sqlInsertPlayer, conn, trans);
                cmdPlayer.Parameters.AddWithValue("@id", userId); 
                cmdPlayer.Parameters.AddWithValue("@name", user.PlayerData.Name); 
                cmdPlayer.Parameters.AddWithValue("@ph", user.PlayerData.CurrentPhase); 
                cmdPlayer.Parameters.AddWithValue("@loc", user.PlayerData.CurrentLocation);
                cmdPlayer.Parameters.AddWithValue("@b", JsonSerializer.Serialize(user.PlayerData.Badges, _jsonOptions)); 
                cmdPlayer.Parameters.AddWithValue("@i", JsonSerializer.Serialize(user.PlayerData.Inventory, _jsonOptions));

                cmdPlayer.Parameters.AddWithValue("@isChamp", user.PlayerData.IsChampion ? 1 : 0);
                cmdPlayer.Parameters.AddWithValue("@wins", user.PlayerData.PvPWins);
                cmdPlayer.Parameters.AddWithValue("@losses", user.PlayerData.PvPLosses);

                cmdPlayer.ExecuteNonQuery();

                // 3. Refresh Pokemon Team (Clear old team, insert current team)
                new MySqlCommand($"DELETE FROM pokemons WHERE PlayerId={userId}", conn, trans).ExecuteNonQuery();
                
                foreach (var pkm in user.PlayerData.PokemonTeam) 
                {
                    string sqlInsertPkm = @"
                        INSERT INTO pokemons (PlayerId, SpeciesId, Level, TotalExp, CurrentExp, MaxExpForNextLevel, CurrentHP, MaxHP, IsFainted, Status, MovesData) 
                        VALUES (@pid, @sid, @lv, @tx, @cx, @mx, @chp, @mhp, @faint, @sts, @mvs)";
                    using var cmdPkm = new MySqlCommand(sqlInsertPkm, conn, trans);
                    cmdPkm.Parameters.AddWithValue("@pid", userId); 
                    cmdPkm.Parameters.AddWithValue("@sid", pkm.Specie.Id); 
                    cmdPkm.Parameters.AddWithValue("@lv", pkm.Level);
                    cmdPkm.Parameters.AddWithValue("@tx", pkm.TotalExp); 
                    cmdPkm.Parameters.AddWithValue("@cx", pkm.CurrentExp); 
                    cmdPkm.Parameters.AddWithValue("@mx", pkm.MaxExpForNextLevel);
                    cmdPkm.Parameters.AddWithValue("@chp", pkm.CurrentHP); 
                    cmdPkm.Parameters.AddWithValue("@mhp", pkm.MaxHP);
                    cmdPkm.Parameters.AddWithValue("@faint", pkm.IsFainted); 
                    cmdPkm.Parameters.AddWithValue("@sts", (int)pkm.Status);
                    cmdPkm.Parameters.AddWithValue("@mvs", JsonSerializer.Serialize(pkm.Moves, _jsonOptions));
                    cmdPkm.ExecuteNonQuery();
                }
            }
            trans.Commit();
        } 
        catch 
        { 
            trans.Rollback(); 
            throw; 
        }
    }
}