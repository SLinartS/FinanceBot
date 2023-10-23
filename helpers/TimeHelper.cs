class TimeHelper
{
  public static string GetCurrentDateTime()
  {
    return DateTime.Now.ToString("dd.MM.yyyy hh:mm").Substring(0, 16);
  }
}