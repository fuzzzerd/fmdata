namespace FMData.Tests
{
    public static partial class DataApiResponses
    {
        public static string SuccessfulFind() => $@"{{
    ""errorCode"": ""0"",
    ""result"": ""OK"",
    ""data"": [
        {{
            ""fieldData"": {{
                ""Id"": ""4"",
                ""Name"": ""Bross"",
                ""Created"": ""03/29/2018 15:22:09"",
                ""Modified"": ""03/29/2018 15:22:12""
            }},
            ""portalData"": {{}},
            ""recordId"": ""4"",
            ""modId"": ""0""
        }},
        {{
            ""fieldData"": {{
                ""Id"": ""1"",
                ""Name"": ""Nate Bross"",
                ""Created"": ""03/07/2018 16:54:34"",
                ""Modified"": ""04/05/2018 21:34:55""
            }},
            ""portalData"": {{}},
            ""recordId"": ""1"",
            ""modId"": ""12""
        }}
    ]
}}";

        public static string SuccessfulCreate() => $@"{{
  ""errorCode"": ""0"",
  ""recordId"": ""25""
}}";
    }
}