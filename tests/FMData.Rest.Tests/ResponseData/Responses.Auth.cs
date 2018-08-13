namespace FMData.Rest.Tests
{
    public static partial class DataApiResponses
    {
        public const string TestToken = "16e798b286a78f6b64e234d8a6eeff7d71ba92f6c882b5ff328";
        public const string TestLayout = "layout";
        public static string SuccessfulAuthentication(
            string token = TestToken,
            string layout = TestLayout) => $@"{{
    ""response"": {{
        ""token"": ""{token}""
    }},
    ""messages"": [
        {{
            ""code"": ""0"",
            ""message"": ""OK""
        }}
    ]
}}";
        public static string Authentication401() => @"{""messages"":[{""message"":""HTTP Authorization header or OAuth headers are missing."",""code"":""10""}],""response"":{}}";
    }
}