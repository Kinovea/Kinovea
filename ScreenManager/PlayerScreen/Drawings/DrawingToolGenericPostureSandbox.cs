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
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    // A sandbox for the generic posture tool.
    
    public class DrawingToolGenericPostureSandbox : AbstractDrawingTool
    {
    	#region Properties
		public override string DisplayName
		{
		    get { return displayName;}
		}
		public override Bitmap Icon
		{
			get { return Properties.Drawings.generic_posture; }
		}
    	public override bool Attached
    	{
    		get { return true; }
    	}
		public override bool KeepTool
    	{
    		get { return false; }
    	}
    	public override bool KeepToolFrameChanged
    	{
    		get { return false; }
    	}
		public override DrawingStyle StylePreset
		{
			get { return m_StylePreset;}
			set { m_StylePreset = value;}
		}
		public override DrawingStyle DefaultStylePreset
		{
			get { return m_DefaultStylePreset;}
		}
    	#endregion
    	
    	#region Members
    	private DrawingStyle m_DefaultStylePreset = new DrawingStyle();
    	private DrawingStyle m_StylePreset;
    	private string filename;
    	private string displayName = "Generic Posture";
    	#endregion
		
		#region Constructor
		public DrawingToolGenericPostureSandbox()
		{
			m_DefaultStylePreset.Elements.Add("line color", new StyleElementColor(Color.DarkOliveGreen));
			m_StylePreset = m_DefaultStylePreset.Clone();
		}
		#endregion
		
		#region Public Methods
		public override AbstractDrawing GetNewDrawing(Point _Origin, long _iTimestamp, long _AverageTimeStampsPerFrame)
		{
		    //string tool = @"C:\Users\Joan\Dev  Prog\Videa\Bitbucket\GenericPosture\Tools\postures\AlignmentAngle.xml";
		    if(string.IsNullOrEmpty(filename) || !File.Exists(filename))
		        return null;
		    
		    GenericPosture posture = new GenericPosture(filename);
		    return new DrawingGenericPosture(_Origin, posture, _iTimestamp, _AverageTimeStampsPerFrame, m_StylePreset);
		}
		public override Cursor GetCursor(double _fStretchFactor)
		{
			return Cursors.Cross;
		}
		public void SetFile(string filename)
		{
		    this.filename = filename;
		    this.displayName = Path.GetFileNameWithoutExtension(filename);
		    
		    // Extract icon.
		}
		#endregion
    }
}




