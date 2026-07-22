namespace Server;

public class TokenManager
{
  private TokenManager() { }
  private static TokenManager? _manager;
  private static CancellationTokenSource cts => new();
  private static CancellationToken ct;

  public static TokenManager GetManager()
  {
    if (_manager is null)
    {
      _manager = new TokenManager();
      ct = cts.Token;
    }

    return _manager;
  }

  public CancellationToken GetCt() { return ct; }

  public async Task stop()
  {
    await cts.CancelAsync();
    cts.Dispose();
  }
}