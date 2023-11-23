using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace SaveSystem
{
    public class SaveSystem
    {

        public static void SaveToStreamingAssets<T>(T data, string filePath)
        {
            SaveImpl(data, Application.streamingAssetsPath + "/" + filePath);
        }
    
        public static T LoadFromStreamingAssets<T>(string filePath)
        {
            return LoadImpl<T>(Application.streamingAssetsPath + "/" + filePath);
        }
    
        private static void SaveImpl<T>(T data, string filePath)
        {
            try
            {
                var byteArray = ToByteArray(data); 
                File.WriteAllBytes(filePath, byteArray);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static T LoadImpl<T>(string filePath)
        {
            try
            {
                var byteArray = File.ReadAllBytes(filePath);
                return FromByteArray<T>(byteArray);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    
        private static byte[] ToByteArray<T>(T obj)
        {
            if(obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using(MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        private static T FromByteArray<T>(byte[] data)
        {
            if(data == null)
                return default(T);
            BinaryFormatter bf = new BinaryFormatter();
            using(MemoryStream ms = new MemoryStream(data))
            {
                object obj = bf.Deserialize(ms);
                return (T)obj;
            }
        }
    
    
    }
}
