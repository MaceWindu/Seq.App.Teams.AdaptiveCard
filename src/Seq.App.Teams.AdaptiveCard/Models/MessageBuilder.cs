namespace Seq.App.Teams.Models;

internal static class MessageBuilder
{
    private const string WRAP_PREFIX = @"{
   ""type"":""message"",
   ""attachments"":[
      {
         ""contentType"":""application/vnd.microsoft.card.adaptive"",
         ""content"":";

    private const string WRAP_SUFFIX = @"
      }
   ]
}";

    public static string Wrap(string json)
    {
        return WRAP_PREFIX + json + WRAP_SUFFIX;
    }
}
