class StringAnalyserHelper
{
  public static string[] GetParsedString(string message)
  {
    string[] splittedMessage = message.Split("-m");
    string[] trimmedMessages = new string[splittedMessage.Length];
    for (int index = 0; index < splittedMessage.Length; index++)
    {
      trimmedMessages[index] = splittedMessage[index].Trim();
    }

    return trimmedMessages;
  }
}