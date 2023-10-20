using System.Text.Json;

enum EnvKeys
{
  Token,
}

class EnvHelper
{

  public static string GetValue(EnvKeys key)
  {
    using FileStream fs = new("settings.json", FileMode.Open);
    var envFile = JsonSerializer.Deserialize<Dictionary<EnvKeys, string>>(fs);
    if (envFile is not null)
    {
      return envFile[key];
    }
    throw new Exception("Read setting file failed");
  }
}