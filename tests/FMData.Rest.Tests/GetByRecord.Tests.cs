﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FMData.Rest.Requests;
using FMData.Rest.Tests.TestModels;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Rest.Tests
{
    public class GetByRecordIdTests
    {
        [Fact]
        public async Task GetByRecordId_ShouldReturnMatchingRecordId()
        {
            // arrange
            static object FMrecordIdMapper(User o, int id) => o.FileMakerRecordId = id;
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "Users";
            var recordId = 26;

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Get, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records/{recordId}")
                    .Respond("application/json", DataApiResponses.SuccessfulGetById(recordId));

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), new ConnectionInfo { FmsUri = server, Database = file, Username = user, Password = pass });

            // act
            var response = await fdc.GetByFileMakerIdAsync<User>(layout, recordId, FMrecordIdMapper);

            // assert
            Assert.Equal(recordId, response.FileMakerRecordId);
        }

        [Fact]
        public async Task GetByRecordId_ShouldHaveContainerWithContainerDataFor()
        {
            // arrange
            static object FMrecordIdMapper(ContainerFieldTestModel o, int id) => o.FileMakerRecordId = id;
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "Users";
            var recordId = 26;

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            var b64String = System.IO.File.ReadAllText(System.IO.Path.Combine("ResponseData", "b64-string.dat"));
            var bytes = Convert.FromBase64String(b64String);
            var b64 = new ByteArrayContent(bytes);

            mockHttp.When(HttpMethod.Get, $"{server}/some-data-path")
                    .Respond(b64);

            mockHttp.When(HttpMethod.Get, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records/{recordId}")
                    .Respond("application/json", DataApiResponses.SuccessfulGetByIdWithContainer(recordId, $"{server}/some-data-path"));

            var fdc = new FileMakerRestClient(mockHttp.ToHttpClient(), new ConnectionInfo { FmsUri = server, Database = file, Username = user, Password = pass });

            // act
            var response = await fdc.GetByFileMakerIdAsync<ContainerFieldTestModel>(recordId, FMrecordIdMapper);

            // assert
            Assert.Equal(bytes, response.SomeContainerFieldData);
        }

        [Fact]
        public async Task GetByRecordId_Should_Throw_FMDataException_When_HTTP500()
        {
            // arrange
            static object FMrecordIdMapper(User o, int id) => o.FileMakerRecordId = id;
            var mockHttp = new MockHttpMessageHandler();

            var server = "http://localhost";
            var file = "test-file";
            var user = "unit";
            var pass = "test";
            var layout = "invalid-layout"; // hard-coding an error below, but still
            var recordId = 26;

            mockHttp.When(HttpMethod.Post, $"{server}/fmi/data/v1/databases/{file}/sessions")
                    .Respond("application/json", DataApiResponses.SuccessfulAuthentication());

            mockHttp.When(HttpMethod.Get, $"{server}/fmi/data/v1/databases/{file}/layouts/{layout}/records/{recordId}")
                    .Respond(HttpStatusCode.InternalServerError, "application/json", DataApiResponses.LayoutNotFound());

            var fdc = new FileMakerRestClient(
                mockHttp.ToHttpClient(),
                new ConnectionInfo
                {
                    FmsUri = server,
                    Database = file,
                    Username = user,
                    Password = pass
                });

            // act
            // assert
            await Assert.ThrowsAsync<FMDataException>(async () => await fdc.GetByFileMakerIdAsync<User>(layout, recordId, FMrecordIdMapper));
        }
    }
}
