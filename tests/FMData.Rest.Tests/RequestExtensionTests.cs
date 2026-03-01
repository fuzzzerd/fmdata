using System.Linq;
using FMData.Rest.Requests;
using FMData.Rest.Tests.TestModels;
using Xunit;

namespace FMData.Rest.Tests
{
    public class RequestExtensionTests
    {
        #region FindRequest Extensions

        [Fact(DisplayName = "AddCriteria adds query and returns same request")]
        public void AddCriteria_AddsQueryAndReturnsSameRequest()
        {
            var req = new FindRequest<User> { Layout = "Users" };
            var result = req.AddCriteria(new User { Name = "test" }, false);

            Assert.Same(req, result);
            Assert.Single(req.Query);
        }

        [Fact(DisplayName = "AddCriteria with omit sets omit flag")]
        public void AddCriteria_WithOmit_SetsOmitFlag()
        {
            var req = new FindRequest<User> { Layout = "Users" };
            req.AddCriteria(new User { Name = "exclude" }, true);

            Assert.Single(req.Query);
            Assert.True(req.Query.First().Omit);
        }

        [Fact(DisplayName = "AddSortFieldDirection adds sort and returns same request")]
        public void AddSortFieldDirection_AddsSortAndReturnsSameRequest()
        {
            var req = new FindRequest<User> { Layout = "Users" };
            var result = req.AddSortFieldDirection("Name", "ascend");

            Assert.Same(req, result);
            Assert.Single(req.Sort);
            Assert.Equal("Name", req.Sort.First().FieldName);
            Assert.Equal("ascend", req.Sort.First().SortOrder);
        }

        [Fact(DisplayName = "SetLimit sets limit and returns same request")]
        public void SetLimit_SetsLimitAndReturnsSameRequest()
        {
            var req = new FindRequest<User> { Layout = "Users" };
            var result = req.SetLimit(50);

            Assert.Same(req, result);
            Assert.Equal(50, req.Limit);
        }

        [Fact(DisplayName = "SetOffset sets offset and returns same request")]
        public void SetOffset_SetsOffsetAndReturnsSameRequest()
        {
            var req = new FindRequest<User> { Layout = "Users" };
            var result = req.SetOffset(25);

            Assert.Same(req, result);
            Assert.Equal(25, req.Offset);
        }

        [Fact(DisplayName = "UseLayout with string sets layout and returns same request")]
        public void UseLayout_String_SetsLayoutAndReturnsSameRequest()
        {
            var req = new FindRequest<User>();
            var result = req.UseLayout("CustomLayout");

            Assert.Same(req, result);
            Assert.Equal("CustomLayout", req.Layout);
        }

        [Fact(DisplayName = "UseLayout with instance infers layout from DataContract")]
        public void UseLayout_Instance_InfersLayoutFromDataContract()
        {
            var req = new FindRequest<User>();
            var result = req.UseLayout(new User());

            Assert.Same(req, result);
            Assert.Equal("Users", req.Layout);
        }

        [Fact(DisplayName = "LoadContainers sets flag and returns same request")]
        public void LoadContainers_SetsFlagAndReturnsSameRequest()
        {
            var req = new FindRequest<User> { Layout = "Users" };
            var result = req.LoadContainers();

            Assert.Same(req, result);
            Assert.True(req.LoadContainerData);
        }

        [Fact(DisplayName = "SetScript sets script name and returns same request")]
        public void SetScript_SetsScriptNameAndReturnsSameRequest()
        {
            var req = new FindRequest<User> { Layout = "Users" };
            var result = req.SetScript("MyScript");

            Assert.Same(req, result);
            Assert.Equal("MyScript", req.Script);
        }

        [Fact(DisplayName = "SetScript with parameter sets both values")]
        public void SetScript_WithParameter_SetsBothValues()
        {
            var req = new FindRequest<User> { Layout = "Users" };
            req.SetScript("MyScript", "param1");

            Assert.Equal("MyScript", req.Script);
            Assert.Equal("param1", req.ScriptParameter);
        }

        [Fact(DisplayName = "SetScript without parameter does not set parameter")]
        public void SetScript_WithoutParameter_DoesNotSetParameter()
        {
            var req = new FindRequest<User> { Layout = "Users" };
            req.SetScript("MyScript");

            Assert.Equal("MyScript", req.Script);
            Assert.Null(req.ScriptParameter);
        }

