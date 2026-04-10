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
        var poke1 = state.Player1Active;
        var poke2 = state.Player2Active;

        // MoveIndex = -1 means the player skipped (timeout)
        bool p1Skipped = req1.MoveIndex < 0 || req1.MoveIndex >= poke1.Moves.Count;
        bool p2Skipped = req2.MoveIndex < 0 || req2.MoveIndex >= poke2.Moves.Count;

        PokemonMove? move1 = p1Skipped ? null : poke1.Moves[req1.MoveIndex];
        PokemonMove? move2 = p2Skipped ? null : poke2.Moves[req2.MoveIndex];

        // Determine move order by speed
        bool p1First = poke1.Specie.BaseStats.Speed >= poke2.Specie.BaseStats.Speed;

        // Execute moves in order
        var attackOrder = p1First
            ? new[] { (state.Player1Name, poke1, move1, state.Player2Name, poke2, p1Skipped),
                      (state.Player2Name, poke2, move2, state.Player1Name, poke1, p2Skipped) }
            : new[] { (state.Player2Name, poke2, move2, state.Player1Name, poke1, p2Skipped),
                      (state.Player1Name, poke1, move1, state.Player2Name, poke2, p1Skipped) };

        foreach (var (atkName, attacker, move, defName, defender, skipped) in attackOrder)
        {
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
            logs.Add($"{defender.Specie.Name} took {dmgResult.Damage} damage ({defender.CurrentHP}/{defender.MaxHP})");

            if (defender.IsFainted)
            {
                string winnerName = _battleStates[roomId].Player1Name;
                _battleStates[roomId].Player2ActiveIndex++;
                if (_battleStates[roomId].Player2ActiveIndex == _battleStates[roomId].Player2Party.Count - 1)
                {
                    _pendingMoves.TryRemove(roomId, out _);
                    state.IsGameOver = true;
                    state.WinnerName = winnerName;
                    return PvPTurnResult.GameOver(
                        winnerName, logs,
                        ToPvPMonState(poke1), ToPvPMonState(poke2)
                    );
                }
            }
            else if (attacker.IsFainted)
            {
                string winnerName = _battleStates[roomId].Player2Name;
                _battleStates[roomId].Player1ActiveIndex++;
                if (_battleStates[roomId].Player1ActiveIndex == _battleStates[roomId].Player1Party.Count - 1)
                {
                    _pendingMoves.TryRemove(roomId, out _);
                    state.IsGameOver = true;
                    state.WinnerName = winnerName;
                    return PvPTurnResult.GameOver(
                        winnerName, logs,
                        ToPvPMonState(poke1), ToPvPMonState(poke2)
                    );
                }
            }
        }
        _pendingMoves.TryRemove(roomId, out _);
        return PvPTurnResult.Continue(
            poke1.CurrentHP, poke2.CurrentHP, logs,
            ToPvPMonState(poke1), ToPvPMonState(poke2)
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
}