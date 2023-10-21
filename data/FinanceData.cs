using System.Text.Json.Serialization;

class FinanceData
{
  public int CreditLimit { get; set; }
  public int CreditBalance { get; set; }
  public int DebitBalance { get; set; }
  public required List<FinanceOperation> CreditOperations { get; set; }
  public required List<FinanceOperation> DebitOperations { get; set; }
}