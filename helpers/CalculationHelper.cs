class CalculationHelper
{
  public async static Task<string> GetFinanceInformation()
  {
    var financeData = await DatabaseHelper.GetFinanceInformation();
    int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
    int currentDay = DateTime.Now.Day;
    int daysBeforePayday = daysInMonth - currentDay + 10;
    if (daysBeforePayday < 10)
    {
      daysBeforePayday = 10 - currentDay;
    }

    int remainingMoney = financeData.DebitBalance - (financeData.CreditLimit - financeData.CreditBalance);
    int moneyForDay = remainingMoney / daysBeforePayday;
    int impactOfHundreds = moneyForDay - ((remainingMoney - 100) / daysBeforePayday);
    return string.Concat
    (
      $"Баланс кредитки: {financeData.CreditBalance} ₽ \n",
      $"Баланс счёта: {financeData.DebitBalance} ₽ \n",
      $"ИТОГ: {remainingMoney} ₽\n",
      new string('-', 25) + "\n",
      $"Денег на день: {moneyForDay} ₽\n",
      $"Влияние 100₽: {impactOfHundreds} ₽\n",
      $"Дней до зарплаты: {daysBeforePayday}\n",
      new string('-', 25) + "\n",
      $"Ежедневные проценты: {financeData.DebitBalance * financeData.InterestRate / 36000} ₽\n",
      $"Ежемесячные проценты: {financeData.DebitBalance * financeData.InterestRate / 1200} ₽\n",
      $"Годовой процент: {financeData.DebitBalance * financeData.InterestRate / 100} ₽\n",
      new string('-', 25) + "\n",
      $"Текущая ставка по счёту: {financeData.InterestRate} %\n"
    );
  }
  public async static Task<string> GetLastCreditOperations()
  {
    var financeData = await DatabaseHelper.GetFinanceInformation();
    return GetOperations(financeData.CreditOperations);
  }
  public async static Task<string> GetLastDebitOperations()
  {
    var financeData = await DatabaseHelper.GetFinanceInformation();
    return GetOperations(financeData.DebitOperations);
  }

  private static string GetOperations(List<FinanceOperation> operations)
  {
    string returnedText = "";

    foreach (FinanceOperation operation in operations.GetRange(operations.Count - 10, 10))
    {
      returnedText += $"{operation.Date} | {operation.Value} ₽ | {operation.Description} \n";
    }

    return returnedText;
  }
}