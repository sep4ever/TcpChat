namespace Chat;

public static class Settings
{
    private static char endSymbol = '\n';
    private static char[] prohibitedSymbols =
    {
        ':',
        '\n'
    };
    private static string ipAddress = "127.0.0.1";
    public static char EndSymbol => endSymbol;
    public static char[] ProhibitedSymbols => prohibitedSymbols;
    public static string IpAddress => ipAddress;
}
