using Moq;

namespace DiscordStickyMessagesTests;

[TestFixture]
public static class DiscordStickyMessagesTests
{
    [Test]
    public static void RunAsync_Test()
    {
        // Arrange
        var mockDiscord = new Mock<Discord.Interfaces.IDiscord>();

        var guildId = Guid.NewGuid().ToString();

        var channelId = Guid.NewGuid().ToString();

        var currentUserId = Guid.NewGuid().ToString();

        var messageId = Guid.NewGuid().ToString();

        var getLatestMessageMessage = new Discord.Responses.Message
        {
            Id = messageId
        };

        var getCurrentUserUser = new Discord.Responses.User
        {
            Id = currentUserId
        };

        var cancellationTokenSource = new CancellationTokenSource();

        mockDiscord.Setup(discord => discord.GetCurrentUserAsync(cancellationTokenSource.Token))
            .ReturnsAsync(getCurrentUserUser);

        var getLatestMessageAsyncParams = new Discord.Params.GetLatestMessageAsyncParams
        {
            GuildId = guildId,
            ChannelId = channelId,
            AuthorId = currentUserId
        };

        mockDiscord
            .Setup(discord => discord.GetLatestMessageAsync(getLatestMessageAsyncParams, cancellationTokenSource.Token))
            .ReturnsAsync(getLatestMessageMessage);

        var getChannelMessagesAfterMessageId = Guid.NewGuid().ToString();

        Discord.Responses.Message[] getChannelMessagesAfterMessages =
        [
            new()
            {
                Id = getChannelMessagesAfterMessageId
            }
        ];

        var getChannelMessagesAfterAsyncParamsChannelId = new Discord.Params.GetChannelMessagesAfterAsyncParams
        {
            ChannelId = channelId,
            MessageId = messageId
        };

        var getChannelMessagesAfterCount = 0;

        mockDiscord
            .Setup(discord =>
                discord.GetChannelMessagesAfterAsync(getChannelMessagesAfterAsyncParamsChannelId,
                    cancellationTokenSource.Token))
            .ReturnsAsync(() => getChannelMessagesAfterCount++ < 3 ? [] : getChannelMessagesAfterMessages);

        var message = Guid.NewGuid().ToString();

        var createMessageId = Guid.NewGuid().ToString();

        var createMessageMessage = new Discord.Responses.Message
        {
            Id = createMessageId
        };

        var createMessageAsyncParams = new Discord.Params.CreateMessageAsyncParams
        {
            ChannelId = channelId,
            Content = message
        };

        mockDiscord
            .Setup(discord => discord.CreateMessageAsync(createMessageAsyncParams, cancellationTokenSource.Token))
            .ReturnsAsync(createMessageMessage);

        var getChannelMessagesAfterAsyncParamsCreateMessageId = new Discord.Params.GetChannelMessagesAfterAsyncParams
        {
            ChannelId = channelId,
            MessageId = createMessageId
        };

        mockDiscord
            .Setup(discord => discord.GetChannelMessagesAfterAsync(getChannelMessagesAfterAsyncParamsCreateMessageId,
                cancellationTokenSource.Token))
            .ThrowsAsync(new TaskCanceledException());

        var channelIdsToMessage = new Dictionary<string, string>
        {
            {
                channelId,
                message
            }
        };

        var mockConfig = new Mock<DiscordStickyMessages.Interfaces.IConfig>();

        mockConfig.SetupGet(config => config.ChannelIdsToMessages).Returns(channelIdsToMessage);
        mockConfig.SetupGet(config => config.GuildId).Returns(guildId);

        var discordStickyMessages =
            new DiscordStickyMessages.DiscordStickyMessages(mockDiscord.Object, mockConfig.Object);

        // Act
        Assert.ThrowsAsync<TaskCanceledException>(() => discordStickyMessages.RunAsync(cancellationTokenSource.Token));

        // Assert
        mockDiscord.Verify(discord => discord.GetCurrentUserAsync(cancellationTokenSource.Token), Times.Exactly(1));

        mockDiscord.Verify(
            discord => discord.GetLatestMessageAsync(getLatestMessageAsyncParams, cancellationTokenSource.Token),
            Times.Exactly(1));

        mockDiscord.Verify(
            discord => discord.GetChannelMessagesAfterAsync(getChannelMessagesAfterAsyncParamsChannelId,
                cancellationTokenSource.Token),
            Times.Exactly(4));

        var deleteMessageAsyncParams = new Discord.Params.DeleteMessageAsyncParams
        {
            ChannelId = channelId,
            MessageId = messageId
        };

        mockDiscord.Verify(
            discord => discord.DeleteMessageAsync(deleteMessageAsyncParams, cancellationTokenSource.Token),
            Times.Exactly(1));

        mockDiscord.Verify(
            discord => discord.CreateMessageAsync(createMessageAsyncParams, cancellationTokenSource.Token),
            Times.Exactly(1));

        mockDiscord.Verify(
            discord => discord.GetChannelMessagesAfterAsync(getChannelMessagesAfterAsyncParamsCreateMessageId,
                cancellationTokenSource.Token),
            Times.Exactly(1));

        mockDiscord.VerifyNoOtherCalls();
    }
}