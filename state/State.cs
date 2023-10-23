enum OperationType
{
  AddCreditOperation,
  AddDebitOperation,
  ChangeCreditBalance,
  ChangeDebitBalance,
}

static class State
{
  public static bool IsEnteringOperation = false;
  public static OperationType OperationType;
}