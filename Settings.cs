namespace Chat;

public static class Settings
{
    private static char endSymbol = '\n';
    private static char[] prohibitedSymbols =
    {
        ':',
        '\n'
    };

    public static char EndSymbol => endSymbol;
    public static char[] ProhibitedSymbols => prohibitedSymbols;
}
