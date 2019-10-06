namespace Silicon.Core
{
    public static class BotVersion
    {
        public const string Version = "0.1";
#if DEBUG
        public const bool Release = false;
#else
        public const bool Release = true;
#endif
    }
}
