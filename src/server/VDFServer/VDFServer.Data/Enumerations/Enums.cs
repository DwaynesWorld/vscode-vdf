namespace VDFServer.Data.Enumerations
{
    public enum TagType
    {
        Class,
        Object,
        Function,
        Procedure,
        Struct
    }

    public enum CommandType
    {
        Arguments,
        Completions,
        Hover,
        Usages,
        Definitions,
        Symbols
    }
}