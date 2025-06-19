namespace CounterSplit
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }


    public static class Config
    {
        public static int SelectedGame = 1;
        public static int SelectedPlatform = 1;
        public static string SelectedDifficulty = "Very Hard";
        public static bool OHKO = false;
        public static bool OHCRASH = false;
    }
}