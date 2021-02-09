using System;
using System.Collections.Generic;
using System.Text;

namespace CsvFile.Error
{
    public class Errors
    {
        public Dictionary<string, string> Properties { get; set; }
        public Dictionary<string, string> Types { get; set; }
        public Errors()
        {
            this.Properties = new Dictionary<string, string>();
            this.Types = new Dictionary<string, string>();
        }
    }
}
