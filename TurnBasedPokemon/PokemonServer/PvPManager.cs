using System.Collections.Concurrent;
using System.Linq;
using PokemonEntity;

public class PvPManager
{
    private readonly ConcurrentDictionary<string, PvPBattleState> _battleStates = new();
    private readonly ConcurrentDictionary<string, List<BattleActionRequest>> _pendingMoves = new();

    /// <summary>
    /// Initialize a new PvP battle state for a room.
    /// </summary>
    public void InitializeBattle(string roomId, string player1, List<Pokemon> party1, string player2, List<Pokemon> party2)
    {
        var state = new PvPBattleState
        {
            RoomId = roomId,
            Player1Name = player1,
            Player2Name = player2,
            Player1Party = party1.Select(p => ClonePokemon(p)).ToList(),
            Player2Party = party2.Select(p => ClonePokemon(p)).ToList(),
            Player1ActiveIndex = 0,
            Player2ActiveIndex = 0
        };
        _battleStates[roomId] = state;
    }

    public void RegisterMove(BattleActionRequest request, string connectionId)
    {
        var moves = _pendingMoves.GetOrAdd(request.RoomId, _ => new List<BattleActionRequest>());
        lock (moves)
        {
            // Deduplicate by ConnectionId (always unique, unlike PlayerName)
            if (!moves.Any(m => m.ConnectionId == connectionId))
            {
                request.ConnectionId = connectionId;
                moves.Add(request);
                Console.WriteLine($"[PvP] Move registered for {request.PlayerName} (conn: {connectionId}). Total moves: {moves.Count}");
            }
            else
            {
                Console.WriteLine($"[PvP] Duplicate move rejected for conn: {connectionId}");
            }
        }
    }

    public bool IsRoomReady(string roomId)
    {
        // A room is ready when both players have submitted their moves
        return _pendingMoves.TryGetValue(roomId, out var moves) && moves.Count == 2;
    }

