namespace LocalAiAssistant.Utilities
{
#pragma warning disable IDE0051 // Remove unused private members
    internal class MyTextUtils
    {
        private static string? ExtractFileSnippet(string input, string beginDenotation = "```", string endDenotation = "```")
        {
            int startIndex = input.IndexOf(beginDenotation, StringComparison.Ordinal);
            if (startIndex == -1)
            {
                return null;
            }

            int endIndex = input.IndexOf(endDenotation, startIndex + endDenotation.Length, StringComparison.Ordinal);
            return endIndex == -1
                ? null
                : input.Substring(startIndex + beginDenotation.Length, endIndex - startIndex - endDenotation.Length).Trim();
        }
    }
#pragma warning restore IDE0051 // Remove unused private members
}