        [Fact(DisplayName = "SetPreRequestScript sets pre-request script and parameter")]
        public void SetPreRequestScript_SetsPreRequestScriptAndParameter()
        {
            var req = new FindRequest<User> { Layout = "Users" };
            var result = req.SetPreRequestScript("PreScript", "preParam");

            Assert.Same(req, result);
            Assert.Equal("PreScript", req.PreRequestScript);
            Assert.Equal("preParam", req.PreRequestScriptParameter);
        }

        [Fact(DisplayName = "SetPreSortScript sets pre-sort script and parameter")]
        public void SetPreSortScript_SetsPreSortScriptAndParameter()
        {
            var req = new FindRequest<User> { Layout = "Users" };
            var result = req.SetPreSortScript("SortScript", "sortParam");

            Assert.Same(req, result);
            Assert.Equal("SortScript", req.PreSortScript);
            Assert.Equal("sortParam", req.PreSortScriptParameter);
        }

        [Fact(DisplayName = "IncludePortals adds portal names to request")]
        public void IncludePortals_AddsPortalNamesToRequest()
        {
            var req = new FindRequest<User> { Layout = "Users" };
            var result = req.IncludePortals("Portal1", "Portal2");

            Assert.Same(req, result);
            Assert.Equal(2, req.Portals.Count);
            Assert.Contains(req.Portals, p => p.PortalName == "Portal1");
            Assert.Contains(req.Portals, p => p.PortalName == "Portal2");
        }

        [Fact(DisplayName = "Method chaining works across multiple extensions")]
        public void MethodChaining_WorksAcrossMultipleExtensions()
        {
            var req = new FindRequest<User>();

            req.UseLayout("Users")
               .AddCriteria(new User { Name = "test" }, false)
               .SetLimit(10)
               .SetOffset(5)
               .AddSortFieldDirection("Name", "ascend")
               .SetScript("PostScript", "postParam")
               .SetPreRequestScript("PreScript")
               .SetPreSortScript("SortScript")
               .LoadContainers();

            Assert.Equal("Users", req.Layout);
            Assert.Single(req.Query);
            Assert.Equal(10, req.Limit);
            Assert.Equal(5, req.Offset);
            Assert.Single(req.Sort);
            Assert.Equal("PostScript", req.Script);
            Assert.Equal("PreScript", req.PreRequestScript);
            Assert.Equal("SortScript", req.PreSortScript);
            Assert.True(req.LoadContainerData);
        }

        #endregion

        #region CreateRequest Extensions

        [Fact(DisplayName = "CreateRequest SetData sets data and returns same request")]
        public void CreateRequest_SetData_SetsDataAndReturnsSameRequest()
        {
            var req = new CreateRequest<User>();
            var user = new User { Name = "test" };
            var result = req.SetData(user);

            Assert.Same(req, result);
            Assert.Same(user, req.Data);
        }

        [Fact(DisplayName = "CreateRequest UseLayout with string sets layout")]
        public void CreateRequest_UseLayout_String_SetsLayout()
        {
            var req = new CreateRequest<User>();
            var result = req.UseLayout("CustomLayout");

            Assert.Same(req, result);
            Assert.Equal("CustomLayout", req.Layout);
        }

        [Fact(DisplayName = "CreateRequest UseLayout with instance infers layout")]
        public void CreateRequest_UseLayout_Instance_InfersLayout()
        {
            var req = new CreateRequest<User>();
            var result = req.UseLayout(new User());

            Assert.Same(req, result);
            Assert.Equal("Users", req.Layout);
        }

        [Fact(DisplayName = "CreateRequest SetScript sets script name and parameter")]
        public void CreateRequest_SetScript_SetsScriptNameAndParameter()
        {
            var req = new CreateRequest<User> { Layout = "Users" };
            var result = req.SetScript("MyScript", "param1");

            Assert.Same(req, result);
            Assert.Equal("MyScript", req.Script);
            Assert.Equal("param1", req.ScriptParameter);
        }

        [Fact(DisplayName = "CreateRequest SetPreRequestScript sets values")]
        public void CreateRequest_SetPreRequestScript_SetsValues()
        {
            var req = new CreateRequest<User> { Layout = "Users" };
            var result = req.SetPreRequestScript("PreScript", "preParam");

            Assert.Same(req, result);
            Assert.Equal("PreScript", req.PreRequestScript);
            Assert.Equal("preParam", req.PreRequestScriptParameter);
        }