    public PvPTurnResult ProcessTurn(string roomId)
    {
        // If both players haven't submitted moves, wait
        if (!_pendingMoves.TryGetValue(roomId, out var moves) || moves.Count != 2)
        {
            return PvPTurnResult.Continue(
                0, 0, new List<string> { "Waiting for both players to submit moves." },
                new PvPMonState(), new PvPMonState()
            );
        }
        
        // If the battle state is missing, return error
        if (!_battleStates.TryGetValue(roomId, out var state))
        {
            return PvPTurnResult.Continue(
                0, 0, new List<string> { "Battle state not found." },
                new PvPMonState(), new PvPMonState()
            );
        }
        
        var logs = new List<string>();
        // Map requests to the correct player
        var req1 = moves.First(m => m.PlayerName == state.Player1Name);
        var req2 = moves.First(m => m.PlayerName == state.Player2Name);
        
        // Cache the initial pokemon for this turn's actions ONLY
        var turnPoke1 = state.Player1Active;
        var turnPoke2 = state.Player2Active;

        // MoveIndex = -1 means the player skipped (timeout)
        bool p1Skipped = req1.MoveIndex < 0 || req1.MoveIndex >= turnPoke1.Moves.Count;
        bool p2Skipped = req2.MoveIndex < 0 || req2.MoveIndex >= turnPoke2.Moves.Count;

        PokemonMove? move1 = p1Skipped ? null : turnPoke1.Moves[req1.MoveIndex];
        PokemonMove? move2 = p2Skipped ? null : turnPoke2.Moves[req2.MoveIndex];

        // Determine move order by speed
        bool p1First = turnPoke1.Specie.BaseStats.Speed >= turnPoke2.Specie.BaseStats.Speed;

        // Execute moves in order
        var attackOrder = p1First
            ? new[] { (state.Player1Name, turnPoke1, move1, state.Player2Name, turnPoke2, p1Skipped),
                      (state.Player2Name, turnPoke2, move2, state.Player1Name, turnPoke1, p2Skipped) }
            : new[] { (state.Player2Name, turnPoke2, move2, state.Player1Name, turnPoke1, p2Skipped),
                      (state.Player1Name, turnPoke1, move1, state.Player2Name, turnPoke2, p1Skipped) };

        foreach (var (atkName, attacker, move, defName, defender, skipped) in attackOrder)
        {
            // FIX: If the attacker died before their move (e.g., the faster Pokemon killed them), skip their attack!
            if (attacker.CurrentHP <= 0) continue;

            if (skipped)
            {
                logs.Add($"{atkName} did nothing this turn!");
                continue;
            }

            logs.Add($"{atkName} used {move!.Name}!");
            var dmgResult = DamageCalculator.Calculate(attacker, defender, move);
            defender.TakeDamage(dmgResult.Damage);
            
            if (dmgResult.IsCritical) logs.Add("A critical hit!");
            if (dmgResult.IsImmune) logs.Add("It doesn't affect the opponent...");
            else if (dmgResult.IsSuperEffective) logs.Add("It's super effective!");
            else if (dmgResult.IsNotVeryEffective) logs.Add("It's not very effective...");
            
            // Format HP cleanly so it doesn't show negative numbers
            logs.Add($"{defender.Specie.Name} took {dmgResult.Damage} damage ({Math.Max(0, defender.CurrentHP)}/{defender.MaxHP})");

            // FIX: Check who the fainting defender actually belongs to!
            if (defender.IsFainted)
            {
                logs.Add($"{defender.Specie.Name} fainted!");
                
                bool isPlayer1TheDefender = (defName == state.Player1Name);
                
                if (isPlayer1TheDefender)
                {
                    // Player 1's Pokémon fainted
                    if (state.Player1ActiveIndex >= state.Player1Party.Count - 1)
                    {
                        // Player 1 has no more Pokémon. Player 2 wins.
                        state.IsGameOver = true;
                        state.WinnerName = state.Player2Name; 
                        _pendingMoves.TryRemove(roomId, out _);
                        return PvPTurnResult.GameOver(state.WinnerName, logs, ToPvPMonState(state.Player1Active), ToPvPMonState(state.Player2Active));
                    }
                    else
                    {
                        // Swap to the next Pokemon
                        state.Player1ActiveIndex++;
                        logs.Add($"{state.Player1Name} sent out {state.Player1Active.Specie.Name}!");
                    }
                }
                else
                {
                    // Player 2's Pokémon fainted
                    if (state.Player2ActiveIndex >= state.Player2Party.Count - 1)
                    {
                        // Player 2 has no more Pokémon. Player 1 wins.
                        state.IsGameOver = true;
                        state.WinnerName = state.Player1Name;
                        _pendingMoves.TryRemove(roomId, out _);
                        return PvPTurnResult.GameOver(state.WinnerName, logs, ToPvPMonState(state.Player1Active), ToPvPMonState(state.Player2Active));
                    }
                    else
                    {
                        // Swap to the next Pokemon
                        state.Player2ActiveIndex++;
                        logs.Add($"{state.Player2Name} sent out {state.Player2Active.Specie.Name}!");
                    }
                }
                
                // FIX: Break the loop early! A newly sent out Pokemon shouldn't be attacked in the exact same turn.
                break; 
            }
        }

        _pendingMoves.TryRemove(roomId, out _);
        
        // FIX: Always pull directly from state.PlayerXActive here, NOT the cached 'turnPoke' variables.
        // If a swap occurred, state.PlayerXActive correctly points to the new Pokemon.
        return PvPTurnResult.Continue(
            state.Player1Active.CurrentHP, state.Player2Active.CurrentHP, logs,
            ToPvPMonState(state.Player1Active), ToPvPMonState(state.Player2Active)
        );
    }

    private static PvPMonState ToPvPMonState(Pokemon p)
    {
        return new PvPMonState
        {
            Name = p.Specie.Name,
            CurrentHP = p.CurrentHP,
            MaxHP = p.MaxHP,
            IsFainted = p.IsFainted,
            Moves = p.Moves.Select(m => new PvPMoveState
            {
                Name = m.Name,
                Type = m.Type.ToString(),
                Power = m.Power,
                PP = m.PP
            }).ToList()
        };
    }

