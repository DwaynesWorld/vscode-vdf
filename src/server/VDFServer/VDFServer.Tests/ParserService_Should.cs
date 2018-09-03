using System;
using VDFServer.Data.Enumerations;
using VDFServer.Parser.Services;
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
            var symbol = _parserService.ParseLine(string.Empty, string.Empty);
            Assert.Null(symbol);
        }

        [Fact]
        public void ReturnNullFromLineWithoutDeclaration()
        {
            var line = "Get piActvRecnum to Activity.Recnum";
            var symbol = _parserService.ParseLine(line, line);
            Assert.Null(symbol);
        }

        [Fact]
        public void ReturnClassTypeFromClasstDeclarationLine()
        {
            var line = "Class HCSS_cTextEdit Is A cTextEdit";
            var symbol = _parserService.ParseLine(line, line);
            Assert.Equal(SymbolKind.Class, symbol.Type);
            Assert.Equal("HCSS_cTextEdit", symbol.Name);
        }

        [Fact]
        public void ReturnNullFromLineWithClassWithoutDeclaration()
        {
            var line = "Set Field_Options          Field Bidclass.Class           To DD_CAPSLOCK";
            var symbol = _parserService.ParseLine(line, line);
            Assert.Null(symbol);
        }

        [Fact]
        public void ReturnNullFromLineWithObjectWithoutDeclaration()
        {
            var line = "Object Width    __._ __ (Diameter of circular cross sections)";
            var symbol = _parserService.ParseLine(line, line);
            Assert.Null(symbol);
        }

        [Fact]
        public void ReturnObjectTypeFromObjectDeclarationLine()
        {
            var line = "Object aNotes is a HCSS_cTextEdit";
            var symbol = _parserService.ParseLine(line, line);
            Assert.Equal(SymbolKind.Object, symbol.Type);
            Assert.Equal("aNotes", symbol.Name);
        }

        [Fact]
        public void ReturnProcedureTypeFromProcedureDeclarationLine()
        {
            var line = "Procedure UpdateData";
            var symbol = _parserService.ParseLine(line, line);
            Assert.Equal(SymbolKind.Method, symbol.Type);
            Assert.Equal("UpdateData", symbol.Name);
        }

        [Fact]
        public void ReturnFunctionTypeFromFunctionDeclarationLine()
        {
            var line = "Function Detail_Used String sCode Returns Integer";
            var symbol = _parserService.ParseLine(line, line);
            Assert.Equal(SymbolKind.Function, symbol.Type);
            Assert.Equal("Detail_Used", symbol.Name);
        }

        [Fact]
        public void ReturnFunctionTypeFromFunctionDeclarationLineWithArrayParams()
        {
            var line = "Function BiditemIndex String sBid tBid[] ByRef BidData Returns Integer";
            var symbol = _parserService.ParseLine(line, line);
            Assert.Equal(SymbolKind.Function, symbol.Type);
            Assert.Equal("BiditemIndex", symbol.Name);
        }

        [Fact]
        public void ReturnFunctionTypeFromFunctionDeclarationLineWithArrayParamsAndArrayReturn()
        {
            var line = "Function fAdjustExceptionArray tEstdetlData[] tsEstdetldata Integer iPoint Returns tEstdetlData[]";
            var symbol = _parserService.ParseLine(line, line);
            Assert.Equal(SymbolKind.Function, symbol.Type);
            Assert.Equal("fAdjustExceptionArray", symbol.Name);
        }

        [Fact]
        public void ReturnStructTypeFromStructDeclarationLine()
        {
            var line = "Struct tProductions";
            var symbol = _parserService.ParseLine(line, line);
            Assert.Equal(SymbolKind.Struct, symbol.Type);
            Assert.Equal("tProductions", symbol.Name);
        }
    }
}