        [Fact(DisplayName = "CreateRequest SetPreSortScript sets values")]
        public void CreateRequest_SetPreSortScript_SetsValues()
        {
            var req = new CreateRequest<User> { Layout = "Users" };
            var result = req.SetPreSortScript("SortScript", "sortParam");

            Assert.Same(req, result);
            Assert.Equal("SortScript", req.PreSortScript);
            Assert.Equal("sortParam", req.PreSortScriptParameter);
        }

        #endregion

        #region EditRequest Extensions

        [Fact(DisplayName = "EditRequest SetData sets data and returns same request")]
        public void EditRequest_SetData_SetsDataAndReturnsSameRequest()
        {
            var req = new EditRequest<User> { Layout = "Users", RecordId = 1 };
            var user = new User { Name = "updated" };
            var result = req.SetData(user);

            Assert.Same(req, result);
            Assert.Same(user, req.Data);
        }

        [Fact(DisplayName = "EditRequest UseLayout with string sets layout")]
        public void EditRequest_UseLayout_String_SetsLayout()
        {
            var req = new EditRequest<User>();
            var result = req.UseLayout("CustomLayout");

            Assert.Same(req, result);
            Assert.Equal("CustomLayout", req.Layout);
        }

        [Fact(DisplayName = "EditRequest UseLayout with instance infers layout")]
        public void EditRequest_UseLayout_Instance_InfersLayout()
        {
            var req = new EditRequest<User>();
            var result = req.UseLayout(new User());

            Assert.Same(req, result);
            Assert.Equal("Users", req.Layout);
        }

        [Fact(DisplayName = "EditRequest SetScript sets script name and parameter")]
        public void EditRequest_SetScript_SetsScriptNameAndParameter()
        {
            var req = new EditRequest<User> { Layout = "Users", RecordId = 1 };
            var result = req.SetScript("MyScript", "param1");

            Assert.Same(req, result);
            Assert.Equal("MyScript", req.Script);
            Assert.Equal("param1", req.ScriptParameter);
        }

        [Fact(DisplayName = "EditRequest SetPreRequestScript sets values")]
        public void EditRequest_SetPreRequestScript_SetsValues()
        {
            var req = new EditRequest<User> { Layout = "Users", RecordId = 1 };
            var result = req.SetPreRequestScript("PreScript", "preParam");

            Assert.Same(req, result);
            Assert.Equal("PreScript", req.PreRequestScript);
            Assert.Equal("preParam", req.PreRequestScriptParameter);
        }

        [Fact(DisplayName = "EditRequest SetPreSortScript sets values")]
        public void EditRequest_SetPreSortScript_SetsValues()
        {
            var req = new EditRequest<User> { Layout = "Users", RecordId = 1 };
            var result = req.SetPreSortScript("SortScript", "sortParam");

            Assert.Same(req, result);
            Assert.Equal("SortScript", req.PreSortScript);
            Assert.Equal("sortParam", req.PreSortScriptParameter);
        }

        #endregion

        #region IFileMakerRequest (Obsolete) Extensions

        [Fact(DisplayName = "RequestExtensions SetScript on IFileMakerRequest sets values")]
        public void RequestExtensions_SetScript_OnIFileMakerRequest()
        {
            IFileMakerRequest req = new FindRequest<User> { Layout = "Users" };

#pragma warning disable CS0612 // intentionally testing the obsolete extension
            var result = req.SetScript("MyScript", "param1");
#pragma warning restore CS0612

            Assert.Same(req, result);
            Assert.Equal("MyScript", req.Script);
            Assert.Equal("param1", req.ScriptParameter);
        }

        [Fact(DisplayName = "RequestExtensions SetPreRequestScript on IFileMakerRequest sets values")]
        public void RequestExtensions_SetPreRequestScript_OnIFileMakerRequest()
        {
            IFileMakerRequest req = new CreateRequest<User> { Layout = "Users" };

#pragma warning disable CS0612 // intentionally testing the obsolete extension
            var result = req.SetPreRequestScript("PreScript", "preParam");
#pragma warning restore CS0612

            Assert.Same(req, result);
            Assert.Equal("PreScript", req.PreRequestScript);
            Assert.Equal("preParam", req.PreRequestScriptParameter);
        }

