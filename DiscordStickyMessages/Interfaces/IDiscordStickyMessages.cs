namespace DiscordStickyMessages.Interfaces;

/// <summary>
/// A Discord bot instance for creating sticky messages.
/// </summary>
public interface IDiscordStickyMessages
{
    /// <summary>
    /// Run.
    /// </summary>
    /// <param name="cancellationToken"> The cancellation token to cancel operation.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    Task RunAsync(CancellationToken cancellationToken);
}