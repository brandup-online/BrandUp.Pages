using System;

namespace MongoDB.Bson.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class CamelCaseAttribute : Attribute, IBsonMemberMapAttribute
    {
        public void Apply(BsonMemberMap memberMap)
        {
            var elementName = memberMap.ElementName;

            memberMap.SetElementName(string.Concat(elementName.Substring(0, 1).ToLower(), elementName.Substring(1)));
        }
    }
}