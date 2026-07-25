namespace Protocol.Token;

public class TokenManager: IAsyncDisposable
{
  private TokenManager() { }
  public static TokenManager _instance { get; } = new();
  
  private readonly CancellationTokenSource cts = new();
  public CancellationToken token => cts.Token;

  public void Cancel() { cts.Cancel(); }

  public async ValueTask DisposeAsync()
  {
    await cts.CancelAsync();
    cts.Dispose();
  }
}