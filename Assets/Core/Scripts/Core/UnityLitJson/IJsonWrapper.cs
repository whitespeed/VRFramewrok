using System.Collections;
using System.Collections.Specialized;

namespace Framework.LitJson
{
    public enum JsonType
    {
        None,

        Object,
        Array,
        String,
        Int,
        Float,
        Long,
        Double,
        Boolean
    }

    public interface IJsonWrapper : IList, IOrderedDictionary
    {
        bool IsArray { get; }
        bool IsBoolean { get; }
        bool IsDouble { get; }
        bool IsInt { get; }
        bool IsFloat { get; }
        bool IsLong { get; }
        bool IsObject { get; }
        bool IsString { get; }

        bool GetBoolean();
        double GetDouble();
        int GetInt();
        JsonType GetJsonType();
        long GetLong();
        string GetString();
        float GetFloat();
        void SetFloat(float value);
        void SetBoolean(bool val);
        void SetDouble(double val);
        void SetInt(int val);
        void SetJsonType(JsonType type);
        void SetLong(long val);
        void SetString(string val);

        string ToJson();
        void ToJson(JsonWriter writer);
    }
}