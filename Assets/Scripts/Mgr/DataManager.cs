using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Summer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    //这里存储的都是配置文件里面的数据
    public Dictionary<int, SpaceDefine> Spaces;
    public Dictionary<int, UnitDefine> Units;
    public Dictionary<int, SkillDefine> Skills;
    public Dictionary<int, ItemDefine> Items;

    //读取配置文件中的数据
    public void Init()
    {

        Spaces = Load<SpaceDefine>("Data/SpaceDefine");
        Units = Load<UnitDefine>("Data/UnitDefine");
        Skills = Load<SkillDefine>("Data/SkillDefine");
        Items = Load<ItemDefine>("Data/ItemDefine");

    }

    private Dictionary<int, T> Load<T>(string path)
    {
        string jsonStr = Resources.Load<TextAsset>(path).text;
        return JsonConvert.DeserializeObject<Dictionary<int, T>>(jsonStr, settings);
    }


    JsonSerializerSettings settings = new JsonSerializerSettings
    {
        Converters = new JsonConverter[] {
                new FloatArrayConverter(),
                new IntArrayConverter(),
            }
    };


    //float[]
    public class FloatArrayConverter : JsonConverter<float[]>
    {
        public override float[] ReadJson(JsonReader reader, Type objectType, float[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.String)
            {
                string[] values = token.ToString().Replace("[", "").Replace("]", "").Split(',');
                float[] result = new float[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    float.TryParse(values[i], out result[i]);
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, float[] value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
    //int[]
    public class IntArrayConverter : JsonConverter<int[]>
    {
        public override int[] ReadJson(JsonReader reader, Type objectType, int[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.String)
            {
                string[] values = token.ToString().Replace("[", "").Replace("]", "").Split(',');
                int[] result = new int[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    int.TryParse(values[i], out result[i]);
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, int[] value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

}
