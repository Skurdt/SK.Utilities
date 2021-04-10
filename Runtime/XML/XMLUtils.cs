/* MIT License

 * Copyright (c) 2020 Skurdt
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE. */

using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SK.Utilities
{
    public static class XMLUtils
    {
        public static bool Serialize<T>(string filePath, T value) where T : class
        {
            if (string.IsNullOrEmpty(filePath) || value == null)
                return false;

            filePath         = Path.GetFullPath(filePath);
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                _ = Directory.CreateDirectory(directory);

            using StringWriter stream  = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent             = true,
                OmitXmlDeclaration = true
            };
            using XmlWriter writer             = XmlWriter.Create(stream, settings);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            XmlSerializer serializer           = new XmlSerializer(typeof(T));
            serializer.Serialize(writer, value, namespaces);

            File.WriteAllText(filePath, stream.ToString());
            return true;
        }

        public static T Deserialize<T>(string filePath) where T : class
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            filePath = Path.GetFullPath(filePath);
            if (!File.Exists(filePath))
                return null;

            using FileStream stream  = new FileStream(filePath, FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return serializer.Deserialize(stream) as T;
        }

        public static async Task<T> DeserializeAsync<T>(string filePath) where T : class
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            filePath = Path.GetFullPath(filePath);
            if (!File.Exists(filePath))
                return null;

            return await Task.Run(() =>
            {
                using FileStream stream  = new FileStream(filePath, FileMode.Open);
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return serializer.Deserialize(stream) as T;
            });
        }
    }
}
