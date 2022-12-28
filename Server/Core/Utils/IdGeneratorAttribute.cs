using System;

namespace AO.Core.Utils
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class IdGeneratorAttribute : Attribute
    {
        public readonly Type IdType;
        public readonly string CustomFileOutputPath;
        public readonly bool GenerateJsonConverters;
        
        public IdGeneratorAttribute(Type idType, bool generateJsonConverters = true)
        {
            IdType = idType;
            GenerateJsonConverters = generateJsonConverters;
        }

        public IdGeneratorAttribute(Type idType, string customFileOutputPath, bool generateJsonConverters = true)
        {
            IdType = idType;
            CustomFileOutputPath = customFileOutputPath;
            GenerateJsonConverters = generateJsonConverters;
        }
    }
}