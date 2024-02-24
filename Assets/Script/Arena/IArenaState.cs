

public interface IArenaState
{
    public void Init(ArenaManager arenaManager);
    public void OnEnterState();
    public void OnUpdateState();
    public void OnExitState();
}
