using System;

namespace VDFServer.Parser
{
    // Be careful modifying this class,
    // some regex can take an inordinate amount
    // of time to process, causing programs to
    // hange and use CPU.
    // If it seems like your process is hanging
    // but still using a ton of CPU, check this first!
    public static class Language
    {
        public const string CLASS_PATTERN = @"(\bclass\b)(?=\s+)";
        public const string END_CLASS_PATTERN = @"(\bend_class\b)(?=\s+)";

        public const string OBJECT_PATTERN = @"(\bobject\b|\bcd_popup_object\b|\bhcss_cd_object\b)(?=\s+)";
        public const string END_OBJECT_PATTERN = @"(\bend_object\b|\bcd_end_object\b)(?=\s+)";

        public const string PROCEDURE_PATTERN = @"(\bprocedure\b)(?=\s+)";
        public const string END_PROCEDURE_PATTERN = @"(\bend_procedure\b)(?=\s+)";

        public const string FUNCTION_PATTERN = @"(\bfunction\b)(?=\s+)";
        public const string END_FUNCTION_PATTERN = @"(\bend_function\b)(?=\s+)";

        public const string STRUCT_PATTERN = @"(\bstruct\b)(?=\s+)";
        public const string END_STRUCT_PATTERN = @"(\bstruct\b)(?=\s+)";


        public const string CLASS_OBJECT_NAME_PATTERN = @"\w*(?=\s+\bis\b\s+a)";
        public const string PROCEDURE_NAME_PATTERN = @"(?:\bprocedure\b\s+)(\w+)";
        public const string STRUCT_NAME_PATTERN = @"(?:\bstruct\b\s+)(\w+)";
        public const string FUNCTION_NAME_PATTERN = @"(?:\bfunction\b)\s+(\w+)";
    }
}