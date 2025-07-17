using System;
using System.Collections.Generic;

namespace Dbf
{
    public class ObjectToDfbFields<T>
    {
        public Func<T, List<Object>> ExtractAttributesFunc { get; set; }

        public List<Dbf.DbfFieldDescriptor> Fields { get; set; }
    }
}
