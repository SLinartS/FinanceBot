class TimeHelper
{
  public static string GetCurrentDateTime()
  {
    return DateTime.Now.ToString().Substring(0, 16);
  }
}