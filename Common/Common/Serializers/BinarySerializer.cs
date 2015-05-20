using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServiceBlocks.Common.Serializers
{
    public static class BinarySerializer<T>
    {

        public static void Serialize(T item,string fileName)
        {
            using(FileStream fs = new FileStream(fileName,FileMode.OpenOrCreate))
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.AssemblyFormat = FormatterAssemblyStyle.Simple;
                bin.Serialize(fs,item);
                fs.Close();
            }
        }
        public static T DeSerialize(string fileName)
        {
            using(FileStream fs = new FileStream(fileName,FileMode.Open))
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.AssemblyFormat = FormatterAssemblyStyle.Simple;
                T resultObject = (T)bin.Deserialize(fs);
                fs.Close();
                return resultObject;
            }
        }
        public static byte[] SerializeToByteArray(T item)
        {
            BinaryFormatter bin = new BinaryFormatter();
            bin.AssemblyFormat = FormatterAssemblyStyle.Simple;
            using(MemoryStream memStream = new MemoryStream())
            {
                bin.Serialize(memStream,item);
                memStream.Close();
                return memStream.ToArray();
            }
        }

        public static byte[] SerializeToByteArrayWithIgnoreAllBinder(T item)
        {
            BinaryFormatter bin = new BinaryFormatter();

            bin.AssemblyFormat = FormatterAssemblyStyle.Simple;

            bin.Binder = new IgnoreAssemblyVersionBinder(true, true);           
            
            using (MemoryStream memStream = new MemoryStream())
            {
                bin.Serialize(memStream, item);
                memStream.Close();
                return memStream.ToArray();
            }
        }

        public static string SerializeToBase64(T item)
        {
            return Convert.ToBase64String(SerializeToByteArray(item));
        }

        public static T DeSerializeFromByteArray(byte[] dataArray)
        {
            return DeSerializeFromByteArray(dataArray,true);
        }

        public static T DeSerializeFromByteArray(byte[] dataArray,bool ignoreVersion)
        {
            using(MemoryStream memStream = new MemoryStream(dataArray))
            {
                BinaryFormatter bin = new BinaryFormatter();

                if(ignoreVersion)
                {
                    bin.AssemblyFormat = FormatterAssemblyStyle.Simple;
                    bin.Binder = new IgnoreAssemblyVersionBinder();
                }

                T resultObject = (T)bin.Deserialize(memStream);
                memStream.Close();
                return resultObject;
            }
        }

        public static T DeSerializeFromByteArrayWithIgnoreAllBinder(byte[] dataArray)
        {
            using (MemoryStream memStream = new MemoryStream(dataArray))
            {
                BinaryFormatter bin = new BinaryFormatter();

                bin.AssemblyFormat = FormatterAssemblyStyle.Simple;

                bin.Binder = new IgnoreAssemblyVersionBinder(true, true);                

                T resultObject = (T)bin.Deserialize(memStream);
                
                memStream.Close();
                
                return resultObject;
            }
        }

        public static T DeSerializeFromBase64(string data)
        {
            return DeSerializeFromByteArray(Convert.FromBase64String(data));
        }

       public static List<T> ByteArrayToStruct<T>(byte[] bytesRecieved) where T : new()
        {
            IntPtr ptr = new IntPtr();

            int size;

            try
            {
                Type typeOfT = typeof(T);

                size = Marshal.SizeOf(typeOfT);

                ptr = Marshal.AllocHGlobal(size);

                int chunks = bytesRecieved.Length / size;

                List<T> messages = new List<T>(chunks);

                T message = new T();
                
                for (int i = 0; i < chunks; i++)
                {
                    Marshal.Copy(bytesRecieved, i * size, ptr, size);

                    message = (T)Marshal.PtrToStructure(ptr, typeOfT);

                    messages.Add(message);
                }

                return messages;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                    ptr = IntPtr.Zero;
                }
            }
        }

       public static T ObjectCopy(T copiedObject)
       {
           BinaryFormatter binaryFormatter = new BinaryFormatter();
           
           binaryFormatter.AssemblyFormat = FormatterAssemblyStyle.Simple;

           binaryFormatter.Binder = new IgnoreAssemblyVersionBinder();
           
           using (MemoryStream memStream = new MemoryStream())
           {
               binaryFormatter.Serialize(memStream, copiedObject);
               
               memStream.Position = 0;

               T newObject = (T)binaryFormatter.Deserialize(memStream);
               
               memStream.Close();

               return newObject;
           }
       }
    }
}
