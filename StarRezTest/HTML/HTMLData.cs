using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.HTML
{
    public class HTMLData
    {
        private HtmlDocument Document;

        public HTMLData(string htmlString)
        {
            Document = new HtmlDocument();
            Document.LoadHtml(htmlString);
        }

        public string[] Find(string xpath)
        {
            List<string> result = new();
            var nodes = Document.DocumentNode.SelectNodes(xpath);
            foreach (var node in nodes)
            {
                result.Add(ExtractFromNode(node));
            }
            return result.ToArray();
        }

        public bool TryFind(string xpath, out string[] result)
        {
            result = Find(xpath);
            return result.Length < 1;
        }

        public string FindOne(string xpath)
        {
            var node = Document.DocumentNode.SelectSingleNode(xpath);
            return ExtractFromNode(node);
        }

        public bool TryFindOne(string xpath, out string result)
        {
            result = FindOne(xpath);
            return !string.IsNullOrWhiteSpace(result);
        }

        private string ExtractFromNode(HtmlNode node)
        {
            if (node == null) { return string.Empty; }

            switch (node.Name)
            {
                case "input":
                    return node.GetAttributeValue("value", "") ?? "";
                default:
                    return node.InnerText ?? "";
            }
        }

        public T ToObject<T>() where T : class, new()
        {
            Type type = typeof(T);

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var obj = new T();

            foreach (PropertyInfo property in properties)
            {
                HTMLDOMObjectAttribute? HtmlDomAttribute = property.GetCustomAttribute<HTMLDOMObjectAttribute>();

                string targetId = HtmlDomAttribute == null ? property.Name : HtmlDomAttribute.Id;

                if (!TryFindOne(GetXPath(targetId), out string result)) { continue; }

                if (property.PropertyType == typeof(string))
                {
                    property.SetValue(obj, result);
                }
                else
                {
                    var converted = Convert.ChangeType(result, property.PropertyType);
                    property.SetValue(obj, converted);
                }
            }

            return obj;
        }

        private string GetXPath(string id)
        {
            return $"//*[@id=\"{id}\"]";
        }
    }

    public class HTMLDOMObjectAttribute: Attribute
    {
        public readonly string Id;

        public HTMLDOMObjectAttribute(string htmlElementId)
        {
            Id = htmlElementId;
        }
    }
}
