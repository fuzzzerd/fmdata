namespace FMData.Rest.Tests
{
    public static partial class DataApiResponses
    {
        public static string SuccessfulFind() => $@"{{
    ""response"": {{
        ""data"": [
            {{
                ""fieldData"": {{
                    ""Id"": ""4"",
                    ""Name"": ""fuzzzerd"",
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
                    ""Name"": ""Fuzzzerd Buzz"",
                    ""Created"": ""03/07/2018 16:54:34"",
                    ""Modified"": ""04/05/2018 21:34:55""
                }},
                ""portalData"": {{}},
                ""recordId"": ""1"",
                ""modId"": ""12""
            }}
        ]
    }},
    ""messages"":[{{""code"":""0"",""message"":""OK""}}]
}}";

        public static string SuccessfulGetById(int id) => $@"{{
    ""response"": {{
        ""data"": [
            {{
                ""fieldData"": {{
                    ""Id"": ""4"",
                    ""Name"": ""fuzzzerd"",
                    ""Created"": ""03/29/2018 15:22:09"",
                    ""Modified"": ""03/29/2018 15:22:12""
                }},
                ""portalData"": {{}},
                ""recordId"": ""{id}"",
                ""modId"": ""0""
            }}
        ]
    }},
    ""messages"":[{{""code"":""0"",""message"":""OK""}}]
}}";

        public static string LayoutNotFound() => @"{
    ""response"": {},
    ""messages"":[{""code"":""105"",""message"":""Layout is missing""}]
}";

        public static string FindNotFound() => @"{
    ""response"": {},
    ""messages"":[{""code"":""401"",""message"":""No records match the request""}]
}";

        public static string SuccessfulCreate(int createdId = 254) => $@"{{
    ""response"": {{""recordId"":{createdId}}},
    ""messages"":[{{""code"":""0"",""message"":""OK""}}]
}}";

        public static string SuccessfulEdit(int modId = 3) => @"{
    ""response"": { ""modId"": """ + modId.ToString() + @""" },
    ""messages"":[{""code"":""0"",""message"":""OK""}]
}";

        public static string SetGlobalSuccess() => @"{
    ""response"": {},
    ""messages"":[{""code"":""0"",""message"":""OK""}]
}";


        public static string SuccessfulDelete() => @"{
    ""response"": {},
    ""messages"":[{""code"":""0"",""message"":""OK""}]
}";
        public static string UploadPayloadError() => @"{
    ""messages"": [
        {
            ""message"": ""Upload payload must contain a part named 'upload'."",
            ""code"": ""960""
        }
    ],
    ""response"": {}
}";

        public static string SuccessfulFindWithPortal()
        {
            return System.IO.File.ReadAllText("ResponseData\\fms-find-with-portal.json");
        }

    }
}