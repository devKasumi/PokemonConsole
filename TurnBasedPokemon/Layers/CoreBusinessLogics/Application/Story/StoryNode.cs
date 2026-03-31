public class StoryNode
{
    public string Id { get; }
    public int Phase { get; }
    public List<string> Lines { get; }

    public StoryNode(string id, int phase, List<string> lines)
    {
        Id = id;
        Phase = phase;
        Lines = lines;
    }
    
}