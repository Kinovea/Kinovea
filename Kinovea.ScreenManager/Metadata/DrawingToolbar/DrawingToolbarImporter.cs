using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class DrawingToolbarImporter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Imports the hierarchy of tools to use in the screen.
        /// </summary>
        public void Import(string file, DrawingToolbarPresenter drawingToolbarPresenter, EventHandler handler)
        {
            string path = Path.Combine(Software.ToolbarsDirectory, file);

            if (!File.Exists(path))
                return;

            List<List<AbstractDrawingTool>> hierarchy = ImportDrawingTools(path);
            Commit(hierarchy, drawingToolbarPresenter, handler);
        }

        /// <summary>
        /// Commit the tools to the toolbar.
        /// </summary>
        public void Commit(List<List<AbstractDrawingTool>> hierarchy, DrawingToolbarPresenter drawingToolbarPresenter, EventHandler handler)
        {
            foreach (List<AbstractDrawingTool> tools in hierarchy)
            {
                if (tools.Count == 0)
                    continue;

                if (tools.Count == 1)
                    drawingToolbarPresenter.AddToolButton(tools[0], handler);
                else
                    drawingToolbarPresenter.AddToolButtonGroup(tools.ToArray(), 0, handler);
            }
        }

        private List<List<AbstractDrawingTool>> ImportDrawingTools(string path)
        {
            List<List<AbstractDrawingTool>> hierarchy = new List<List<AbstractDrawingTool>>();
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNodeList nodes = doc.SelectNodes("/KinoveaToolbar/entry");

            foreach (XmlNode node in nodes)
            {
                List<AbstractDrawingTool> tools = new List<AbstractDrawingTool>();
                ImportDrawingTool(tools, node);

                XmlNodeList subNodes = node.SelectNodes("subentry");
                foreach (XmlNode subNode in subNodes)
                    ImportDrawingTool(tools, subNode);

                hierarchy.Add(tools);
            }

            return hierarchy;
        }

        private void ImportDrawingTool(List<AbstractDrawingTool> list, XmlNode node)
        {
            XmlAttribute nameAttribute = node.Attributes["name"];
            if (nameAttribute == null)
                return;

            string toolName = nameAttribute.Value;

            if (toolName == "%CustomTools%")
            {
                foreach (AbstractDrawingTool customTool in GenericPostureManager.Tools)
                    list.Add(customTool);

                return;
            }

            if (toolName == "%separator%")
            {
                AbstractDrawingTool tool = new DrawingToolSeparator();
                list.Add(tool);
                return;
            }

            if (ToolManager.Tools.ContainsKey(toolName))
            {
                AbstractDrawingTool tool = ToolManager.Tools[toolName];
                list.Add(tool);
            }
            else
            {
                log.ErrorFormat("The tool manager doesn't know the tool named {0}.", toolName);
            }
        }
    }
}
