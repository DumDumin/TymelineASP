using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ListIPermissionsConverter : JsonConverter
{
    public override bool CanWrite => false;
    public override bool CanRead => true;
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(List<IRole>);
    }
    public override void WriteJson(JsonWriter writer,
        object value, JsonSerializer serializer)
    {
        throw new InvalidOperationException("Use default serialization.");
    }

    public override List<IRole> ReadJson(JsonReader reader,
        Type objectType, object existingValue,
        JsonSerializer serializer)
    {
        var jsonArray = JArray.Load(reader);
        var permissionList = new List<IRole>();
        foreach (var item in jsonArray)
            {

                // Create a form field instance by the field type ID.
                var jsonObject = item as JObject;
                var key = jsonObject["type"].Value<string>();
                var value = jsonObject["value"].Value<string>();
                var instance = new Role(key,value);


                // Populate the form field instance.
                serializer.Populate(jsonObject.CreateReader(), instance);
                permissionList.Add(instance);

            }
        return permissionList;
    }
}


public class IPermissionConverter : JsonConverter
{
    public override bool CanWrite => false;
    public override bool CanRead => true;
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(IRole);
    }
    public override void WriteJson(JsonWriter writer,
        object value, JsonSerializer serializer)
    {
        throw new InvalidOperationException("Use default serialization.");
    }

    public override IRole ReadJson(JsonReader reader,
        Type objectType, object existingValue,
        JsonSerializer serializer)
    {

        JObject jsonObject = JObject.Load(reader);
        var perm = new Role(jsonObject["type"].Value<string>(),jsonObject["value"].Value<string>());
        // serializer.Populate(jsonObject.CreateReader(),perm);
        return perm;


       
    }
}


