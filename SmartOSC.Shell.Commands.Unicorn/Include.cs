using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using System.Xml;
using System.Xml.Schema;
using System.Configuration;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Sitecore.Extensions;
using Sitecore.Text;
using Sitecore.Xml.Xsl;

namespace SmartOSC.Shell.Commands.Unicorn
{
    /*
     * Class execute Include a Item
     */
    public class Include : Command
    {
        // Custom XML that contains your temporary config  
        private static readonly string RootPath =
            HttpContext.Current.Server.MapPath(@"~\App_Config\Include\Unicorn\Unicorn.SmartOSC.Config.Default");

        // Main function be call when has Include command
        public override void Execute(CommandContext context)
        {
            /*
             * Declare necessary variable
             */
            string path = context.Items[0].Paths.FullPath;
            string database = context.Items[0].Database.ToString();
            string name = context.Items[0].Name;

            // Call function
            ItemInclude(database, path, name);
        }

        /*
         * Function include item
         * @databse : database name
         * @path : path name
         * @name : item name
         */
        public void ItemInclude(string database, string path, string name)
        {
            // init XmlDocument
            XmlDocument doc = new XmlDocument();

            // load Physical Xml to doc
            doc.Load(RootPath);

            // Create Atribute for a node
            XmlAttribute attributeDatabase = doc.CreateAttribute("database");
            attributeDatabase.Value = database.ToString();

            XmlAttribute attributePath = doc.CreateAttribute("path");
            attributePath.Value = path.ToString();

            XmlElement elem = doc.CreateElement("include");

            elem.SetAttribute("name", name);
            elem.SetAttribute("path", path);
            elem.SetAttribute("database", database);

            // get predicate Node
            XmlNode predicate = doc.SelectSingleNode("/configuration/sitecore/unicorn/configurations/configuration/predicate");

            // If predicate node not null
            if (predicate != null)

                // If predicate node have child node
                if (predicate.HasChildNodes)
                {
                    // foreach all child in predicate node
                    foreach (XmlNode predicateChildNode in predicate.ChildNodes)
                    {
                        // Check Is Parrent of new Node have been to include 
                        if (predicateChildNode.Attributes != null &&
                            elem.Attributes["path"].Value.StartsWith(predicateChildNode.Attributes["path"].Value))
                        {
                            // Check path is exist in xml?
                            if (!ContextItemStatus(predicate, elem.Attributes["path"].Value) &&
                                !ContextItemStatus(predicateChildNode, elem.Attributes["path"].Value))
                            {
                                // Append Elem to predicate node
                                predicateChildNode.AppendChild(elem);
                            }
                            else
                            {
                                SheerResponse.Alert(String.Format("This Item has included or excluded."));
                                return;
                            }
                        }
                    }
                }
                else
                {
                    // Append Elem to predicate node (this case is predicate node is empty)
                    predicate.AppendChild(elem);
                }
            // Save xml. If not, the xml will not change
            doc.Save(RootPath);
        }

