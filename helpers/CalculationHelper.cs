class CalculationHelper
{
  public async static Task<string> GetFinanceInformation()
  {
    var financeData = await DatabaseHelper.GetFinanceInformation();
    int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
    int currentDay = DateTime.Now.Day;
    int daysBeforePayday = daysInMonth - currentDay + 10;
    int remainingMoney = financeData.DebitBalance - (financeData.CreditLimit - financeData.CreditBalance);
    int moneyForDay = remainingMoney / daysBeforePayday;
    int impactOfHundreds = moneyForDay - ((remainingMoney - 100) / daysBeforePayday);
    string returnedText = string.Concat
    (
      $"Баланс кредитки: {financeData.CreditBalance} ₽ \n",
      $"Баланс счёта: {financeData.DebitBalance} ₽ \n",
      $"ИТОГ: {remainingMoney} ₽\n",
      new string('-', 25) + "\n",
      $"Денег на день: {moneyForDay} ₽\n",
      $"Влияние 100₽: {impactOfHundreds} ₽\n",
      $"Дней до зарплаты: {daysBeforePayday}\n"
    );
    return returnedText;
  }
  public async static Task<string> GetLastCreditOperations()
  {
    var financeData = await DatabaseHelper.GetFinanceInformation();
    string returnedText = "";

    foreach (FinanceOperation operation in financeData.CreditOperations)
    {
      returnedText += $"{operation.Date} | {operation.Value} ₽ | {operation.Description} \n";
    }

    return returnedText;
  }
  public async static Task<string> GetLastDebitOperations()
  {
    var financeData = await DatabaseHelper.GetFinanceInformation();
    string returnedText = "";

    foreach (FinanceOperation operation in financeData.DebitOperations)
    {
      returnedText += $"{operation.Date} | {operation.Value} ₽ | {operation.Description} \n";
    }

    return returnedText;
  }
}