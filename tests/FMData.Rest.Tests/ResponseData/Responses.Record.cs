using System.IO;

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

        public static string SuccessfulGetByIdWithContainer(int id, string containerPath) => $@"{{
    ""response"": {{
        ""data"": [
            {{
                ""fieldData"": {{
                    ""Id"": ""4"",
                    ""Name"": ""fuzzzerd"",
                    ""Created"": ""03/29/2018 15:22:09"",
                    ""Modified"": ""03/29/2018 15:22:12"",
                    ""SomeContainerField"": ""{containerPath}""
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

        public static string FieldNotFound() => @"{
    ""response"": {},
    ""messages"":[{""code"":""102"",""message"":""Field missing""}]
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
            return System.IO.File.ReadAllText(Path.Combine("ResponseData", "fms-find-with-portal.json"));
        }

        public static string SuccessfulFindWithDataInfo()
        {
            return System.IO.File.ReadAllText(Path.Combine("ResponseData", "fms-find-with-datainfo.json"));
        }

        public static string SuccessfulCreateWithScript(int createdId = 254) => $@"{{
    ""response"": {{
        ""recordId"": {createdId},
        ""scriptError"": 0,
        ""scriptResult"": ""create-script-result"",
        ""scriptError.prerequest"": 0,
        ""scriptResult.prerequest"": ""create-prerequest-result"",
        ""scriptError.presort"": 0,
        ""scriptResult.presort"": ""create-presort-result""
    }},
    ""messages"":[{{""code"":""0"",""message"":""OK""}}]
}}";

        public static string SuccessfulEditWithScript(int modId = 3) => $@"{{
    ""response"": {{
        ""modId"": ""{modId}"",
        ""scriptError"": 0,
        ""scriptResult"": ""edit-script-result"",
        ""scriptError.prerequest"": 0,
        ""scriptResult.prerequest"": ""edit-prerequest-result"",
        ""scriptError.presort"": 0,
        ""scriptResult.presort"": ""edit-presort-result""
    }},
    ""messages"":[{{""code"":""0"",""message"":""OK""}}]
}}";

        public static string SuccessfulDeleteWithScript() => @"{
    ""response"": {
        ""scriptError"": 0,
        ""scriptResult"": ""delete-script-result"",
        ""scriptError.prerequest"": 0,
        ""scriptResult.prerequest"": ""delete-prerequest-result"",
        ""scriptError.presort"": 0,
        ""scriptResult.presort"": ""delete-presort-result""
    },
    ""messages"":[{""code"":""0"",""message"":""OK""}]
}";

        public static string SuccessfulFindWithScript() => $@"{{
    ""response"": {{
        ""dataInfo"": {{
            ""database"": ""DATABASE"",
            ""layout"": ""Layout"",
            ""table"": ""Table"",
            ""totalRecordCount"": 100,
            ""foundCount"": 1,
            ""returnedCount"": 1
        }},
        ""scriptError"": 0,
        ""scriptResult"": ""find-script-result"",
        ""scriptError.prerequest"": 0,
        ""scriptResult.prerequest"": ""find-prerequest-result"",
        ""scriptError.presort"": 0,
        ""scriptResult.presort"": ""find-presort-result"",
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
            }}
        ]
    }},
    ""messages"":[{{""code"":""0"",""message"":""OK""}}]
}}";

        public static string SuccessfulCreateWithScriptError() => @"{
    ""response"": {
        ""recordId"": 254,
        ""scriptError"": 3,
        ""scriptResult"": """",
        ""scriptError.prerequest"": 5,
        ""scriptResult.prerequest"": """",
        ""scriptError.presort"": 7,
        ""scriptResult.presort"": """"
    },
    ""messages"":[{""code"":""0"",""message"":""OK""}]
}";
    }
}
