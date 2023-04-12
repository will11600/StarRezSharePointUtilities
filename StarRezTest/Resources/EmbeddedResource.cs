using LitJson;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;


namespace StarRezTest.Resources
{
    public class EmbeddedResource
    {
        public readonly string Name;

        public Stream? Stream => Assembly.GetExecutingAssembly().GetManifestResourceStream(Name);

        public EmbeddedResource(string name) => Name = name;

        public async Task<T?> ReadAllAsync<T>() where T : class, IConvertible
        {
            T? obj = null;
            if (Stream == null) { return obj; }
            using (StreamReader reader = new StreamReader(Stream))
            {
                string data = await reader.ReadToEndAsync();
                obj = Convert.ChangeType(data, typeof(T)) as T;
            }
            return obj;
        }

        public async Task<string> ReadAllAsync()
        {
            string data = string.Empty;
            if (Stream == null) { return data; }
            using (StreamReader reader = new StreamReader(Stream))
                data = await reader.ReadToEndAsync();
            return data;
        }

        public async Task<JsonData?> ReadJsonAsync()
        {
            JsonData? jsonData = null;
            if (Stream == null) { return jsonData; }
            using (StreamReader reader = new StreamReader(Stream))
            {
                string data = await reader.ReadToEndAsync();
                jsonData = JsonMapper.ToObject(data);
            }
            return jsonData;
        }

        public T? ReadAll<T>() where T : class, IConvertible => ReadAllAsync<T>().Result;

        public string ReadAll() => ReadAllAsync().Result;

        public JsonData? ReadJson() => ReadJsonAsync().Result;
    }
}
