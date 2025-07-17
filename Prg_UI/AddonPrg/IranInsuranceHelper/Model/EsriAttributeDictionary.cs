using System.Collections.Generic;

namespace Dbf
{
    public class EsriAttributeDictionary
    {
        public List<Dictionary<string, object>> Attributes { get; set; }

        public List<DbfFieldDescriptor> Fields { get; set; }

        public EsriAttributeDictionary()
        {

        }

        public EsriAttributeDictionary(List<Dictionary<string,object>> attributes, List<DbfFieldDescriptor> fields)
        {
            this.Attributes = attributes;

            this.Fields = fields;
        }
    }
}
