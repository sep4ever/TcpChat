namespace Chat;

public static class Settings
{
    private static char endSymbol = '\n';
    private static char[] prohibitedSymbols =
    {
        ':',
        '\n'
    };
    private static int port = 8000;
    public static char EndSymbol => endSymbol;
    public static char[] ProhibitedSymbols => prohibitedSymbols;
    public static int Port => port;
}
