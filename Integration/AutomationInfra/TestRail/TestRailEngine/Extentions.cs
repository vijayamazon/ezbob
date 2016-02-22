namespace TestRailEngine {
    using System;

    public static class MyExtensions {
        public static string ReplaceWordWrapChars(this String str) {
            return str.Replace(",", "-")
                .Replace("\r\n", "    ")
                .Replace("\n", "    ")
                .Replace("\r", "    ");
        }
    }
}