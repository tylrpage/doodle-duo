public class WaitingState : IState
{
    public void Enter()
    {
        GameManager.Instance.GetService<UIManager>().SetStatusText("Waiting...");
    }
}