namespace ShoppingCartService.Models.Constants
{
    /// <summary>
    /// Serilog also allowsyou to enrich log messages, which means adding extra properties to them. 
    /// Enriching can be used to add a server name, an environment name (such as QA or production), 
    /// the role of the user who initiated the request, or whatever else you’re interested in 
    /// putting in log messages that isn’t readily available in the code where you write the messages.
    /// </summary>
    public class LoggingTemplate
    {
        public static string Default = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";
        public static string DefaultWithCorrelationToken = "{NewLine}[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationToken}) {Message:lj} {Properties:j}{NewLine}{Exception}";
        public static string RequestTemplate = "Handled [{RequestMethod}] {RequestPath} - {StatusCode} - {Elapsed}";
    }
}