    private static PokemonMove GetSelectedMove(Pokemon pokemon, int moveIndex)
    {
        if (pokemon?.Moves == null || pokemon.Moves.Count == 0)
            throw new InvalidOperationException($"No moves available.");
        if (moveIndex < 0 || moveIndex >= pokemon.Moves.Count)
            throw new InvalidOperationException($"Invalid move index {moveIndex}.");
        return pokemon.Moves[moveIndex];
    }




    public void SwapPokemon(string roomId, string playerName, int newIndex)
    {
        // Change the active Pokémon for the player
        if (_battleStates.TryGetValue(roomId, out var state))
        {
            if (state.Player1Name == playerName)
                state.Player1ActiveIndex = newIndex;
            else if (state.Player2Name == playerName)
                state.Player2ActiveIndex = newIndex;
        }
    }

    public PvPBattleState? GetBattleState(string roomId)
    {
        _battleStates.TryGetValue(roomId, out var state);
        return state;
    }

    /// <summary>
    /// Returns (playerName, connectionId) of the player who has NOT submitted a move yet, or null if both have.
    /// </summary>
    public (string playerName, string connectionId)? GetMissingPlayer(string roomId)
    {
        if (!_battleStates.TryGetValue(roomId, out var state)) return null;
        if (!_pendingMoves.TryGetValue(roomId, out var moves)) return (state.Player1Name, "timeout-p1");

        bool has1 = moves.Any(m => m.PlayerName == state.Player1Name);
        bool has2 = moves.Any(m => m.PlayerName == state.Player2Name);

        if (!has1) return (state.Player1Name, "timeout-p1");
        if (!has2) return (state.Player2Name, "timeout-p2");
        return null;
    }

    private static PokemonMove GetSelectedMove(BattleActionRequest request, Pokemon pokemon)
    {
        if (pokemon?.Moves == null || pokemon.Moves.Count == 0)
            throw new InvalidOperationException($"Player {request.PlayerName} does not have any moves available.");
        if (request.MoveIndex < 0 || request.MoveIndex >= pokemon.Moves.Count)
            throw new InvalidOperationException($"Invalid move index {request.MoveIndex} for player {request.PlayerName}.");
        return pokemon.Moves[request.MoveIndex];
    }

    private static Pokemon ClonePokemon(Pokemon p)
    {
        // Deep clone for safety (implement as needed)
        return new Pokemon
        {
            Specie = p.Specie, // If Specie is immutable, this is fine
            Level = p.Level,
            TotalExp = p.TotalExp,
            CurrentHP = p.CurrentHP,
            MaxHP = p.MaxHP,
            Moves = p.Moves.Select(m => new PokemonMove
            {
                Name = m.Name,
                Type = m.Type,
                Power = m.Power,
                PP = m.PP,
                MoveType = m.MoveType,
                LevelLearned = m.LevelLearned
            }).ToList(),
            IsFainted = p.IsFainted,
            Status = p.Status,
            CurrentExp = p.CurrentExp,
            MaxExpForNextLevel = p.MaxExpForNextLevel
        };
    }

    /// <summary>
    /// Forces the battle to end, granting victory to the player who did not disconnect.
    /// </summary>
    public PvPTurnResult? HandlePlayerDisconnect(string roomId, string disconnectedConnectionId, string p1ConnId, string p2ConnId)
    {
        if (!_battleStates.TryGetValue(roomId, out var state)) return null;

        state.IsGameOver = true;
        
        // Determine who the remaining player is
        string remainingPlayerName = (disconnectedConnectionId == p1ConnId) ? state.Player2Name : state.Player1Name;
        state.WinnerName = remainingPlayerName;

        var logs = new List<string> 
        { 
            "The opponent fled the battle! (Disconnected)", 
            "You win by default!" 
        };

        // Create the final result payload
        var result = PvPTurnResult.GameOver(
            remainingPlayerName, 
            logs,
            ToPvPMonState(state.Player1Active), 
            ToPvPMonState(state.Player2Active)
        );

        // Clean up pending moves and the battle state to prevent memory leaks
        _pendingMoves.TryRemove(roomId, out _);
        _battleStates.TryRemove(roomId, out _);

        return result;
    }
}