        /*
         * Check item has been included or excluded ?
         * @parentNode : Node will be add item (@path)
         * @path : path of a item want to check
         */
        public static bool ContextItemStatus(XmlNode parentNode, string path)
        {
            // check parrent have child
            if (parentNode.HasChildNodes)
            {
                XmlNodeList nodeList = parentNode.ChildNodes;
                foreach (XmlNode node in nodeList)
                {
                    // check child of parrent is item (@path)
                    if (node.Attributes != null && node.Attributes["path"].Value.Equals(path))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    /*
     * Class execute Exclude Command
     */
    public class Exclude : Command
    {
        private static readonly string RootPath =
            HttpContext.Current.Server.MapPath(@"~\App_Config\Include\Unicorn\Unicorn.SmartOSC.Config.Default");

        public override void Execute(CommandContext context)
        {
            string path = context.Items[0].Paths.FullPath;
            string database = context.Items[0].Database.ToString();
            string name = context.Items[0].Name;
            ItemExclude(path);
        }

        public void ItemExclude(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(RootPath);
            XmlAttribute attributePath = doc.CreateAttribute("path");
            attributePath.Value = path.ToString();

            XmlElement elem = doc.CreateElement("exclude");
            elem.SetAttribute("path", path);

            XmlNode predicate = doc.SelectSingleNode("/configuration/sitecore/unicorn/configurations/configuration/predicate");
            bool flag = false;

            if (predicate != null)
                foreach (XmlNode predicateChildNode in predicate.ChildNodes)
                {
                    // Check Is Parrent of new Node have been to include 
                    if (predicateChildNode.Attributes != null &&
                        elem.Attributes["path"].Value.StartsWith(predicateChildNode.Attributes["path"].Value))
                    {
                        // Check path is exist in xml?
                        if (!ContextItemStatus(predicate, elem.Attributes["path"].Value) &&
                            !ContextItemStatus(predicateChildNode, elem.Attributes["path"].Value))
                        {
                            predicateChildNode.AppendChild(elem);

                        }
                        else
                        {
                            SheerResponse.Alert(String.Format("This Item has included or excluded."));
                            return;
                        }

                        // Here set the Item has been include
                        flag = true;
                    }
                }

            if (!flag)
            {
                SheerResponse.Alert(String.Format("This Parent of Item not be included."));
            }
            else
            {
                doc.Save(RootPath);
            }
        }
        public static bool ContextItemStatus(XmlNode parentNode, string path)
        {
            if (parentNode.HasChildNodes)
            {
                XmlNodeList nodeList = parentNode.ChildNodes;
                foreach (XmlNode node in nodeList)
                {
                    if (node.Attributes != null && node.Attributes["path"].Value.Equals(path))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class RemoveInclude : Command
    {
        private static readonly string RootPath =
            HttpContext.Current.Server.MapPath(@"~\App_Config\Include\Unicorn\Unicorn.SmartOSC.Config.Default");

        public override void Execute(CommandContext context)
        {
            string path = context.Items[0].Paths.FullPath;
            string database = context.Items[0].Database.ToString();
            string name = context.Items[0].Name;
            ItemRemoveInclude(path);
        }

        public void ItemRemoveInclude(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(RootPath);

            XmlNode predicate = doc.SelectSingleNode("/configuration/sitecore/unicorn/configurations/configuration/predicate");
            XmlNodeList nodes = doc.SelectNodes($"//include[@path='{path}']");
            foreach (XmlNode node in nodes)
            {
                try
                {
                    predicate?.RemoveChild(node);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            }
            doc.Save(RootPath);

        }
    }

    public class ShowConfig : Command
    {
        public override void Execute(CommandContext context)
        {
            SheerResponse.Alert(HttpContext.Current.Server.MapPath(@"~\App_Config\Include\Unicorn\Unicorn.SmartOSC.Config.Default"));
        }
    }


    public class ExcludeByPattern : Command
    {
        private static readonly string RootPath =
            HttpContext.Current.Server.MapPath(@"~\App_Config\Include\Unicorn\Unicorn.SmartOSC.Config.Default");

        public override void Execute(CommandContext context)
        {
            string path = context.Items[0].Paths.FullPath;
            string database = context.Items[0].Database.ToString();
            string name = context.Items[0].Name;
            ItemExclude("Your Pattern");
        }

        public void ItemExclude(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(RootPath);
            XmlAttribute attributePath = doc.CreateAttribute("namePattern");
            attributePath.Value = path.ToString();

            XmlElement elem = doc.CreateElement("exclude");
            elem.SetAttribute("namePattern", path);

            XmlNode predicate = doc.SelectSingleNode("/configuration/sitecore/unicorn/configurations/configuration/predicate");

            if (predicate != null)
                foreach (XmlNode predicateChildNode in predicate.ChildNodes)
                {
                    if (!ContextItemStatus(predicateChildNode))
                    {
                        predicateChildNode.AppendChild(elem);
                    }
                    else
                    {
                        SheerResponse.Alert("Item is Exist Pattern");
                    }
                }
            doc.Save(RootPath);
        }
        public static bool ContextItemStatus(XmlNode parentNode)
        {
            if (parentNode.HasChildNodes)
            {
                XmlNodeList nodeList = parentNode.ChildNodes;
                foreach (XmlNode node in nodeList)
                {
                    if (node.Attributes?["namePattern"] != null )
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    /*
     * Class execute ExcludeByTemplateID
     */
    public class ExcludeByTemplateID : Command
    {
        static string rootPath =
            HttpContext.Current.Server.MapPath(@"~\App_Config\Include\Unicorn\Unicorn.SmartOSC.Config.Default");

        public override void Execute(CommandContext context)
        {
            string path = context.Items[0].Paths.FullPath;
            string database = context.Items[0].Database.ToString();
            string name = context.Items[0].Name;
            ItemExclude("Your Template ID");
        }

        public void ItemExclude(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(rootPath);

            XmlAttribute attributePath = doc.CreateAttribute("templateId");
            attributePath.Value = path.ToString();

            XmlElement elem = doc.CreateElement("exclude");
            elem.SetAttribute("templateId", path);

            XmlNode predicate = doc.SelectSingleNode("/configuration/sitecore/unicorn/configurations/configuration/predicate");

            if (predicate != null)
                foreach (XmlNode predicateChildNode in predicate.ChildNodes)
                {
                    if (!ContextItemStatus(predicateChildNode))
                    {
                        predicateChildNode.AppendChild(elem);
                    }
                    else
                    {
                        SheerResponse.Alert("Item is Exist Pattern");
                    }
                }
            doc.Save(rootPath);
        }

        public static bool ContextItemStatus(XmlNode parentNode)
        {
            if (parentNode.HasChildNodes)
            {
                XmlNodeList nodeList = parentNode.ChildNodes;
                foreach (XmlNode node in nodeList)
                {
                    if (node.Attributes?["templateId"] != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class Merge : Command
    {
        private static readonly string CustomXml =
            HttpContext.Current.Server.MapPath(@"~\App_Config\Include\Unicorn\Unicorn.SmartOSC.Config.Default");

        private static readonly string MainXml =
            HttpContext.Current.Server.MapPath(@"~\App_Config\Include\Unicorn\Unicorn.Configs.Default.config");

        public override void Execute(CommandContext context)
        {
            // call merge function
            DoMerge();
        }

        public void DoMerge()
        {
            // load main xml
            XmlDocument main = new XmlDocument();
            main.Load(MainXml);

            // load custom xml
            XmlDocument custom = new XmlDocument();
            custom.Load(CustomXml);

            // init main node
            XmlNode mConfigurations = main.SelectSingleNode("/configuration/sitecore/unicorn/configurations");

            // init custom node
            XmlNode cConfigurations = custom.SelectSingleNode("/configuration/sitecore/unicorn/configurations");

            // check main and custom node is not nul. If null so the reason is the config of main file or custom file is wrong. 
            // Right like this: /configuration/sitecore/unicorn/configurations
            if (cConfigurations != null && mConfigurations != null)
            {
                XmlNodeList allCustomNode = cConfigurations.ChildNodes;
                foreach (XmlNode cNode in allCustomNode)
                {
                    if (!CheckFeatureExist(mConfigurations, cNode))
                    {
                        if (mConfigurations.OwnerDocument != null)
                        {
                            try
                            {
                                // Convert To context xml to import
                                XmlNode importNode = mConfigurations.OwnerDocument.ImportNode(cNode, true);
                                mConfigurations.AppendChild(importNode);
                                main.Save(MainXml);
                                return;
                            }
                            catch (Exception e)
                            {
                                SheerResponse.Alert(
                                    $@"Your Unicorn.SmartOSC.Config.Default is missing inlcude at predicate node: Error: {
                                        e
                                    }");
                                throw;
                            }
                        }

                        SheerResponse.Alert("xxx");
                        return;
                    }
                    else
                    {
                        SheerResponse.Alert("Merge Failed. Because the Feature is Exist!");
                        return;
                    }
                }
            }
            else
            {
                SheerResponse.Alert("Merge Failed. Because the structure of Unicorn.Configs.Default.config is wrong.");
                return;
            }
        }

        /*
         * Function check Feature is Exist
         * @des : destination of node will be check
         * @src : source of node check
         */
        public static bool CheckFeatureExist(XmlNode des, XmlNode src)
        {
            // If des have node
            if (des.HasChildNodes)
            {
                foreach (XmlNode desChildNode in des.ChildNodes)
                {
                    // If Node of source have name Equal node name of des
                    if (src.Attributes != null && (desChildNode.Attributes != null && 
                                                   desChildNode.Attributes["name"].Value.Equals(src.Attributes["name"].Value)))
                    return true;
                }
            }
            return false;
        }
    }
}