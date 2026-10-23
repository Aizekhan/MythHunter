namespace MythHunter.Events.Network
{
    /// <summary>
    /// Pure policy for deciding whether a locally published network event may be sent.
    /// Offline is never network-authoritative. A combined host mode is deliberately not
    /// represented until the transport layer supports and tests simultaneous client/server.
    /// </summary>
    internal static class EventNetworkAuthorityPolicy
    {
        internal static bool CanTransmit(EventNetworkMode mode, NetworkEventAuthority authority)
        {
            switch (mode)
            {
                case EventNetworkMode.Client:
                    return authority == NetworkEventAuthority.Client ||
                           authority == NetworkEventAuthority.Both;

                case EventNetworkMode.Server:
                    return authority == NetworkEventAuthority.Server ||
                           authority == NetworkEventAuthority.Both;

                case EventNetworkMode.Offline:
                default:
                    return false;
            }
        }
    }
}
