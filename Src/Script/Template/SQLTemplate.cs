using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace SqlEtl.Script.Template
{
    internal static class SqlTemplate
    {
        private static readonly XElement Xelement;

        static SqlTemplate()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(@"SqlEtl.Script.Template.Script.xml");
            if (stream == null) return;
            string loadedTemplate;
            using (var reader = new StreamReader(stream))
            {
                loadedTemplate = reader.ReadToEnd();
            }
            Xelement = XElement.Parse(loadedTemplate);
        }

        /// <summary>
        ///     returns the corresponding template text
        /// </summary>
        /// <param name="templateName">name of the template</param>
        /// <returns></returns>
        internal static string Get(string templateName)
        {
            var xcData = (XCData)
                ((from node in Xelement.Elements("Script")
                  where (string)node.Attribute("Name") == templateName
                  select node.FirstNode).FirstOrDefault());
            return
                xcData?.Value;
        }
    }
}