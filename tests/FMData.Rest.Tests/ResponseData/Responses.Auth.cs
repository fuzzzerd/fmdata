namespace FMData.Rest.Tests
{
    public static partial class DataApiResponses
    {
        public const string TestToken = "{something-secret}";
        public const string TestLayout = "layout";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "IDE0060", Justification = "Used for generic parameter.")]
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
