namespace MythHunter.Resources.Pool
{
    public static class PoolManagerDebugProxy
    {
        public static IPoolManager Instance
        {
            get; private set;
        }

        public static void Register(IPoolManager poolManager)
        {
            Instance = poolManager;
        }
    }
}
