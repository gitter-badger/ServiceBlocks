using System;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServiceBlocks.Common.Serializers
{
    public static class NonGenericBinarySerializer
    {
        public static byte[] SerializeToByteArray(object item)
        {
            BinaryFormatter bin = new BinaryFormatter();
            using(MemoryStream memStream = new MemoryStream())
            {
                bin.AssemblyFormat = FormatterAssemblyStyle.Simple;
                bin.Serialize(memStream,item);
                memStream.Close();
                return memStream.ToArray();
            }
        }

        public static string SerializeToBase64(object item)
        {
            return Convert.ToBase64String(SerializeToByteArray(item));
        }

        public static object DeSerializeFromByteArray(byte[] dataArray)
        {
            using(MemoryStream memStream = new MemoryStream(dataArray))
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.AssemblyFormat = FormatterAssemblyStyle.Simple;
                object resultObject = bin.Deserialize(memStream);
                memStream.Close();
                return resultObject;
            }
        }

        public static object DeSerializeFromBase64(string data)
        {
            return DeSerializeFromByteArray(Convert.FromBase64String(data));
        }
    }
}
