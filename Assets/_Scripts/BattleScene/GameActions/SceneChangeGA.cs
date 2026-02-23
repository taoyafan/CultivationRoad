public class SceneChangeGA : GameAction
{
    public string SceneName { get; }

    public SceneChangeGA(string sceneName)
    {
        SceneName = sceneName;
    }
}
