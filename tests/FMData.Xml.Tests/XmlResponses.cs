using System;
using System.Collections.Generic;
using System.Text;

namespace FMData.Xml.Tests
{
    public static class XmlResponses
    {
        public const string GrammarSample_fmresultset = @"<fmresultset xmlns=""http://www.filemaker.com/xml/fmresultset"" version=""1.0"">
<error code=""0""/>
<product build=""03/29/2017"" name=""FileMaker Web Publishing Engine"" version=""16.0.1.0""/>
<datasource database=""art"" date-format=""MM/dd/yyyy"" layout=""web3"" table=""art"" time-format=""HH:mm:ss"" timestamp-format=""MM/dd/yyyy HH:mm:ss"" total-count=""12""/>
<metadata>
<field-definition auto-enter=""no"" four-digit-year=""no"" global=""no"" max-repeat=""1"" name=""Title"" not-empty=""no"" numeric-only=""no"" result=""text"" time-of-day=""no"" type=""normal""/>
<field-definition auto-enter=""no"" four-digit-year=""no"" global=""no"" max-repeat=""1"" name=""Artist"" not-empty=""no"" numeric-only=""no"" result=""text"" time-of-day=""no"" type=""normal""/>
<field-definition auto-enter=""no"" four-digit-year=""no"" global=""no"" max-repeat=""1"" name=""Style"" not-empty=""no"" numeric-only=""no"" result=""text"" time-of-day=""no"" type=""normal""/>
<field-definition auto-enter=""no"" four-digit-year=""no"" global=""no"" max-repeat=""1"" name=""length"" not-empty=""no"" numeric-only=""no"" result=""number"" time-of-day=""no"" type=""calculation""/>
<relatedset-definition table=""artlocations"">
<field-definition auto-enter=""no"" four-digit-year=""no"" global=""no"" max-repeat=""1"" name=""artlocations::Location"" not-empty=""no"" numeric-only=""no"" result=""text"" time-of-day=""no"" type=""normal""/>
<field-definition auto-enter=""no"" four-digit-year=""no"" global=""no"" max-repeat=""1"" name=""artlocations::Date"" not-empty=""no"" numeric-only=""no"" result=""date"" time-of-day=""no"" type=""normal""/>
</relatedset-definition>
</metadata>
<resultset count=""1"" fetch-size=""1"">
<record mod-id=""6"" record-id=""14"">
<field name=""Title"">
<data>Spring in Giverny 3</data>
</field>
<field name=""Artist"">
<data>Claude Monet</data>
</field>
<field name=""Style"">
<data/>
</field>
<field name=""length"">
<data>19</data>
</field>
<relatedset count=""0"" table=""artlocations"">
<record>
<field name=""artlocations::Location"">
<data>Chicago</data>
</field>
<field name=""artlocations::Date"">
<data>08/08/15</data>
</field>
</record>
</relatedset>
</record>
</resultset>
</fmresultset>";
    }
}
