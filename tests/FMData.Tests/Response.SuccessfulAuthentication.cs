namespace FMREST.Tests
{
    public static partial class DataApiResponses
    {
        public static string SuccessfulAuthentication(
            string token = "16e798b286a78f6b64e234d8a6eeff7d71ba92f6c882b5ff328", 
            string layout = "layout") => $@"{{
    ""token"": ""{token}"",
    ""layout"": ""{layout}"",
    ""errorCode"": ""0"",
    ""result"": ""OK""
}}";
    }
}