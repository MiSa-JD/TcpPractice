namespace Server;

public class Sample2
{
  public async Task ManageClients()
  {
    
  }
  public async Task StartListening()
  {
    
  }
  public static async Task Main(string[] args)
  {
    Sample2 sample = new Sample2();
    await sample.StartListening();
    Console.WriteLine("Done");
  }
}