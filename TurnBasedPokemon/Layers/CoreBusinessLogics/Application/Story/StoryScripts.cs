using System.Collections.Generic;

public static class StoryScripts
{
    public static readonly List<StoryNode> Nodes = new List<StoryNode>
    {
        new StoryNode(
            id: "p1_start",
            phase: 1,
            lines: new List<string>
            {
                "Welcome to the world of Pokemon! Your journey as a Pokemon Trainer begins here.",
                "You live in the small town of Pallet, where you have always dreamed of becoming a great Pokemon Trainer.",
                "Today is the day you will receive your first Pokemon and start your adventure!"
            }
        ),
        
        new StoryNode(
            id: "p1_oak",
            phase: 1,
            lines: new List<string>
            {
                "Professor Oak is waiting outside.",
                "\"So, you're finally awake!\"",
                "\"Come, I have something to give you.\""
            }
        ),
        
        new StoryNode(
            id: "p1_brock_intro",
            phase: 1,
            lines: new List<string>
            {
                "You arrive at Pewter City.",
                "The Gym stands tall, made of solid stone.",
                "A man approaches you.",
                "\"I'm Brock! Let's see what you're made of!\""
            }
        ),

        new StoryNode(
            id: "p1_after_brock",
            phase: 1,
            lines: new List<string>
            {
                "Brock falls to one knee.",
                "\"I took you for granted. As proof of your victory, take this.\"",
                "You received the Boulder Badge."
            }
        ),

        // Phase 2 – Team Rocket xuất hiện
        new StoryNode(
            id: "p2_rocket_intro",
            phase: 2,
            lines: new List<string>
            {
                "Lavender Town feels eerily quiet.",
                "You sense something is wrong.",
                "Shadows move in the distance..."
            }
        ),
    };
}