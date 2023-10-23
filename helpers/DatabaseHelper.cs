using System.Text.Json;

class DatabaseHelper
{
  public async static Task<FinanceData> GetFinanceInformation()
  {
    var financeData = await ReadFile();
    foreach (var operation in financeData.CreditOperations)
    {
      financeData.CreditBalance += operation.Value;
    }
    foreach (var operation in financeData.DebitOperations)
    {
      financeData.DebitBalance += operation.Value;
    }
    return financeData;
  }
  public async static Task<bool> AddCreditOperation(int operation, string description = "")
  {
    var financeData = await ReadFile();
    string currentDateTime = TimeHelper.GetCurrentDateTime();
    financeData.CreditOperations.Add(new FinanceOperation(currentDateTime, operation, description));
    WriteFile(financeData);
    return true;

  }
  public async static Task<bool> AddDebitOperation(int operation, string description = "")
  {
    var financeData = await ReadFile();
    string currentDateTime = TimeHelper.GetCurrentDateTime();
    financeData.DebitOperations.Add(new FinanceOperation(currentDateTime, operation, description));
    WriteFile(financeData);
    return true;

  }
  public async static Task<bool> ChangeCreditBalance(int newBalance)
  {
    var financeData = await ReadFile();
    financeData.CreditBalance = newBalance;
    WriteFile(financeData);
    return true;

  }
  public async static Task<bool> ChangeDebitBalance(int newBalance)
  {
    var financeData = await ReadFile();
    financeData.DebitBalance = newBalance;
    WriteFile(financeData);
    return true;

  }
  private async static Task<FinanceData> ReadFile()
  {
    FileStream fs = new(Settings.DatabaseFileName, FileMode.Open);
    FinanceData? financeData = await JsonSerializer.DeserializeAsync<FinanceData>(fs);
    fs.Close();
    if (financeData is not null)
    {
      return financeData;
    }
    throw new Exception("File reading error");
  }
  private async static void WriteFile(FinanceData financeData)
  {
    FileStream fs = new(Settings.DatabaseFileName, FileMode.Create);
    await JsonSerializer.SerializeAsync(fs, financeData);
    fs.Close();
  }
}