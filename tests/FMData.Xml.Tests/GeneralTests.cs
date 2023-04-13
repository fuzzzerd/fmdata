using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FMData.Xml;
using FMData.Xml.Requests;
using FMData.Xml.Tests.TestModels;
using Xunit;

namespace FMData.Xml.Tests
{
    public class GeneralTests
    {
        [Fact]
        public void Sample_fmresultset_FieldExtraction()
        {
            //arrange 
            var local = XmlResponses.GrammarSample_fmresultset;
            var xdoc = XDocument.Load(new StringReader(local));
            var ns = XNamespace.Get("http://www.filemaker.com/xml/fmresultset");

            // act
            var dict = new Dictionary<string, string>();
            var records = xdoc
                .Descendants(ns + "resultset")
                .Elements(ns + "record")
                .Select(r => new Record()
                {
                    RecordId = Convert.ToInt32(r.Attribute("record-id").Value),
                    ModId = Convert.ToInt32(r.Attribute("mod-id").Value),
                    FieldData = r
                        .Elements(ns + "field")
                        .ToDictionary(
                            k => k.Attribute("name").Value,
                            v => v.Value
                        )
                });

            //assert
            Assert.Contains(records, r => r.RecordId == 14);
            Assert.Contains(records.SelectMany(f => f.FieldData), r => r.Key == "Title");
            Assert.Contains(records.SelectMany(f => f.FieldData), r => r.Value == "Spring in Giverny 3");
        }

        [Fact]
        public void ObjectToDictionary_Should_ProcessData()
        {
            // arrange
            var a = new Art { Title = "Test" };

            // act
            var d = a.AsDictionary(false);

            // assert
            Assert.False(d.ContainsKey("length"));
        }

        [Fact]
        public void DictionaryToModel_ShouldPutData_IntoDataMemberIfSpecified()
        {
            // arrange
            var expected = "theField";
            var d = new Dictionary<string, object> { { "alt-Title", expected } };

            // act
            var a = d.ToObject<ArtDataCM>();

            // assert
            Assert.Equal(expected, a.Title);
        }

        [Fact]
        public void GenCreate_ShouldBeCreateRequest()
        {
            //arrange 
            var fmc = new FileMakerXmlClient("", "", "", "");

            // act
            var req = fmc.GenerateCreateRequest<TestModels.User>();

            //assert
            Assert.IsAssignableFrom<CreateRequest<TestModels.User>>(req);
        }

        [Fact]
        public void GenFind_ShouldBeFindRequest()
        {
            //arrange 
            var fmc = new FileMakerXmlClient("", "", "", "");

            // act
            var req = fmc.GenerateFindRequest<TestModels.User>();

            //assert
            Assert.IsAssignableFrom<FindRequest<TestModels.User>>(req);
        }

        [Fact]
        public void GenEdit_ShouldBeEditRequest()
        {
            //arrange 
            var fmc = new FileMakerXmlClient("", "", "", "");

            // act
            var req = fmc.GenerateEditRequest<TestModels.User>();

            //assert
            Assert.IsAssignableFrom<EditRequest<TestModels.User>>(req);
        }

        [Fact]
        public void GenDelete_ShouldBeDeleteRequest()
        {
            //arrange 
            var fmc = new FileMakerXmlClient("", "", "", "");

            // act
            var req = fmc.GenerateDeleteRequest();

            //assert
            Assert.IsAssignableFrom<DeleteRequest>(req);
        }
    }
}
