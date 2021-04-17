using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SK.Utilities
{
    public struct IniValue
    {
        public static IniValue Default { get; } = new IniValue();

        public string Value;

        public IniValue(object value)
        {
            if (value is IFormattable formattable)
                Value = formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture);
            else
                Value = value?.ToString();
        }

        public IniValue(string value) => Value = value;

        public bool ToBool(bool valueIfInvalid = false)
        {
            if (TryConvertBool(out bool res))
                return res;
            return valueIfInvalid;
        }

        public bool TryConvertBool(out bool result)
        {
            if (Value == null)
            {
                result = default;
                return false;
            }

            string boolStr = Value.Trim().ToLowerInvariant();
            if (boolStr == "true")
            {
                result = true;
                return true;
            }
            else if (boolStr == "false")
            {
                result = false;
                return true;
            }
            result = default;
            return false;
        }

        public int ToInt(int valueIfInvalid = 0)
        {
            if (TryConvertInt(out int res))
                return res;
            return valueIfInvalid;
        }

        public bool TryConvertInt(out int result)
        {
            if (Value == null)
            {
                result = default;
                return false;
            }
            return TryParseInt(Value.Trim(), out result);
        }

        public double ToDouble(double valueIfInvalid = 0)
        {
            if (TryConvertDouble(out double res))
                return res;
            return valueIfInvalid;
        }

        public bool TryConvertDouble(out double result)
        {
            if (Value == null)
            {
                result = default;
                return false;
            }
            return TryParseDouble(Value.Trim(), out result);
        }

        public string GetString() => GetString(true, false);

        public string GetString(bool preserveWhitespace) => GetString(true, preserveWhitespace);

        public string GetString(bool allowOuterQuotes, bool preserveWhitespace)
        {
            if (Value == null)
                return "";

            string trimmed = Value.Trim();
            if (allowOuterQuotes && trimmed.Length >= 2 && trimmed[0] == '"' && trimmed[trimmed.Length - 1] == '"')
            {
                string inner = trimmed.Substring(1, trimmed.Length - 2);
                return preserveWhitespace ? inner : inner.Trim();
            }
            return preserveWhitespace ? Value : Value.Trim();
        }

        public override string ToString() => Value;

        public static implicit operator IniValue(byte o) => new IniValue(o);

        public static implicit operator IniValue(short o) => new IniValue(o);

        public static implicit operator IniValue(int o) => new IniValue(o);

        public static implicit operator IniValue(sbyte o) => new IniValue(o);

        public static implicit operator IniValue(ushort o) => new IniValue(o);

        public static implicit operator IniValue(uint o) => new IniValue(o);

        public static implicit operator IniValue(float o) => new IniValue(o);

        public static implicit operator IniValue(double o) => new IniValue(o);

        public static implicit operator IniValue(bool o) => new IniValue(o);

        public static implicit operator IniValue(string o) => new IniValue(o);

        private static bool TryParseInt(string text, out int value)
        {
            if (int.TryParse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int res))
            {
                value = res;
                return true;
            }
            value = 0;
            return false;
        }

        private static bool TryParseDouble(string text, out double value)
        {
            if (double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double res))
            {
                value = res;
                return true;
            }
            value = double.NaN;
            return false;
        }
    }

    public class IniSection : IEnumerable<KeyValuePair<string, IniValue>>, IDictionary<string, IniValue>
    {
        private readonly Dictionary<string, IniValue> _values;

        #region Ordered
        private List<string> _orderedKeys;

        public int IndexOf(string key)
        {
            if (!Ordered)
                throw new InvalidOperationException("Cannot call IndexOf(string) on IniSection: section was not ordered.");
            return IndexOf(key, 0, _orderedKeys.Count);
        }

        public int IndexOf(string key, int index)
        {
            if (!Ordered)
                throw new InvalidOperationException("Cannot call IndexOf(string, int) on IniSection: section was not ordered.");
            return IndexOf(key, index, _orderedKeys.Count - index);
        }

        public int IndexOf(string key, int index, int count)
        {
            if (!Ordered)
                throw new InvalidOperationException("Cannot call IndexOf(string, int, int) on IniSection: section was not ordered.");
            if (index < 0 || index > _orderedKeys.Count)
                throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than zero." + Environment.NewLine + "Parameter name: count");
            if (index + count > _orderedKeys.Count)
                throw new ArgumentException("Index and count were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
            int end = index + count;
            for (int i = index; i < end; i++)
                if (Comparer.Equals(_orderedKeys[i], key))
                    return i;
            return -1;
        }

        public int LastIndexOf(string key)
        {
            if (!Ordered)
                throw new InvalidOperationException("Cannot call LastIndexOf(string) on IniSection: section was not ordered.");
            return LastIndexOf(key, 0, _orderedKeys.Count);
        }

        public int LastIndexOf(string key, int index)
        {
            if (!Ordered)
                throw new InvalidOperationException("Cannot call LastIndexOf(string, int) on IniSection: section was not ordered.");
            return LastIndexOf(key, index, _orderedKeys.Count - index);
        }

        public int LastIndexOf(string key, int index, int count)
        {
            if (!Ordered)
                throw new InvalidOperationException("Cannot call LastIndexOf(string, int, int) on IniSection: section was not ordered.");
            if (index < 0 || index > _orderedKeys.Count)
                throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than zero." + Environment.NewLine + "Parameter name: count");
            if (index + count > _orderedKeys.Count)
                throw new ArgumentException("Index and count were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
            int end = index + count;
            for (int i = end - 1; i >= index; i--)
                if (Comparer.Equals(_orderedKeys[i], key))
                    return i;
            return -1;
        }

        public void Insert(int index, string key, IniValue value)
        {
            if (!Ordered)
                throw new InvalidOperationException("Cannot call Insert(int, string, IniValue) on IniSection: section was not ordered.");
            if (index < 0 || index > _orderedKeys.Count)
                throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");
            _values.Add(key, value);
            _orderedKeys.Insert(index, key);
        }

        public void InsertRange(int index, IEnumerable<KeyValuePair<string, IniValue>> collection)
        {
            if (!Ordered)
                throw new InvalidOperationException("Cannot call InsertRange(int, IEnumerable<KeyValuePair<string, IniValue>>) on IniSection: section was not ordered.");
            if (collection == null)
                throw new ArgumentNullException("Value cannot be null." + Environment.NewLine + "Parameter name: collection");
            if (index < 0 || index > _orderedKeys.Count)
                throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");
            foreach (KeyValuePair<string, IniValue> kvp in collection)
            {
                Insert(index, kvp.Key, kvp.Value);
                ++index;
            }
        }

        public void RemoveAt(int index)
        {
            if (!Ordered)
                throw new InvalidOperationException("Cannot call RemoveAt(int) on IniSection: section was not ordered.");
            if (index < 0 || index > _orderedKeys.Count)
                throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");
            string key = _orderedKeys[index];
            _orderedKeys.RemoveAt(index);
            _ = _values.Remove(key);
        }

        public void RemoveRange(int index, int count)
        {
            if (!Ordered)
                throw new InvalidOperationException("Cannot call RemoveRange(int, int) on IniSection: section was not ordered.");
            if (index < 0 || index > _orderedKeys.Count)
                throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than zero." + Environment.NewLine + "Parameter name: count");
            if (index + count > _orderedKeys.Count)
                throw new ArgumentException("Index and count were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
            for (int i = 0; i < count; i++)
                RemoveAt(index);
        }

        public void Reverse()
        {
            if (!Ordered)
                throw new InvalidOperationException("Cannot call Reverse() on IniSection: section was not ordered.");
            _orderedKeys.Reverse();
        }

        public void Reverse(int index, int count)
        {
            if (!Ordered)
                throw new InvalidOperationException("Cannot call Reverse(int, int) on IniSection: section was not ordered.");
            if (index < 0 || index > _orderedKeys.Count)
                throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");
            if (count < 0)
                throw new IndexOutOfRangeException("Count cannot be less than zero." + Environment.NewLine + "Parameter name: count");
            if (index + count > _orderedKeys.Count)
                throw new ArgumentException("Index and count were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
            _orderedKeys.Reverse(index, count);
        }

        public ICollection<IniValue> GetOrderedValues()
        {
            if (!Ordered)
                throw new InvalidOperationException("Cannot call GetOrderedValues() on IniSection: section was not ordered.");

            List<IniValue> list = new List<IniValue>();
            for (int i = 0; i < _orderedKeys.Count; i++)
                list.Add(_values[_orderedKeys[i]]);
            return list;
        }

        public IniValue this[int index]
        {
            get
            {
                if (!Ordered)
                    throw new InvalidOperationException("Cannot index IniSection using integer key: section was not ordered.");
                if (index < 0 || index >= _orderedKeys.Count)
                    throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");
                return _values[_orderedKeys[index]];
            }
            set
            {
                if (!Ordered)
                    throw new InvalidOperationException("Cannot index IniSection using integer key: section was not ordered.");
                if (index < 0 || index >= _orderedKeys.Count)
                    throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");
                string key = _orderedKeys[index];
                _values[key] = value;
            }
        }

        public bool Ordered
        {
            get => _orderedKeys != null;
            set
            {
                if (Ordered != value)
                    _orderedKeys = value ? new List<string>(_values.Keys) : null;
            }
        }
        #endregion

        public IniSection()
        : this(IniFile.DefaultComparer)
        {
        }

        public IniSection(IEqualityComparer<string> stringComparer) => _values = new Dictionary<string, IniValue>(stringComparer);

        public IniSection(Dictionary<string, IniValue> values)
        : this(values, IniFile.DefaultComparer)
        {
        }

        public IniSection(Dictionary<string, IniValue> values, IEqualityComparer<string> stringComparer)
            => _values = new Dictionary<string, IniValue>(values, stringComparer);

        public IniSection(IniSection values)
        : this(values, IniFile.DefaultComparer)
        {
        }

        public IniSection(IniSection values, IEqualityComparer<string> stringComparer)
            => _values = new Dictionary<string, IniValue>(values._values, stringComparer);

        public void Add(string key, IniValue value)
        {
            _values.Add(key, value);
            if (Ordered)
                _orderedKeys.Add(key);
        }

        public bool ContainsKey(string key) => _values.ContainsKey(key);

        /// <summary>
        /// Returns this IniSection's collection of keys. If the IniSection is ordered, the keys will be returned in order.
        /// </summary>
        public ICollection<string> Keys => Ordered ? (ICollection<string>)_orderedKeys : _values.Keys;

        public bool Remove(string key)
        {
            bool ret = _values.Remove(key);
            if (Ordered && ret)
            {
                for (int i = 0; i < _orderedKeys.Count; i++)
                {
                    if (Comparer.Equals(_orderedKeys[i], key))
                    {
                        _orderedKeys.RemoveAt(i);
                        break;
                    }
                }
            }
            return ret;
        }

        public bool TryGetValue(string key, out IniValue value) => _values.TryGetValue(key, out value);

        /// <summary>
        /// Returns the values in this IniSection. These values are always out of order. To get ordered values from an IniSection call GetOrderedValues instead.
        /// </summary>
        public ICollection<IniValue> Values => _values.Values;

        void ICollection<KeyValuePair<string, IniValue>>.Add(KeyValuePair<string, IniValue> item)
        {
            ((IDictionary<string, IniValue>)_values).Add(item);
            if (Ordered)
                _orderedKeys.Add(item.Key);
        }

        public void Clear()
        {
            _values.Clear();
            if (Ordered)
                _orderedKeys.Clear();
        }

        bool ICollection<KeyValuePair<string, IniValue>>.Contains(KeyValuePair<string, IniValue> item)
            => ((IDictionary<string, IniValue>)_values).Contains(item);

        void ICollection<KeyValuePair<string, IniValue>>.CopyTo(KeyValuePair<string, IniValue>[] array, int arrayIndex)
            => ((IDictionary<string, IniValue>)_values).CopyTo(array, arrayIndex);

        public int Count => _values.Count;

        bool ICollection<KeyValuePair<string, IniValue>>.IsReadOnly => ((IDictionary<string, IniValue>)_values).IsReadOnly;

        bool ICollection<KeyValuePair<string, IniValue>>.Remove(KeyValuePair<string, IniValue> item)
        {
            bool ret = ((IDictionary<string, IniValue>)_values).Remove(item);
            if (Ordered && ret)
            {
                for (int i = 0; i < _orderedKeys.Count; i++)
                {
                    if (Comparer.Equals(_orderedKeys[i], item.Key))
                    {
                        _orderedKeys.RemoveAt(i);
                        break;
                    }
                }
            }
            return ret;
        }

        public IEnumerator<KeyValuePair<string, IniValue>> GetEnumerator()
        {
            if (Ordered)
                return GetOrderedEnumerator();
            else
                return _values.GetEnumerator();
        }

        private IEnumerator<KeyValuePair<string, IniValue>> GetOrderedEnumerator()
        {
            for (int i = 0; i < _orderedKeys.Count; i++)
                yield return new KeyValuePair<string, IniValue>(_orderedKeys[i], _values[_orderedKeys[i]]);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public IEqualityComparer<string> Comparer => _values.Comparer;

        public IniValue this[string name]
        {
            get
            {
                if (_values.TryGetValue(name, out IniValue val))
                    return val;
                return IniValue.Default;
            }
            set
            {
                if (Ordered && !_orderedKeys.Contains(name, Comparer))
                    _orderedKeys.Add(name);
                _values[name] = value;
            }
        }

        public static implicit operator IniSection(Dictionary<string, IniValue> dict) => new IniSection(dict);

        public static explicit operator Dictionary<string, IniValue>(IniSection section) => section._values;
    }

    public class IniFile : IEnumerable<KeyValuePair<string, IniSection>>, IDictionary<string, IniSection>
    {
        public IEqualityComparer<string> StringComparer;
        public bool SaveEmptySections;

        private readonly Dictionary<string, IniSection> _sections;

        public IniFile()
        : this(DefaultComparer)
        {
        }

        public IniFile(IEqualityComparer<string> stringComparer)
        {
            StringComparer = stringComparer;
            _sections = new Dictionary<string, IniSection>(StringComparer);
        }

        public void Save(string path, FileMode mode = FileMode.Create)
        {
            using FileStream stream = new FileStream(path, mode, FileAccess.Write);
            Save(stream);
        }

        public void Save(Stream stream)
        {
            using StreamWriter writer = new StreamWriter(stream);
            Save(writer);
        }

        public void Save(StreamWriter writer)
        {
            foreach (KeyValuePair<string, IniSection> section in _sections)
            {
                if (section.Value.Count > 0 || SaveEmptySections)
                {
                    writer.WriteLine(string.Format("[{0}]", section.Key.Trim()));
                    foreach (KeyValuePair<string, IniValue> kvp in section.Value)
                        writer.WriteLine(string.Format("{0}={1}", kvp.Key, kvp.Value));
                    writer.WriteLine("");
                }
            }
        }

        public void Load(string path, bool ordered = false)
        {
            using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            Load(stream, ordered);
        }

        public void Load(Stream stream, bool ordered = false)
        {
            using StreamReader reader = new StreamReader(stream);
            Load(reader, ordered);
        }

        public void Load(StreamReader reader, bool ordered = false)
        {
            IniSection section = null;

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                if (line != null)
                {
                    string trimStart = line.TrimStart();

                    if (trimStart.Length > 0)
                    {
                        if (trimStart[0] == '[')
                        {
                            int sectionEnd = trimStart.IndexOf(']');
                            if (sectionEnd > 0)
                            {
                                string sectionName = trimStart.Substring(1, sectionEnd - 1).Trim();
                                section = new IniSection(StringComparer) { Ordered = ordered };
                                _sections[sectionName] = section;
                            }
                        }
                        else if (section != null && trimStart[0] != ';')
                        {
                            if (LoadValue(line, out string key, out IniValue val))
                                section[key] = val;
                        }
                    }
                }
            }
        }

        private bool LoadValue(string line, out string key, out IniValue val)
        {
            int assignIndex = line.IndexOf('=');

            string value;
            if (assignIndex <= 0)
            {
                key   = line.Trim();
                value = string.Empty;
            }
            else
            {
                key = line.Substring(0, assignIndex).Trim();
                value = line.Substring(assignIndex + 1);
            }
            val = new IniValue(value);
            return true;
        }

        public bool ContainsSection(string section) => _sections.ContainsKey(section);

        public bool TryGetSection(string section, out IniSection result) => _sections.TryGetValue(section, out result);

        bool IDictionary<string, IniSection>.TryGetValue(string key, out IniSection value) => TryGetSection(key, out value);

        public bool Remove(string section) => _sections.Remove(section);

        public IniSection Add(string section, Dictionary<string, IniValue> values, bool ordered = false)
            => Add(section, new IniSection(values, StringComparer) { Ordered = ordered });

        public IniSection Add(string section, IniSection value)
        {
            if (value.Comparer != StringComparer)
                value = new IniSection(value, StringComparer);
            _sections.Add(section, value);
            return value;
        }

        public IniSection Add(string section, bool ordered = false)
        {
            IniSection value = new IniSection(StringComparer) { Ordered = ordered };
            _sections.Add(section, value);
            return value;
        }

        void IDictionary<string, IniSection>.Add(string key, IniSection value) => Add(key, value);

        bool IDictionary<string, IniSection>.ContainsKey(string key) => ContainsSection(key);

        public ICollection<string> Keys => _sections.Keys;

        public ICollection<IniSection> Values => _sections.Values;

        void ICollection<KeyValuePair<string, IniSection>>.Add(KeyValuePair<string, IniSection> item)
            => ((IDictionary<string, IniSection>)_sections).Add(item);

        public void Clear() => _sections.Clear();

        bool ICollection<KeyValuePair<string, IniSection>>.Contains(KeyValuePair<string, IniSection> item)
            => ((IDictionary<string, IniSection>)_sections).Contains(item);

        void ICollection<KeyValuePair<string, IniSection>>.CopyTo(KeyValuePair<string, IniSection>[] array, int arrayIndex)
            => ((IDictionary<string, IniSection>)_sections).CopyTo(array, arrayIndex);

        public int Count => _sections.Count;

        bool ICollection<KeyValuePair<string, IniSection>>.IsReadOnly => ((IDictionary<string, IniSection>)_sections).IsReadOnly;

        bool ICollection<KeyValuePair<string, IniSection>>.Remove(KeyValuePair<string, IniSection> item)
            => ((IDictionary<string, IniSection>)_sections).Remove(item);

        public IEnumerator<KeyValuePair<string, IniSection>> GetEnumerator() => _sections.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public IniSection this[string section]
        {
            get
            {
                if (_sections.TryGetValue(section, out IniSection s))
                    return s;
                s = new IniSection(StringComparer);
                _sections[section] = s;
                return s;
            }
            set
            {
                IniSection v = value;
                if (v.Comparer != StringComparer)
                    v = new IniSection(v, StringComparer);
                _sections[section] = v;
            }
        }

        public string GetContents()
        {
            using MemoryStream stream = new MemoryStream();
            Save(stream);
            stream.Flush();
            StringBuilder builder = new StringBuilder(Encoding.UTF8.GetString(stream.ToArray()));
            return builder.ToString();
        }

        public static IEqualityComparer<string> DefaultComparer = new CaseInsensitiveStringComparer();

        private class CaseInsensitiveStringComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y) => string.Compare(x, y, true) == 0;

            public int GetHashCode(string obj) => obj.ToLowerInvariant().GetHashCode();
        }
    }
}
