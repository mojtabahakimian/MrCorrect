using System;

namespace Dbf
{
    public class ObjectToDbfTypeMap<T>
    {
        public Func<T, object> MapFunction { get; set; }

        public DbfFieldDescriptor FieldType { get; set; }

        public ObjectToDbfTypeMap(DbfFieldDescriptor fieldType, Func<T, object> mapFunction)
        {
            FieldType = fieldType;

            MapFunction = mapFunction;
        }
    }
}
