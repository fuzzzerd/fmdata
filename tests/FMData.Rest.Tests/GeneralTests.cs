using System.Collections.Generic;
using FMData.Rest.Requests;
using FMData.Tests.TestModels;
using Xunit;

namespace FMData.Tests
{
    public class GeneralTests
    {
        private FindRequest<User> FindReq => new FindRequest<User>()
        {
            Query = new List<User>()
            {
                new User()
                {
                    Id =1
                }
            },
            Layout = "layout"
        };

        [Fact]
        public void FindRequest_Numbers_ShouldSerialize_ToStrings_ForFileMaker()
        {
            //arrange 
            var r = FindReq;

            // act
            var json = r.ToJson();

            //assert
            Assert.Contains("\"Id\":\"1\"", json);
        }

        [Fact]
        public void FindRequest_Numbers_ShouldNotSerialize_ToNumbers()
        {
            //arrange 
            var r = FindReq;

            // act
            var json = r.ToJson();

            //assert
            Assert.DoesNotContain("\"Id\":1", json);
        }
    }
}