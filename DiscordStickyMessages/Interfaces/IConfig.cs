namespace DiscordStickyMessages.Interfaces;

/// <summary>
/// A <see cref="DiscordStickyMessages"/> config.
/// </summary>
public interface IConfig
{
    /// <summary>
    /// Get the IDs of the channels to send paired messages to.
    /// </summary>
    /// <returns>The IDs of the channels to send paired messages to.</returns>
    Dictionary<string, string> ChannelIdsToMessages { get; }

    /// <summary>
    /// Get the ID of the guild.
    /// </summary>
    /// <returns>The ID of the guild.</returns>
    string GuildId { get; }
}