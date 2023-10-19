using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

class Program
{
  static async Task Main(string[] args)
  {
    var botClient = new TelegramBotClient("6136093952:AAFY98biTwWb2Wa5S9iw3YEHoVPB8a9LwMA");

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
    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

    if (messageText.Length > 4 && messageText[..5] == "write")
    {
      using (FileStream fs = new("words.json", FileMode.OpenOrCreate))
      {
        Words words = new(messageText[6..]);
        await JsonSerializer.SerializeAsync(fs, words);
        Console.WriteLine("Data has been saved to file");
      }

      // Echo received message text
      Message sentMessage = await botClient.SendTextMessageAsync(
          chatId: chatId,
          text: "You write:\n" + messageText[6..],
          cancellationToken: cancellationToken);
    }

    if (messageText.Length == 4 && messageText[..4] == "read")
    {
      using (FileStream fs = new("words.json", FileMode.OpenOrCreate))
      {
        Words? words = await JsonSerializer.DeserializeAsync<Words>(fs);

        Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "Your text is:\n" + words?.Messages,
        cancellationToken: cancellationToken);
      }

    }

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

