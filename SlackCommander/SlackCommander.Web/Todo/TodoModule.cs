using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using MassTransit;
using Nancy;
using Nancy.ModelBinding;
using NLog;
using SlackCommander.Web.SlackMessage.Messages;

namespace SlackCommander.Web.Todo
{
    public class TodoModule : NancyModule
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly ITodoService _todoService;
        private readonly IServiceBus _bus;

        public TodoModule(IAppSettings appSettings, ITodoService todoService, IServiceBus bus)
        {
            _todoService = todoService;
            _bus = bus;

            Post["/todo"] = _ =>
            {
                var slashCommand = this.Bind<SlashCommand>();
                if (slashCommand == null ||
                    slashCommand.command.Missing())
                {
                    Log.Info("Rejected an incoming slash command (unable to parse request body).");
                    return HttpStatusCode.BadRequest.WithReason("Unable to parse slash command.");
                }
                if (!appSettings.Get("todo:slackSlashCommandToken").Equals(slashCommand.token))
                {
                    Log.Info("Blocked an unauthorized slash command.");
                    return HttpStatusCode.Unauthorized.WithReason("Missing or invalid token.");
                }
                if (!slashCommand.command.Equals("/todo", StringComparison.InvariantCultureIgnoreCase))
                {
                    Log.Info("Rejected an incoming slash command ({0} is not handled by this module).", slashCommand.command);
                    return HttpStatusCode.BadRequest.WithReason("Unsupported slash command.");
                }

                var responseText = HandleTodo(slashCommand);
                if (responseText.Missing())
                {
                    return HttpStatusCode.OK;
                }
                return responseText;
            };
        }

        private string HandleTodo(SlashCommand message)
        {
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
                            text = list.ToSlackString()
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
                    }
                default:
                    {
                        return "Sorry, that is not a valid syntax for the `/todo` command. Use `/todo help` to see available operations.";
                    }
            }
            list = _todoService.GetItems(listId).ToArray();
            return list.ToSlackString();
        }
    }
}