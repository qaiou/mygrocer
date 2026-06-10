namespace MYGROCER.Data
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DESIGN PATTERN 1 — SINGLETON PATTERN
    // Applied to: Database Connection Management
    //
    // PURPOSE:
    // Ensures only ONE instance of the database connection configuration exists
    // throughout the entire application lifecycle. This prevents resource conflicts
    // and ensures consistent connection settings across all modules.
    //
    // WHY SINGLETON HERE:
    // If every module (ProductModule, CartModule, OrderModule) created its own
    // DB connection, we'd have multiple competing connections, wasted memory,
    // and risk of configuration inconsistency.
    //
    // NOTE ON ASP.NET + EF CORE:
    // In ASP.NET Core, EF Core's DbContext is managed by Dependency Injection (DI)
    // which handles connection pooling automatically per HTTP request.
    // This Singleton class demonstrates the Singleton Pattern conceptually
    // and holds the connection string configuration used by AppDbContext.
    // ═══════════════════════════════════════════════════════════════════════════
    public class DbConnectionSingleton
    {
        // The single private static instance — only created once
        private static DbConnectionSingleton? _instance;

        // Thread lock object — ensures thread safety in multi-request web environment
        private static readonly object _lock = new object();

        // The connection string managed by this singleton
        public string ConnectionString { get; private set; }

        // Tracks how many times modules have accessed this connection
        public int AccessCount { get; private set; } = 0;

        // Private constructor — prevents external instantiation with 'new'
        private DbConnectionSingleton(string connectionString)
        {
            ConnectionString = connectionString;
            Console.WriteLine("[Singleton] DbConnectionSingleton instance CREATED.");
        }

        // The single public access point — GetInstance()
        // Uses double-checked locking for thread safety
        public static DbConnectionSingleton GetInstance(string connectionString = "")
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DbConnectionSingleton(connectionString);
                    }
                }
            }

            _instance.AccessCount++;
            Console.WriteLine($"[Singleton] DbConnectionSingleton accessed. Total access count: {_instance.AccessCount}");
            return _instance;
        }

        // Returns a status summary — useful for proving Singleton in presentation
        public string GetStatus()
        {
            return $"Singleton DB Connection | Access Count: {AccessCount} | Connection: {ConnectionString}";
        }
    }
}