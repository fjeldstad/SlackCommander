# SlackCommander

Useful (?) slash commands and miscellaneous integrations for [Slack](https://slack.com/).

Built with [Nancy](http://nancyfx.org/) and can easily be hosted as a free
[Azure Website](http://azure.microsoft.com/en-us/documentation/services/websites/).

[![Build status](https://ci.appveyor.com/api/projects/status/yki2s9y81vw2h7wn/branch/master)](https://ci.appveyor.com/project/Hihaj/slackcommander/branch/master)

## Installation

Setup instructions for the `/todo` feature can be found [here](https://github.com/Hihaj/SlackCommander/wiki/Setup-instructions).


## Todo list

A simple todo list per conversation (channel/direct message/private group).
Use the `/todo` command together with one of the supported operators to 
manage the list. Operators are:

- `[empty]` Displays the list to you.
- `show` Displays the list to everyone in the conversation.
- `add [text]` Adds an item to the list. The item gets a numeric ID automatically.
- `tick [id]` Marks an item as "done".
- `untick [id]` Marks an item as "not done".
- `remove [id]` Removes an item from the list.
- `trim` Removes all ticked (done) items.
- `clear` Removes all items.
- `help` Display usage information.

**Example:**

```
/todo add Update SlackCommander readme
/todo add Get coffee
/todo add Get to inbox zero
/todo
```

![SlackCommander /todo response](https://raw.githubusercontent.com/Hihaj/SlackCommander/master/todo-private.png)

```
/todo tick 1
/todo tick 3
/todo show
```

![SlackCommander /todo show response](https://raw.githubusercontent.com/Hihaj/SlackCommander/master/todo-public.png)

As stated above, each conversation gets its own list and everyone in the
conversation is free to manage it. Use the Slackbot channel if you want a
personal list, or create a private group with no other members (good for
todo-per-topic).


## Whois lookup for e-mail addresses or Twitter handles

```
/whois [e-mail address]
/whois [Twitter handle]
```

Performs a lookup and sends a brief summary of who the person behind the e-mail 
address or Twitter handle is to the current channel. Requires a 
[FullContact](http://www.fullcontact.com/developer/person-api/) API key
(the free tier currently includes 250 lookups per month).

**Example:**

```
/whois @SlackHQ
```

SlackCommander responds to the current channel with something similar to this:

![SlackCommander /whois example response](https://raw.githubusercontent.com/Hihaj/SlackCommander/master/whois-result.png)


## Misc. integrations

- **MailChimp webhook for new subscribers** - automatically posts a notification
  to a preconfigured Slack channel about the new list signup + initiates a whois 
  lookup (which is also posted to the channel when complete).

- **Send e-mail to Slack** - set up a [Mailgun](https://mailgun.com) route to 
  forward incoming e-mail to any Slack channel.

```
Mailgun route details
=====================

Filter expression: match_recipient("^(?P<slackChannel>[\w]+)\.(?P<webhookId>[\w]+)@yourdomain.net$")

Action: forward("https://yourslackcommander.com/webhooks/mailgun/\g<webhookId>/\g<slackChannel>")

(webhookId should be a hard-to-guess token to prevent spam etc.)
```
