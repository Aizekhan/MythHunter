namespace MythHunter.Events.Network
{
    /// <summary>
    /// Explicit composition-root policy for network event synchronization.
    /// This is separate from transport registration and current connection state.
    /// </summary>
    public enum EventNetworkMode
    {
        Offline = 0,
        Client = 1,
        Server = 2
    }
}
