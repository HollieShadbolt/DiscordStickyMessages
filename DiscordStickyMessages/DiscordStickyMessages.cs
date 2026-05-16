using Discord.Params;
using DiscordStickyMessages.Interfaces;
using IDiscord = Discord.Interfaces.IDiscord;

namespace DiscordStickyMessages;

/// <inheritdoc/>
/// <param name="discord">The <see cref="IDiscord"/>.</param>
/// <param name="config">The <see cref="IConfig"/>.</param>
public sealed class DiscordStickyMessages(IDiscord discord, IConfig config) : IDiscordStickyMessages
{
    /// <inheritdoc/>
    /// <exception cref="TaskCanceledException">The cancellation token was cancelled.</exception>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var currentUser = await discord.GetCurrentUserAsync(cancellationToken);

        var tasks = config.ChannelIdsToMessages.Select(keyValuePair =>
            StepAsync(keyValuePair, currentUser.Id, cancellationToken));

        await Task.WhenAll(tasks);
    }

    private async Task StepAsync(
        KeyValuePair<string, string> channelIdToMessage,
        string authorId,
        CancellationToken cancellationToken)
    {
        var getLatestMessageAsyncParams = new GetLatestMessageAsyncParams
        {
            GuildId = config.GuildId,
            ChannelId = channelIdToMessage.Key,
            AuthorId = authorId
        };

        var message = await discord.GetLatestMessageAsync(getLatestMessageAsyncParams, cancellationToken);

        var messageId = message?.Id;

        var createMessageAsyncParams = new CreateMessageAsyncParams
        {
            ChannelId = channelIdToMessage.Key,
            Content = channelIdToMessage.Value
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            messageId ??= (await discord.CreateMessageAsync(createMessageAsyncParams, cancellationToken)).Id;

            var getChannelMessagesAfterAsyncParams = new GetChannelMessagesAfterAsyncParams
            {
                ChannelId = channelIdToMessage.Key,
                MessageId = messageId
            };

            var messages =
                await discord.GetChannelMessagesAfterAsync(getChannelMessagesAfterAsyncParams, cancellationToken);

            if (!messages.Any())
            {
                continue;
            }

            var deleteMessageAsyncParams = new DeleteMessageAsyncParams
            {
                ChannelId = channelIdToMessage.Key,
                MessageId = messageId
            };

            await discord.DeleteMessageAsync(deleteMessageAsyncParams, cancellationToken);

            messageId = null;
        }

        throw new TaskCanceledException();
    }
}