using System;
using System.Collections.Generic;
using System.Text;

namespace CsvEngine.Enums
{
    public enum ErrorLevel
    {
        CSV_PROPERTIES = 1,
        DTO_PROPERTIES = 2,
        CSV_STRUCTURAL = 3,
        DTO_STRUCTURAL = 4,
        UNKNOWN = 5,
    }
}