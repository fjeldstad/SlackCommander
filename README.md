# SlackCommander

Useful (?) slash commands and miscellaneous integrations for [Slack](https://slack.com/).

Built with [Nancy](http://nancyfx.org/) and can easily be hosted as a free
[Azure Website](http://azure.microsoft.com/en-us/documentation/services/websites/).

[![Build status](https://ci.appveyor.com/api/projects/status/yki2s9y81vw2h7wn/branch/master)](https://ci.appveyor.com/project/Hihaj/slackcommander/branch/master)


## Slash commands 

`/whois [e-mail or Twitter handle]`

Performs a lookup and sends a brief summary of who the person behind the e-mail 
address or Twitter handle is to the current channel. Requires a 
[FullContact](http://www.fullcontact.com/developer/person-api/) API key
(the free tier currently includes 250 lookups per month).

**Example:**

    /whois @SlackHQ

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
