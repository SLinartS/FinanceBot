using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

class Program
{
  private static readonly TelegramBotClient botClient = new(EnvHelper.GetValue(EnvKeys.Token));

  static async Task Main(string[] args)
  {
    using CancellationTokenSource cts = new();

    ReceiverOptions receiverOptions = new()
    {
      AllowedUpdates = Array.Empty<UpdateType>()
    };

    botClient.StartReceiving(
        updateHandler: HandleUpdateAsync,
        pollingErrorHandler: HandlePollingErrorAsync,
        receiverOptions: receiverOptions,
        cancellationToken: cts.Token
    );

    var me = await botClient.GetMeAsync();

    Console.WriteLine($"Start listening for @{me.Username}");
    Console.ReadLine();

    cts.Cancel();
  }

  static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
  {
    if (update.Message is not { } message) { return; }
    if (message.Text is not { } messageText) { return; }

    long chatId = message.Chat.Id;

    if (message.From!.Id.ToString() != EnvHelper.GetValue(EnvKeys.UserID))
    {
      ReturnSimpleText("ОШИБКА! Неверный ID Пользователя", chatId, cancellationToken);
      return;
    }

    if (State.IsEnteringOperation)
    {
      string[] parsedMessage = StringAnalyserHelper.GetParsedString(messageText);

      string description = parsedMessage.Length > 1 ? parsedMessage[1] : "";

      if (!int.TryParse(parsedMessage[0], out int parsedNumber))
      {
        ReturnDefaultMenu(chatId, cancellationToken, "Введите число");
        return;
      }

      switch (State.OperationType)
      {
        case OperationType.AddCreditOperation:
          await DatabaseHelper.AddCreditOperation(parsedNumber, description);
          break;
        case OperationType.AddDebitOperation:
          await DatabaseHelper.AddDebitOperation(parsedNumber, description);
          break;
        case OperationType.ChangeCreditBalance:
          await DatabaseHelper.ChangeCreditBalance(parsedNumber);
          break;
        case OperationType.ChangeDebitBalance:
          await DatabaseHelper.ChangeDebitBalance(parsedNumber);
          break;
        case OperationType.ChangeInterestRate:
          await DatabaseHelper.ChangeInterestRate(parsedNumber);
          break;
        default:
          ReturnDefaultMenu(chatId, cancellationToken, "Неверный тип операции");
          break;
      }
    }

    State.IsEnteringOperation = true;
    string returnedText;
    switch (messageText)
    {
      case "Опер. по кредитке":
        State.OperationType = OperationType.AddCreditOperation;
        ReturnSimpleText("Введите операцию по кредитке:", chatId, cancellationToken);
        break;
      case "Опер. по счёту":
        State.OperationType = OperationType.AddDebitOperation;
        ReturnSimpleText("Введите операцию по счёту:", chatId, cancellationToken);
        break;
      case "Уст. нач. баланс кредитки":
        State.OperationType = OperationType.ChangeCreditBalance;
        ReturnSimpleText("Укажите начальный баланс кредитки:", chatId, cancellationToken);
        break;
      case "Уст. нач. баланс счёта":
        State.OperationType = OperationType.ChangeDebitBalance;
        ReturnSimpleText("Укажите начальный баланс счёта:", chatId, cancellationToken);
        break;
      case "Изменить процентную ставку":
        State.OperationType = OperationType.ChangeInterestRate;
        ReturnSimpleText("Новое значение процентной ставки:", chatId, cancellationToken);
        break;
      case "Последние операции по кредитке":
        State.IsEnteringOperation = false;
        returnedText = await CalculationHelper.GetLastCreditOperations();
        ReturnSimpleText(returnedText, chatId, cancellationToken);
        break;
      case "Последние операции по счёту":
        State.IsEnteringOperation = false;
        returnedText = await CalculationHelper.GetLastDebitOperations();
        ReturnSimpleText(returnedText, chatId, cancellationToken);
        break;
      case "Информация по счёту":
        State.IsEnteringOperation = false;
        returnedText = await CalculationHelper.GetFinanceInformation();
        ReturnSimpleText(returnedText, chatId, cancellationToken);
        break;
      default:
        State.IsEnteringOperation = false;
        ReturnDefaultMenu(chatId, cancellationToken);
        break;
    }
  }

  static async void ReturnDefaultMenu(long chatId, CancellationToken cancellationToken, string errorMessage = "")
  {
    State.IsEnteringOperation = false;
    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]{
          new KeyboardButton[] { "Информация по счёту" },
          new KeyboardButton[] { "Опер. по кредитке", "Опер. по счёту" },
          new KeyboardButton[] { "Уст. нач. баланс кредитки", "Уст. нач. баланс счёта" },
          new KeyboardButton[] { "Последние операции по кредитке", "Последние операции по счёту" },
          new KeyboardButton[] { "Изменить процентную ставку" },
        })
    {
      ResizeKeyboard = true
    };

    await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: errorMessage != "" ? $"{errorMessage}! Выберите действие" : $"Выберите действие",
        replyMarkup: replyKeyboardMarkup,
        cancellationToken: cancellationToken);
  }

  private static async void ReturnSimpleText(string message, long chatId, CancellationToken cancellationToken)
  {
    await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: message,
                cancellationToken: cancellationToken);
  }

  static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
  {
    var ErrorMessage = exception switch
    {
      ApiRequestException apiRequestException
          => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
      _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
  }
}

