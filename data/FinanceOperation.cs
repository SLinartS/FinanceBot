class FinanceOperation
{
  public string Date { get; set; }
  public int Value { get; set; }

  public FinanceOperation(string date, int value)
  {
    this.Date = date;
    this.Value = value;
  }
}