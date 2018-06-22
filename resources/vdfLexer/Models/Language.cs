using System;

namespace VdfLexer.Models {
    public static class Language {
        public const string OBJECT_PATTERN = @"(\bobject\b|\bcd_popup_object\b|\bhcss_cd_object\b)(?=\s+\w+\s+is)";
        public const string PROCEDURE_PATTERN = @"(\bprocedure\b)(?=\s+)";
        public const string FUNCTION_PATTERN = @"(\bfunction\b)(?=(\s+\w+)+(\s+\breturns\b\s+)\s*)";
        public const string OBJECT_NAME_PATTERN = @"\w*(?=\s+\bis\b\s+a)";
        public const string PROCEDURE_NAME_PATTERN = @"(?:\bprocedure\b\s+)(\w+)";
        public const string FUNCTION_NAME_PATTERN = @"(?:\bfunction\b)\s+(\w+)";
    }
}