#region License
/*
Copyright © Joan Charmant 2012.
joan.charmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	public static class GenericPostureManager
	{
		#region Properties
		public static List<DrawingToolGenericPosture> Tools
		{
		    get {
		        if(object.ReferenceEquals(m_tools, null))
		            Initialize();
		        
		        return m_tools;
		    }
		}
		#endregion
		
		#region Members
		private static Dictionary<Guid, string> m_files = null;
		private static List<DrawingToolGenericPosture> m_tools = null;
        #endregion
        
        #region Public methods
        public static GenericPosture Instanciate(Guid id)
        {
            if(m_files.ContainsKey(id))
                return new GenericPosture(m_files[id], false);
            else
                return null;
        }
        #endregion
        
        #region Private Methods
        private static void Initialize()
        {
        	m_files = new Dictionary<Guid, string>();
        	m_tools = new List<DrawingToolGenericPosture>();
        	
        	string dir = Path.GetDirectoryName(Application.ExecutablePath) + "\\DrawingTools";
        	
            if(!Directory.Exists(dir))
                return;
            
            foreach (string f in Directory.GetFiles(dir))
            {
                if (!Path.GetExtension(f).ToLower().Equals(".xml"))
                    continue;

                // Extract icon and name.
                GenericPosture posture = new GenericPosture(f, true);
                if(posture == null || posture.Id == Guid.Empty)
                    continue;

                m_files[posture.Id] = f;
                
                DrawingToolGenericPosture tool = new DrawingToolGenericPosture();
                tool.SetInfo(posture);
                m_tools.Add(tool);
            }
        }
        #endregion
	}
}

