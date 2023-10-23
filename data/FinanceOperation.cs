class FinanceOperation
{
  public string Date { get; set; }
  public int Value { get; set; }
  public string Description { get; set; }

  public FinanceOperation(string date, int value, string description)
  {
    this.Date = date;
    this.Value = value;
    this.Description = description;
  }
}