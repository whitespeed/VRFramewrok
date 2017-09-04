using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace Framework.LitJson
{
    public class JsonData : IJsonWrapper, IEquatable<JsonData>
    {
        public bool Equals(JsonData x)
        {
            if (x == null)
                return false;

            if (x.type != type)
                return false;

            switch (type)
            {
                case JsonType.None:
                    return true;

                case JsonType.Object:
                    return inst_object.Equals(x.inst_object);

                case JsonType.Array:
                    return inst_array.Equals(x.inst_array);

                case JsonType.String:
                    return inst_string.Equals(x.inst_string);

                case JsonType.Int:
                    return inst_int.Equals(x.inst_int);

                case JsonType.Float:
                    return inst_float.Equals(x.inst_float);

                case JsonType.Long:
                    return inst_long.Equals(x.inst_long);

                case JsonType.Double:
                    return inst_double.Equals(x.inst_double);

                case JsonType.Boolean:
                    return inst_boolean.Equals(x.inst_boolean);
            }

            return false;
        }

        #region IDictionary Indexer

        object IDictionary.this[object key]
        {
            get { return EnsureDictionary()[key]; }

            set
            {
                if (!(key is String))
                    throw new ArgumentException(
                        "The key has to be a string");

                var data = ToJsonData(value);

                this[(string) key] = data;
            }
        }

        #endregion

        #region IOrderedDictionary Indexer

        object IOrderedDictionary.this[int idx]
        {
            get
            {
                EnsureDictionary();
                return object_list[idx].Value;
            }

            set
            {
                EnsureDictionary();
                var data = ToJsonData(value);

                var old_entry = object_list[idx];

                inst_object[old_entry.Key] = data;

                var entry =
                    new KeyValuePair<string, JsonData>(old_entry.Key, data);

                object_list[idx] = entry;
            }
        }

        #endregion

        #region IList Indexer

        object IList.this[int index]
        {
            get { return EnsureList()[index]; }

            set
            {
                EnsureList();
                var data = ToJsonData(value);

                this[index] = data;
            }
        }

        #endregion

        #region ICollection Methods

        void ICollection.CopyTo(Array array, int index)
        {
            EnsureCollection().CopyTo(array, index);
        }

        #endregion

        #region IEnumerable Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return EnsureCollection().GetEnumerator();
        }

        #endregion

        public JsonType GetJsonType()
        {
            return type;
        }

        public void SetJsonType(JsonType type)
        {
            if (this.type == type)
                return;

            switch (type)
            {
                case JsonType.None:
                    break;

                case JsonType.Object:
                    inst_object = new Dictionary<string, JsonData>();
                    object_list = new List<KeyValuePair<string, JsonData>>();
                    break;

                case JsonType.Array:
                    inst_array = new List<JsonData>();
                    break;

                case JsonType.String:
                    inst_string = default(String);
                    break;

                case JsonType.Int:
                    inst_int = default(Int32);
                    break;

                case JsonType.Long:
                    inst_long = default(Int64);
                    break;

                case JsonType.Float:
                    inst_float = default(Single);
                    break;

                case JsonType.Double:
                    inst_double = default(Double);
                    break;

                case JsonType.Boolean:
                    inst_boolean = default(Boolean);
                    break;
            }

            this.type = type;
        }


        public int Add(object value)
        {
            var data = ToJsonData(value);

            json = null;

            return EnsureList().Add(data);
        }

        public void Clear()
        {
            if (IsObject)
            {
                ((IDictionary) this).Clear();
                return;
            }

            if (IsArray)
            {
                ((IList) this).Clear();
            }
        }

        public string ToJson()
        {
            if (json != null)
                return json;

            var sw = new StringWriter();
            var writer = new JsonWriter(sw);
            writer.Validate = false;

            WriteJson(this, writer);
            json = sw.ToString();

            return json;
        }

        public void ToJson(JsonWriter writer)
        {
            var old_validate = writer.Validate;

            writer.Validate = false;

            WriteJson(this, writer);

            writer.Validate = old_validate;
        }

        public override string ToString()
        {
            switch (type)
            {
                case JsonType.Array:
                    return ToJson();

                case JsonType.Boolean:
                    return inst_boolean.ToString();

                case JsonType.Double:
                    return inst_double.ToString();

                case JsonType.Float:
                    return inst_float.ToString();

                case JsonType.Int:
                    return inst_int.ToString();

                case JsonType.Long:
                    return inst_long.ToString();

                case JsonType.Object:
                    return ToJson();

                case JsonType.String:
                    return inst_string;
            }

            return "Uninitialized JsonData";
        }

        #region Fields

        private IList<JsonData> inst_array;
        private IDictionary<string, JsonData> inst_object;
        private bool inst_boolean;
        private double inst_double;
        private float inst_float;
        private int inst_int;
        private long inst_long;
        private string inst_string;
        private string json;
        private JsonType type;
        // Used to implement the IOrderedDictionary interface
        private IList<KeyValuePair<string, JsonData>> object_list;

        #endregion

        #region Properties

        public int Count
        {
            get { return EnsureCollection().Count; }
        }

        public bool IsArray
        {
            get { return type == JsonType.Array; }
        }

        public bool IsFloat
        {
            get { return type == JsonType.Float; }
        }

        public bool IsBoolean
        {
            get { return type == JsonType.Boolean; }
        }

        public bool IsDouble
        {
            get { return type == JsonType.Double; }
        }

        public bool IsInt
        {
            get { return type == JsonType.Int; }
        }

        public bool IsLong
        {
            get { return type == JsonType.Long; }
        }

        public bool IsObject
        {
            get { return type == JsonType.Object; }
        }

        public bool IsString
        {
            get { return type == JsonType.String; }
        }

        public ICollection<string> Keys
        {
            get
            {
                EnsureDictionary();
                return inst_object.Keys;
            }
        }

        #endregion

        #region ICollection Properties

        int ICollection.Count
        {
            get { return Count; }
        }

        bool ICollection.IsSynchronized
        {
            get { return EnsureCollection().IsSynchronized; }
        }

        object ICollection.SyncRoot
        {
            get { return EnsureCollection().SyncRoot; }
        }

        #endregion

        #region IDictionary Properties

        bool IDictionary.IsFixedSize
        {
            get { return EnsureDictionary().IsFixedSize; }
        }

        bool IDictionary.IsReadOnly
        {
            get { return EnsureDictionary().IsReadOnly; }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                EnsureDictionary();
                IList<string> keys = new List<string>();

                foreach (var entry in
                    object_list)
                {
                    keys.Add(entry.Key);
                }

                return (ICollection) keys;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                EnsureDictionary();
                IList<JsonData> values = new List<JsonData>();

                foreach (var entry in
                    object_list)
                {
                    values.Add(entry.Value);
                }

                return (ICollection) values;
            }
        }

        #endregion

        #region IJsonWrapper Properties

        bool IJsonWrapper.IsArray
        {
            get { return IsArray; }
        }

        bool IJsonWrapper.IsFloat
        {
            get { return IsFloat; }
        }

        bool IJsonWrapper.IsBoolean
        {
            get { return IsBoolean; }
        }

        bool IJsonWrapper.IsDouble
        {
            get { return IsDouble; }
        }

        bool IJsonWrapper.IsInt
        {
            get { return IsInt; }
        }

        bool IJsonWrapper.IsLong
        {
            get { return IsLong; }
        }

        bool IJsonWrapper.IsObject
        {
            get { return IsObject; }
        }

        bool IJsonWrapper.IsString
        {
            get { return IsString; }
        }

        #endregion

        #region IList Properties

        bool IList.IsFixedSize
        {
            get { return EnsureList().IsFixedSize; }
        }

        bool IList.IsReadOnly
        {
            get { return EnsureList().IsReadOnly; }
        }

        #endregion

        #region Public Indexers

        public JsonData this[string prop_name]
        {
            get
            {
                EnsureDictionary();
                return inst_object[prop_name];
            }

            set
            {
                EnsureDictionary();

                var entry =
                    new KeyValuePair<string, JsonData>(prop_name, value);

                if (inst_object.ContainsKey(prop_name))
                {
                    for (var i = 0; i < object_list.Count; i++)
                    {
                        if (object_list[i].Key == prop_name)
                        {
                            object_list[i] = entry;
                            break;
                        }
                    }
                }
                else
                    object_list.Add(entry);

                inst_object[prop_name] = value;

                json = null;
            }
        }

        public JsonData this[int index]
        {
            get
            {
                EnsureCollection();

                if (type == JsonType.Array)
                    return inst_array[index];

                return object_list[index].Value;
            }

            set
            {
                EnsureCollection();

                if (type == JsonType.Array)
                    inst_array[index] = value;
                else
                {
                    var entry = object_list[index];
                    var new_entry =
                        new KeyValuePair<string, JsonData>(entry.Key, value);

                    object_list[index] = new_entry;
                    inst_object[entry.Key] = value;
                }

                json = null;
            }
        }

        #endregion

        #region Constructors

        public JsonData()
        {
        }

        public JsonData(bool boolean)
        {
            type = JsonType.Boolean;
            inst_boolean = boolean;
        }

        public JsonData(double number)
        {
            type = JsonType.Double;
            inst_double = number;
        }

        public JsonData(float number)
        {
            type = JsonType.Float;
            inst_float = number;
        }

        public JsonData(int number)
        {
            type = JsonType.Int;
            inst_int = number;
        }

        public JsonData(long number)
        {
            type = JsonType.Long;
            inst_long = number;
        }

        public JsonData(object obj)
        {
            if (obj is Boolean)
            {
                type = JsonType.Boolean;
                inst_boolean = (bool) obj;
                return;
            }

            if (obj is Single)
            {
                type = JsonType.Float;
                inst_float = (float) obj;
                return;
            }

            if (obj is Double)
            {
                type = JsonType.Double;
                inst_double = (double) obj;
                return;
            }

            if (obj is Int32)
            {
                type = JsonType.Int;
                inst_int = (int) obj;
                return;
            }

            if (obj is Int64)
            {
                type = JsonType.Long;
                inst_long = (long) obj;
                return;
            }

            if (obj is String)
            {
                type = JsonType.String;
                inst_string = (string) obj;
                return;
            }

            throw new ArgumentException(
                "Unable to wrap the given object with JsonData");
        }

        public JsonData(string str)
        {
            type = JsonType.String;
            inst_string = str;
        }

        #endregion

        #region Implicit Conversions

        public static implicit operator JsonData(Boolean data)
        {
            return new JsonData(data);
        }

        public static implicit operator JsonData(Single data)
        {
            return new JsonData(data);
        }

        public static implicit operator JsonData(Double data)
        {
            return new JsonData(data);
        }

        public static implicit operator JsonData(Int32 data)
        {
            return new JsonData(data);
        }

        public static implicit operator JsonData(Int64 data)
        {
            return new JsonData(data);
        }

        public static implicit operator JsonData(String data)
        {
            return new JsonData(data);
        }

        #endregion

        #region Explicit Conversions

        public static explicit operator Boolean(JsonData data)
        {
            if (data.type != JsonType.Boolean)
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a  double or float");

            return data.inst_boolean;
        }

        public static explicit operator Single(JsonData data)
        {
            if (data.type != JsonType.Float && data.type != JsonType.Double)
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a float");
            return data.type == JsonType.Float ? data.inst_float : (float) data.inst_double;
        }

        public static explicit operator Double(JsonData data)
        {
            if (data.type != JsonType.Float && data.type != JsonType.Double)
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a double");
            return data.type == JsonType.Float ? data.inst_float : (float)data.inst_double;
        }

        public static explicit operator Int32(JsonData data)
        {
            if (data.type != JsonType.Int)
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold an int");

            return data.inst_int;
        }

        public static explicit operator Int64(JsonData data)
        {
            if (data.type != JsonType.Long)
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold an int");

            return data.inst_long;
        }

        public static explicit operator String(JsonData data)
        {
            if (data.type == JsonType.String)
                return data.inst_string;
            return data.ToJson();
        }

        #endregion

        #region IDictionary Methods

        void IDictionary.Add(object key, object value)
        {
            var data = ToJsonData(value);

            EnsureDictionary().Add(key, data);

            var entry =
                new KeyValuePair<string, JsonData>((string) key, data);
            object_list.Add(entry);

            json = null;
        }

        void IDictionary.Clear()
        {
            EnsureDictionary().Clear();
            object_list.Clear();
            json = null;
        }

        bool IDictionary.Contains(object key)
        {
            return EnsureDictionary().Contains(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IOrderedDictionary) this).GetEnumerator();
        }

        void IDictionary.Remove(object key)
        {
            EnsureDictionary().Remove(key);

            for (var i = 0; i < object_list.Count; i++)
            {
                if (object_list[i].Key == (string) key)
                {
                    object_list.RemoveAt(i);
                    break;
                }
            }

            json = null;
        }

        #endregion

        #region IJsonWrapper Methods

        public bool GetBoolean()
        {
            return (bool) this;
        }

        public double GetDouble()
        {
            return (double) this;
        }

        public int GetInt()
        {
            return (int) this;
        }

        public long GetLong()
        {
            return (long) this;
        }

        public string GetString()
        {
            return (string) this;
        }

        public float GetFloat()
        {
            return (float) this;
        }

        void IJsonWrapper.SetFloat(float val)
        {
            type = JsonType.Float;
            inst_float = val;
            json = null;
        }

        void IJsonWrapper.SetBoolean(bool val)
        {
            type = JsonType.Boolean;
            inst_boolean = val;
            json = null;
        }

        void IJsonWrapper.SetDouble(double val)
        {
            type = JsonType.Double;
            inst_double = val;
            json = null;
        }


        void IJsonWrapper.SetInt(int val)
        {
            type = JsonType.Int;
            inst_int = val;
            json = null;
        }

        void IJsonWrapper.SetLong(long val)
        {
            type = JsonType.Long;
            inst_long = val;
            json = null;
        }

        void IJsonWrapper.SetString(string val)
        {
            type = JsonType.String;
            inst_string = val;
            json = null;
        }

        string IJsonWrapper.ToJson()
        {
            return ToJson();
        }

        void IJsonWrapper.ToJson(JsonWriter writer)
        {
            ToJson(writer);
        }

        #endregion

        #region IList Methods

        int IList.Add(object value)
        {
            return Add(value);
        }

        void IList.Clear()
        {
            EnsureList().Clear();
            json = null;
        }

        bool IList.Contains(object value)
        {
            return EnsureList().Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return EnsureList().IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            EnsureList().Insert(index, value);
            json = null;
        }

        void IList.Remove(object value)
        {
            EnsureList().Remove(value);
            json = null;
        }

        void IList.RemoveAt(int index)
        {
            EnsureList().RemoveAt(index);
            json = null;
        }

        #endregion

        #region IOrderedDictionary Methods

        IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
        {
            EnsureDictionary();

            return new OrderedDictionaryEnumerator(
                object_list.GetEnumerator());
        }

        void IOrderedDictionary.Insert(int idx, object key, object value)
        {
            var property = (string) key;
            var data = ToJsonData(value);

            this[property] = data;

            var entry =
                new KeyValuePair<string, JsonData>(property, data);

            object_list.Insert(idx, entry);
        }

        void IOrderedDictionary.RemoveAt(int idx)
        {
            EnsureDictionary();

            inst_object.Remove(object_list[idx].Key);
            object_list.RemoveAt(idx);
        }

        #endregion

        #region Private Methods

        private ICollection EnsureCollection()
        {
            if (type == JsonType.Array)
                return (ICollection) inst_array;

            if (type == JsonType.Object)
                return (ICollection) inst_object;

            throw new InvalidOperationException(
                "The JsonData instance has to be initialized first");
        }

        private IDictionary EnsureDictionary()
        {
            if (type == JsonType.Object)
                return (IDictionary) inst_object;

            if (type != JsonType.None)
                throw new InvalidOperationException(
                    "Instance of JsonData is not a dictionary");

            type = JsonType.Object;
            inst_object = new Dictionary<string, JsonData>();
            object_list = new List<KeyValuePair<string, JsonData>>();

            return (IDictionary) inst_object;
        }

        private IList EnsureList()
        {
            if (type == JsonType.Array)
                return (IList) inst_array;

            if (type != JsonType.None)
                throw new InvalidOperationException(
                    "Instance of JsonData is not a list");

            type = JsonType.Array;
            inst_array = new List<JsonData>();

            return (IList) inst_array;
        }

        private JsonData ToJsonData(object obj)
        {
            if (obj == null)
                return null;

            if (obj is JsonData)
                return (JsonData) obj;

            return new JsonData(obj);
        }


        private static void WriteJson(IJsonWrapper wrapper, JsonWriter writer)
        {
            if (wrapper == null && !JsonWriter.SkipNullMember)
            {
                writer.Write(null);
                return;
            }

            if (wrapper.IsString)
            {
                writer.Write(wrapper.GetString());
                return;
            }

            if (wrapper.IsBoolean)
            {
                writer.Write(wrapper.GetBoolean());
                return;
            }

            if (wrapper.IsDouble)
            {
                writer.Write(wrapper.GetDouble());
                return;
            }

            if (wrapper.IsInt)
            {
                writer.Write(wrapper.GetInt());
                return;
            }
            if (wrapper.IsFloat)
            {
                writer.Write(wrapper.GetFloat());
                return;
            }
            if (wrapper.IsLong)
            {
                writer.Write(wrapper.GetLong());
                return;
            }

            if (wrapper.IsArray)
            {
                writer.WriteArrayStart();
                foreach (var elem in (IList) wrapper)
                {
                    WriteJson((JsonData) elem, writer);
                }
                writer.WriteArrayEnd();
                return;
            }

            if (wrapper.IsObject)
            {
                if (JsonWriter.SkipNullMember && wrapper.Count <= 0)
                    return;

                writer.WriteObjectStart();

                foreach (DictionaryEntry entry in (IDictionary) wrapper)
                {
                    if (JsonWriter.SkipNullMember)
                    {
                        var type = ((JsonData) entry.Value).GetJsonType();
                        if (type == JsonType.None)
                            continue;
                        if (type == JsonType.Array && ((IList) entry.Value).Count <= 0)
                            continue;
                    }
                    writer.WritePropertyName((string) entry.Key);
                    WriteJson((JsonData) entry.Value, writer);
                }
                writer.WriteObjectEnd();

                return;
            }

            if (wrapper.GetJsonType() == JsonType.None)
            {
                writer.WriteObjectStart();
                writer.WriteObjectEnd();
            }
        }

        #endregion
    }


    internal class OrderedDictionaryEnumerator : IDictionaryEnumerator
    {
        private readonly IEnumerator<KeyValuePair<string, JsonData>> list_enumerator;


        public OrderedDictionaryEnumerator(
            IEnumerator<KeyValuePair<string, JsonData>> enumerator)
        {
            list_enumerator = enumerator;
        }


        public object Current
        {
            get { return Entry; }
        }

        public DictionaryEntry Entry
        {
            get
            {
                var curr = list_enumerator.Current;
                return new DictionaryEntry(curr.Key, curr.Value);
            }
        }

        public object Key
        {
            get { return list_enumerator.Current.Key; }
        }

        public object Value
        {
            get { return list_enumerator.Current.Value; }
        }


        public bool MoveNext()
        {
            return list_enumerator.MoveNext();
        }

        public void Reset()
        {
            list_enumerator.Reset();
        }
    }
}