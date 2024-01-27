public class PlayingState : IState
{
    public void Enter()
    {
        GameManager.Instance.GetService<UIManager>().SetStatusText("Playing!");
    }
}