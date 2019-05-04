using Godot;

public static class PackedSceneExtensions
{
    public static T Instance<T>(this PackedScene packedScene)
        where T : Node
    {
        return (T) packedScene.Instance();
    }
}