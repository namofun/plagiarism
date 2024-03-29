﻿namespace Xylab.PlagiarismDetect.Frontend.Csharp
{
    public enum TokenConstants : int
    {
        FILE_END = 0,
        SEPARATOR_TOKEN = 1,
        NAMESPACE_BEGIN = 2,
        NAMESPACE_END = 3,
        CLASS_BEGIN = 4,
        CLASS_END = 5,
        METHOD_BEGIN = 6,
        METHOD_END = 7,
        VARDEF = 8,
        LOCK_BEGIN = 9,
        LOCK_END = 10,
        DO_BEGIN = 11,
        DO_END = 12,
        WHILE_BEGIN = 13,
        WHILE_END = 14,
        FOR_BEGIN = 15,
        FOR_END = 16,
        SWITCH_BEGIN = 17,
        SWITCH_END = 18,
        CASE = 19,
        TRY_BEGIN = 20,
        CATCH_BEGIN = 21,
        CATCH_END = 22,
        FINALLY = 23,
        IF_BEGIN = 24,
        ELSE = 25,
        IF_END = 26,
        COND = 27,
        BREAK = 28,
        CONTINUE = 29,
        RETURN = 30,
        THROW = 31,
        INTERFACE_BEGIN = 32,
        INTERFACE_END = 33,
        ENUM_BEGIN = 34,
        ENUM_END = 35,
        UNSAFE = 36,
        LAMBDA = 37,
        GENERIC = 38,
        NEW_ARRAY = 39,
        NEW_OBJECT = 40,
        USING_RESOURCE = 41,
        ASSIGN = 42,
        METHOD_INVOCATION = 43,
        STRUCT_BEGIN = 44,
        STRUCT_END = 45,
        ATTRIBUTE_BEGIN = 46,
        ATTRIBUTE_END = 47,
        GENERIC_CONSTRAINT = 48,
        PATTERN_MATCHING = 49,
        PROPERTY_BEGIN = 50,
        PROPERTY_END = 51,
        EVENT_BEGIN = 52,
        EVENT_END = 53,
        FIXED_POINTER = 54,
        DELEGATE_DECL = 55,
        LINQ_QUERY_BODY = 56,
        FOREACH_BEGIN = 57,
        FOREACH_END = 58,
        TRY_END = 59,
        RANGE = 60,
        NUM_DIFF_TOKENS = 61,
    }
}
