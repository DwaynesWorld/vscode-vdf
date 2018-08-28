using System;
using VDFServer.Data.Enumerations;
using VDFServer.Parser.Service;
using Xunit;

namespace VDFServer.Tests
{
    public class ParserService_Should
    {
        private readonly InternalParser _parserService;

        public ParserService_Should()
        {
            _parserService = new InternalParser();
        }

        [Fact]
        public void ReturnNullFromBlankLine()
        {
            var tag = _parserService.ParseLine(string.Empty, string.Empty);
            Assert.Null(tag);
        }

        [Fact]
        public void ReturnNullFromLineWithoutDeclaration()
        {
            var line = "Get piActvRecnum to Activity.Recnum";
            var tag = _parserService.ParseLine(line, line);
            Assert.Null(tag);
        }

        [Fact]
        public void ReturnClassTypeFromClasstDeclarationLine()
        {
            var line = "Class HCSS_cTextEdit Is A cTextEdit";
            var tag = _parserService.ParseLine(line, line);
            Assert.Equal(TagType.Class, tag.Type);
        }

        [Fact]
        public void ReturnObjectTypeFromObjectDeclarationLine()
        {
            var line = "Object aNotes is a HCSS_cTextEdit";
            var tag = _parserService.ParseLine(line, line);
            Assert.Equal(TagType.Object, tag.Type);
        }

        [Fact]
        public void ReturnProcedureTypeFromProcedureDeclarationLine()
        {
            var line = "Procedure UpdateData";
            var tag = _parserService.ParseLine(line, line);
            Assert.Equal(TagType.Procedure, tag.Type);
        }

        [Fact]
        public void ReturnFunctionTypeFromFunctionDeclarationLine()
        {
            var line = "Function Detail_Used String sCode Returns Integer";
            var tag = _parserService.ParseLine(line, line);
            Assert.Equal(TagType.Function, tag.Type);
        }

        [Fact]
        public void ReturnFunctionTypeFromFunctionDeclarationLineWithArrayParams()
        {
            var line = "Function BiditemIndex String sBid tBid[] ByRef BidData Returns Integer";
            var tag = _parserService.ParseLine(line, line);
            Assert.Equal(TagType.Function, tag.Type);
        }

        [Fact]
        public void ReturnStructTypeFromStructDeclarationLine()
        {
            var line = "Struct tProductions";
            var tag = _parserService.ParseLine(line, line);
            Assert.Equal(TagType.Struct, tag.Type);
        }
    }
}