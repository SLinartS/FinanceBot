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
      AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
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
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
    {
      return;
    }

    // Only process text messages
    if (message.Text is not { } messageText)
    {
      return;
    }

    var chatId = message.Chat.Id;

    if (State.IsEnteringOperation)
    {
      if (int.TryParse(messageText, out int parsedNumber))
      {
        switch (State.OperationType)
        {
          case OperationType.AddCreditOperation:
            await DatabaseHelper.AddCreditOperation(parsedNumber);
            break;
          case OperationType.AddDebitOperation:
            await DatabaseHelper.AddDebitOperation(parsedNumber);
            break;
          case OperationType.ChangeCreditBalance:
            await DatabaseHelper.ChangeCreditBalance(parsedNumber);
            break;
          case OperationType.ChangeDebitBalance:
            await DatabaseHelper.ChangeDebitBalance(parsedNumber);
            break;

          default:
            ReturnDefaultMenu(chatId, cancellationToken);
            break;
        }
        State.IsEnteringOperation = false;
      }
    }

    State.IsEnteringOperation = true;
    switch (messageText)
    {
      case "Добавить операцию по кредитке":
        State.OperationType = OperationType.AddCreditOperation;
        ReturnSimpleText("Введите операцию по кредитке:", chatId, cancellationToken);
        break;
      case "Добавить операцию по дебетовой карте":
        State.OperationType = OperationType.AddDebitOperation;
        ReturnSimpleText("Введите операцию по дебетовой карте:", chatId, cancellationToken);
        break;
      case "Установить начальный баланс кредитки":
        State.OperationType = OperationType.ChangeCreditBalance;
        ReturnSimpleText("Укажите начальный баланс кредитки:", chatId, cancellationToken);
        break;
      case "Установить начальный баланс дебетовой карты":
        State.OperationType = OperationType.ChangeDebitBalance;
        ReturnSimpleText("Укажите начальный баланс дебетовой карты:", chatId, cancellationToken);
        break;
      default:
        State.IsEnteringOperation = false;
        ReturnDefaultMenu(chatId, cancellationToken);
        break;
    }
    var financeData = await DatabaseHelper.GetFinanceInformation();
    string returnedText = string.Concat
    (
    $"Текущий баланс кредитки: {financeData.CreditBalance} \n",
    $"Текущий баланс дебетовой карты: {financeData.DebitBalance}"
    );
    ReturnSimpleText(returnedText, chatId, cancellationToken);
  }

  static async void ReturnDefaultMenu(long chatId, CancellationToken cancellationToken)
  {
    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]{
          new KeyboardButton[] { "Добавить операцию по кредитке", "Добавить операцию по дебетовой карте" },
          new KeyboardButton[] { "Установить начальный баланс кредитки", "Установить начальный баланс дебетовой карты" },
        })
    {
      ResizeKeyboard = true
    };

    // Echo received message text
    await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "Выберите действие",
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

