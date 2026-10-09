namespace Chat;

public static class Settings
{
    private static char endSymbol = '\n';
    private static char[] prohibitedSymbols =
    {
        '|',
        '\n'
    };
    private static char idPostfix = '|';
    private static int serverPort = 8000;
    private static int funnelPort = 443;
    private static string setNameCommand = "/setname";

    public static char EndSymbol => endSymbol;
    public static char[] ProhibitedSymbols => prohibitedSymbols;
    public static int ServerPort => serverPort;
    public static int FunnelPort => funnelPort;
    public static char IdPostfix => idPostfix;
    public static string SetNameCommand => setNameCommand;

}