        [Fact(DisplayName = "RequestExtensions SetPreSortScript on IFileMakerRequest sets values")]
        public void RequestExtensions_SetPreSortScript_OnIFileMakerRequest()
        {
            IFileMakerRequest req = new EditRequest<User> { Layout = "Users", RecordId = 1 };

#pragma warning disable CS0612 // intentionally testing the obsolete extension
            var result = req.SetPreSortScript("SortScript", "sortParam");
#pragma warning restore CS0612

            Assert.Same(req, result);
            Assert.Equal("SortScript", req.PreSortScript);
            Assert.Equal("sortParam", req.PreSortScriptParameter);
        }

        #endregion

        #region PortalBuilder

        [Fact(DisplayName = "WithPortal creates PortalBuilder and adds portal to request")]
        public void WithPortal_CreatesPortalBuilderAndAddsPortal()
        {
            var req = new FindRequest<User> { Layout = "Users" };
            var builder = req.WithPortal("RelatedTable");

            Assert.NotNull(builder);
            Assert.Single(req.Portals);
            Assert.Equal("RelatedTable", req.Portals.First().PortalName);
        }

        [Fact(DisplayName = "PortalBuilder Limit sets portal limit")]
        public void PortalBuilder_Limit_SetsPortalLimit()
        {
            var req = new FindRequest<User> { Layout = "Users" };
            req.WithPortal("RelatedTable").Limit(10);

            var portal = req.Portals.First(p => p.PortalName == "RelatedTable");
            Assert.Equal(10, portal.Limit);
        }

        [Fact(DisplayName = "PortalBuilder Offset sets portal offset")]
        public void PortalBuilder_Offset_SetsPortalOffset()
        {
            var req = new FindRequest<User> { Layout = "Users" };
            req.WithPortal("RelatedTable").Offset(5);

            var portal = req.Portals.First(p => p.PortalName == "RelatedTable");
            Assert.Equal(5, portal.Offset);
        }

        [Fact(DisplayName = "PortalBuilder chaining sets both limit and offset")]
        public void PortalBuilder_Chaining_SetsBothLimitAndOffset()
        {
            var req = new FindRequest<User> { Layout = "Users" };
            req.WithPortal("RelatedTable").Limit(10).Offset(5);

            var portal = req.Portals.First(p => p.PortalName == "RelatedTable");
            Assert.Equal(10, portal.Limit);
            Assert.Equal(5, portal.Offset);
        }

        [Fact(DisplayName = "PortalBuilder WithPortal chains to another portal")]
        public void PortalBuilder_WithPortal_ChainsToAnotherPortal()
        {
            var req = new FindRequest<User> { Layout = "Users" };

            req.WithPortal("Portal1").Limit(10)
               .WithPortal("Portal2").Offset(3);

            Assert.Equal(2, req.Portals.Count);

            var portal1 = req.Portals.First(p => p.PortalName == "Portal1");
            Assert.Equal(10, portal1.Limit);

            var portal2 = req.Portals.First(p => p.PortalName == "Portal2");
            Assert.Equal(3, portal2.Offset);
        }

        [Fact(DisplayName = "Portal configuration serializes correctly")]
        public void Portal_Configuration_SerializesCorrectly()
        {
            var req = new FindRequest<User> { Layout = "Users" };
            req.AddQuery(new User { Name = "test" }, false);
            req.WithPortal("RelatedRecords").Limit(5).Offset(2);

            var json = req.SerializeRequest();

            Assert.Contains("\"portal\":[\"RelatedRecords\"]", json);
            Assert.Contains("\"limit.RelatedRecords\":5", json);
            Assert.Contains("\"offset.RelatedRecords\":2", json);
        }

        [Fact(DisplayName = "Multiple portals serialize correctly")]
        public void MultiplePortals_SerializeCorrectly()
        {
            var req = new FindRequest<User> { Layout = "Users" };
            req.AddQuery(new User { Name = "test" }, false);

            req.WithPortal("Portal1").Limit(10)
               .WithPortal("Portal2").Limit(20).Offset(5);

            var json = req.SerializeRequest();

            Assert.Contains("Portal1", json);
            Assert.Contains("Portal2", json);
            Assert.Contains("\"limit.Portal1\":10", json);
            Assert.Contains("\"limit.Portal2\":20", json);
            Assert.Contains("\"offset.Portal2\":5", json);
        }

        #endregion
    }
}
