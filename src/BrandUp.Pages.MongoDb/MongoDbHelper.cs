using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrandUp.Pages.MongoDb
{
    public static class MongoDbHelper
    {
        public static BsonDocument DictionaryToBsonDocument(IDictionary<string, object> dictionary)
        {
            return new BsonDocument(dictionary);
        }
        public static IDictionary<string, object> BsonDocumentToDictionary(BsonDocument document)
        {
            var result = new Dictionary<string, object>();

            foreach (var element in document.Elements)
            {
                if (element.Value.IsBsonArray)
                {
                    var list = new List<IDictionary<string, object>>();
                    foreach (var d in element.Value.AsBsonArray)
                        list.Add(BsonDocumentToDictionary(d.AsBsonDocument));
                    result.Add(element.Name, list);
                }
                else if (element.Value.IsBsonDocument)
                    result.Add(element.Name, BsonDocumentToDictionary(element.Value.AsBsonDocument));
                else
                    result.Add(element.Name, BsonTypeMapper.MapToDotNetValue(element.Value));
            }

            return result;
        }
    }
}