using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using MassTransit;
using NLog;
using SlackCommander.Web.Messages;
using SlackCommander.Web.Todo;

namespace SlackCommander.Web.CommandHandlers
{
    public class SlashCommandHandler : Consumes<SlashCommand>.All
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly IAppSettings _appSettings;
        private readonly IServiceBus _bus;
        private readonly ITodoService _todoService;

        public SlashCommandHandler(IAppSettings appSettings, IServiceBus bus, ITodoService todoService)
        {
            _appSettings = appSettings;
            _bus = bus;
            _todoService = todoService;
        }

        public void Consume(SlashCommand message)
        {
            string responseText = null;

            switch (message.command.ToLowerInvariant())
            {
                case "/whois":
                    responseText = HandleWhois(message);
                    break;
                case "/todo":
                    responseText = HandleTodo(message);
                    break;
                default:
                    responseText = string.Format("Sorry, *{0}* is not a supported slash command.", message.command);
                    break;
            }

            _bus.Context().Respond(new SlashCommandResponse
            {
                Text = responseText
            });
        }

        private string HandleWhois(SlashCommand message)
        {
            if (!_appSettings.Get("slack:whoisSlashCommandToken").Equals(message.token))
            {
                Log.Info("Blocked an unauthorized /whois slash command.");
                return null;
            }

            if (message.text.IsValidEmail())
            {
                _bus.Publish(new WhoisEmailRequest
                {
                    CorrelationId = Guid.NewGuid(),
                    EmailAddress = message.text,
                    RequestedByUser = message.user_name,
                    RespondToChannel =
                        message.channel_name == "directmessage" ?
                        "@" + message.user_name :
                        "#" + message.channel_name
                });
                return string.Format("Looking up e-mail address *{0}*, one moment please...", message.text);
            }
            
            if (message.text.CouldBeTwitterHandle())
            {
                _bus.Publish(new WhoisTwitterRequest
                {
                    CorrelationId = Guid.NewGuid(),
                    TwitterHandle = message.text,
                    RequestedByUser = message.user_name,
                    RespondToChannel =
                        message.channel_name == "directmessage"
                            ? "@" + message.user_name
                            : "#" + message.channel_name
                });
                return string.Format("Looking up Twitter handle *{0}*, one moment please...", message.text);
            }

            return "Sorry, I'm only able to work with e-mail addresses and Twitter handles.";
        }

        private string HandleTodo(SlashCommand message)
        {
            if (!_appSettings.Get("slack:todoSlashCommandToken").Equals(message.token))
            {
                Log.Info("Blocked an unauthorized /todo slash command.");
                return null;
            }

            var listId = message.channel_id;
            var list = _todoService.GetItems(listId).ToArray();
            var @operator = message.text.SubstringByWords(0, 1);
            if (!@operator.Missing())
            {
                @operator = @operator.ToLowerInvariant();
            }
            switch (@operator)
            {
                case "":
                {
                    // Just echo the list
                    break;
                }
                case "show":
                {
                    _bus.Publish(new MessageToSlack
                    {
                        channel = listId,
                        text = ToSlackString(list)
                    });
                    return null;
                }
                case "add":
                {
                    var todoText = message.text.SubstringByWords(1);
                    if (todoText.Missing())
                    {
                        return null;
                    }
                    _todoService.AddItem(listId, todoText);
                    break;
                }
                case "tick":
                {
                    var todoItemId = message.text.SubstringByWords(1, 1);
                    if (todoItemId.Missing())
                    {
                        return null;
                    }
                    _todoService.TickItem(listId, todoItemId);
                    break;
                }
                case "untick":
                {
                    var todoItemId = message.text.SubstringByWords(1, 1);
                    if (todoItemId.Missing())
                    {
                        return null;
                    }
                    _todoService.UntickItem(listId, todoItemId);
                    break;
                }
                case "remove":
                {
                    var todoItemId = message.text.SubstringByWords(1, 1);
                    if (todoItemId.Missing())
                    {
                        return null;
                    }
                    _todoService.RemoveItem(listId, todoItemId);
                    break;
                }
                case "trim":
                {
                    _todoService.ClearItems(listId, includeUnticked: false);
                    break;
                }
                case "clear":
                {
                    _todoService.ClearItems(listId, includeUnticked: true);
                    break;
                }
                case "help":
                {
                    return "TODO"; // TODO Return usage info
                    break;
                }
                default:
                {
                    return "Sorry, that is not a valid syntax for the `/todo` command. Use `/todo help` to see available operations.";
                }
            }
            list = _todoService.GetItems(listId).ToArray();
            return ToSlackString(list);
        }

        private static string ToSlackString(IEnumerable<TodoItem> todoItems)
        {
            var items = todoItems.ToArray();
            if (!items.Any())
            {
                return "You are all done!";
            }
            var text = new StringBuilder();
            foreach (var item in todoItems)
            {
                text.AppendLine(ToSlackString(item));
            }
            return text.ToString().Trim();
        }

        private static string ToSlackString(TodoItem todoItem)
        {
            return string.Format(
                "`{0}` {1} {2}",
                todoItem.Id,
                todoItem.Done ? ":white_check_mark:" : ":white_square:",
                todoItem.Text);
        }
    }
}