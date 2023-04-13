﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FMData.Xml.Tests.TestModels;
using RichardSzalay.MockHttp;
using Xunit;

namespace FMData.Xml.Tests
{
    public class FindRequestTests
    {
        readonly string _server = "http://localhost";
        readonly string _file = "test-file";
        readonly string _user = "unit";
        readonly string _pass = "test";

        [Fact]
        public async Task FindRecord_Should_MatchName()
        {
            // arrange 
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{_server}/fmi/xml/fmresultset.xml")
                .WithPartialContent("-find")
                .WithPartialContent(Uri.EscapeDataString("Spring in Giverny 3"))
                .Respond(HttpStatusCode.OK, "application/xml", XmlResponses.GrammarSample_fmresultset);

            var fdc = new FileMakerXmlClient(mockHttp.ToHttpClient(), _server, _file, _user, _pass);

            var toFind = new Art()
            {
                Title = "Spring in Giverny 3"
            };

            var response = await fdc.FindAsync(toFind);

            Assert.NotEmpty(response);
            var first = response.First();
            Assert.Equal("Spring in Giverny 3", first.Title);
        }

        [Fact]
        public async Task FindRecord_Should_MatchLength()
        {
            // arrange 
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{_server}/fmi/xml/fmresultset.xml")
                .WithPartialContent("-find")
                .WithPartialContent(Uri.EscapeDataString("Spring in Giverny 3"))
                .Respond(HttpStatusCode.OK, "application/xml", XmlResponses.GrammarSample_fmresultset);

            var fdc = new FileMakerXmlClient(mockHttp.ToHttpClient(), _server, _file, _user, _pass);

            var findRequest = new Art()
            {
                Title = "Spring in Giverny 3"
            };

            var response = await fdc.FindAsync(findRequest);

            Assert.NotEmpty(response);
            var first = response.First();
            Assert.Equal(19, first.Length);
        }

        [Fact]
        public async Task FindRecord_WithPortal_Should_MatchLength()
        {
            // arrange 
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, $"{_server}/fmi/xml/fmresultset.xml")
                .WithPartialContent("-find")
                .WithPartialContent(Uri.EscapeDataString("Spring in Giverny 3"))
                .Respond(HttpStatusCode.OK, "application/xml", XmlResponses.GrammarSample_fmresultset);

            var fdc = new FileMakerXmlClient(mockHttp.ToHttpClient(), _server, _file, _user, _pass);

            var findRequest = new ArtWithPortal()
            {
                Title = "Spring in Giverny 3"
            };

            var response = await fdc.FindAsync(findRequest);

            Assert.NotEmpty(response);
            var first = response.First();
            Assert.Equal(19, first.Length);
            Assert.Equal("Chicago", first.Locations.First().Location);
        }
    }
}
