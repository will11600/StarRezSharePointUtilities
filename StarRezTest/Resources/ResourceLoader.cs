using System.Reflection;

namespace StarRezTest.Resources
{
    public static class ResourceLoader
    {
        private static Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();

        public static readonly Dictionary<string, EmbeddedResource> Embeddded = new Dictionary<string, EmbeddedResource>();

        static ResourceLoader()
        {
            foreach (string manifestResourceName in ExecutingAssembly.GetManifestResourceNames())
            {
                Embeddded.Add(manifestResourceName, new EmbeddedResource(manifestResourceName));
            }
        }
    }
}
