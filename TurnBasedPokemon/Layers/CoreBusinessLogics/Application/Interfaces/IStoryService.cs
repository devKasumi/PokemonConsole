using System.Collections.Generic;

public interface IStoryService
{
    void InitializeStory(string command);
    LocationData GetCurrentLocation();
    bool ShouldPlayNarrative();
    void MarkNarrativeAsPlayed();

    string Move(bool forward);
    string HealTeamAtCenter();

    (bool Success, string Message, Pokemon? WildPokemon, Item? FoundItem) ExploreWildArea();
    (bool Success, string Message, NpcData? Leader, List<Pokemon>? Team) TryChallengeGym();
    (bool Success, string Message, List<Pokemon>? Team) TryChallengePokemonLeague(NpcData boss);
    (bool Success, string Message, List<Pokemon>? Team) TryChallengeTrainer(NpcData trainer);
    (bool Success, string Message) GiveStarter(string starterName);
}