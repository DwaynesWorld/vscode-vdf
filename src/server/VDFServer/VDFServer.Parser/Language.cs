using System;

namespace VDFServer.Parser
{
    public static class Language
    {
        public const string CLASS_PATTERN = @"(\bclass\b)(?=\s+)";
        public const string OBJECT_PATTERN = @"(\bobject\b|\bcd_popup_object\b|\bhcss_cd_object\b)(?=\s+\w+\s+is)";
        public const string PROCEDURE_PATTERN = @"(\bprocedure\b)(?=\s+)";
        public const string FUNCTION_PATTERN = @"(\bfunction\b)(?=(\s+\w+)+(\s+\breturns\b\s+)\s*)";
        public const string STRUCT_PATTERN = @"(\bstruct\b)(?=\s+)";

        public const string CLASS_OBJECT_NAME_PATTERN = @"\w*(?=\s+\bis\b\s+a)";
        public const string PROCEDURE_NAME_PATTERN = @"(?:\bprocedure\b\s+)(\w+)";
        public const string STRUCT_NAME_PATTERN = @"(?:\bstruct\b\s+)(\w+)";
        public const string FUNCTION_NAME_PATTERN = @"(?:\bfunction\b)\s+(\w+)";
    }
}