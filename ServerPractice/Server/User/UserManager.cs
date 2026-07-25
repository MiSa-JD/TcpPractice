namespace Server.User;

public class UserManager
{
  private UserManager() { }
  public static UserManager _instance { get; } = new UserManager();

  public Guid SearchOrCreate(string username)
  {
    // 유저 이름 DB에서 조회
    // 있을 경우 해당 유저의 uuid 출력
    // 없을 경우
    return Guid.NewGuid();
  }
}