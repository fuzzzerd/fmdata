using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using FMData.Rest.Requests;
using FMData.Rest.Tests.TestModels;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Rest.Tests
{
    public class GeneralTests
    {
        private FindRequest<User> FindReq()
        {
            var r = new FindRequest<User>() { Layout = "layout" };
            r.AddQuery(new User() { Id = 1 }, false);
            return r;
        }

        [DataContract(Name = "SomeName")]
        private class DCModel
        {
            public DCModel()
            {
            }

            public string Name { get; set; }
        }

        [Fact]
        public void GetTable_ShouldWorkForDataContract()
        {
            //arrange 
            var mx = new DCModel() { Name = "Name" };

            // act
            var name = FileMakerApiClientBase.GetLayoutName(mx);

            //assert
            Assert.Equal("SomeName", name);
        }

        [Fact]
        public void FindRequest_Numbers_ShouldSerialize_ToStrings_ForFileMaker()
        {
            //arrange 
            var r = FindReq();

            // act
            var json = r.SerializeRequest();

            //assert
            Assert.Contains("\"Id\":", json);
            Assert.Contains("\"1\"", json);
        }

        [Fact]
        public void FindRequest_Numbers_ShouldNotSerialize_ToNumbers()
        {
            //arrange 
            var r = FindReq();

            // act
            var json = r.SerializeRequest();

            //assert
            Assert.DoesNotContain("\"Id\":1", json);
        }

        [Fact]
        public void Nullable_Model_Property_Should_Be_EmptyString_When_Serialized_With_NullValueHandling_Include()
        {
            //arrange 
            var r = new EditRequest<User>() { Data = new User { Id = 94, ForeignKeyId = null } };
            r.IncludeDefaultValuesInSerializedOutput = true;
            r.IncludeNullValuesInSerializedOutput = true;

            // act
            var json = r.SerializeRequest();

            //assert
            Assert.Contains("\"ForeignKeyId\":\"\"", json);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void IRequest_ShouldNot_Include_Null_FileMaker_Operators(bool includeNull, bool includeDefault)
        {
            //arrange 
            var r = FindReq();

            r.IncludeNullValuesInSerializedOutput = includeNull;
            r.IncludeDefaultValuesInSerializedOutput = includeDefault;

            // act
            var json = r.SerializeRequest();

            //assert
            Assert.DoesNotContain("\"layout.response\":", json);
            Assert.DoesNotContain("\"script\":", json);
            Assert.DoesNotContain("\"script.param\":", json);
            Assert.DoesNotContain("\"script.prerequest\":", json);
            Assert.DoesNotContain("\"script.prerequest.param\":", json);
            Assert.DoesNotContain("\"script.presort\":", json);
            Assert.DoesNotContain("\"script.presort.param\":", json);
        }

        [Fact]
        public void GenCreate_ShouldBeCreateRequest()
        {
            //arrange 
            var fmc = new FileMakerRestClient(new HttpClient(), new ConnectionInfo { FmsUri = "", Database = "", Username = "", Password = "" });

            // act
            var req = fmc.GenerateCreateRequest<TestModels.User>();

            //assert
            Assert.IsAssignableFrom<CreateRequest<TestModels.User>>(req);
        }

        [Fact]
        public void GenFind_ShouldBeFindRequest()
        {
            //arrange 
            var fmc = new FileMakerRestClient(new HttpClient(), new ConnectionInfo { FmsUri = "", Database = "", Username = "", Password = "" });

            // act
            var req = fmc.GenerateFindRequest<TestModels.User>();

            //assert
            Assert.IsAssignableFrom<FindRequest<TestModels.User>>(req);
        }

        [Fact]
        public void GenEdit_ShouldBeEditRequest()
        {
            //arrange 
            var fmc = new FileMakerRestClient(new HttpClient(), new ConnectionInfo { FmsUri = "", Database = "", Username = "", Password = "" });

            // act
            var req = fmc.GenerateEditRequest<TestModels.User>();

            //assert
            Assert.IsAssignableFrom<EditRequest<TestModels.User>>(req);
        }

        [Fact]
        public void GenDelete_ShouldBeDeleteRequest()
        {
            //arrange 
            var fmc = new FileMakerRestClient(new HttpClient(), new ConnectionInfo { FmsUri = "", Database = "", Username = "", Password = "" });

            // act
            var req = fmc.GenerateDeleteRequest();

            //assert
            Assert.IsAssignableFrom<DeleteRequest>(req);
        }

        [Fact]
        public async Task Test_DateTime_To_Timestamp_Parsing()
        {
            // arrange
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "Users";

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                           .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/_find")
                .WithPartialContent("fuzzzerd") // ensure the request contains the expected content
                .Respond("application/json", DataApiResponses.SuccessfulFind());

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), new ConnectionInfo { FmsUri = server, Database = file, Username = user, Password = pass });

            // act
            var response = await fdc.FindAsync(new User()
            {
                Name = "fuzzzerd"
            });

            // assert
            var responseDataContainsResult = response.Any(r => r.Created == DateTime.Parse("03/29/2018 15:22:09"));
            Assert.True(responseDataContainsResult);
        }
    }
}