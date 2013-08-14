#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace Kinovea.ScreenManager
{   
    /// <summary>
    /// A styling property for a drawing / drawing tool.
    /// Concrete style elements related to a drawing are then collected in a DrawingStyle object.
    /// </summary>
    /// <remarks>
    /// If two variables are needed to represent a style element, consider creating a composite style.
    /// For exemple, if x and y (e.g: back color and font size) MUST be edited together,
    /// create a class exposing x and y in a meaningful way, and use it as a whole as a style element..
    /// Otherwise, if x and y can be edited separately, add two separate style elements to the DrawingStyle.
    /// Bottom line: a single value will have to be picked from a graphical editor.
    /// </remarks>
    public abstract class AbstractStyleElement
    {
        #region Properties
        /// <summary>
        /// The current value of the style element.
        /// </summary>
        public abstract object Value
        {
            get;
            set;
        }
        
        /// <summary>
        /// Icon used by mini editor containers to hint on the type of property which is edited.
        /// </summary>
        public abstract Bitmap Icon
        {
            get;
        }
        
        /// <summary>
        /// The name of the property. Used by mini editors containers.
        /// </summary>
        public abstract string DisplayName
        {
            get;
        }
        
        /// <summary>
        /// The type of the property for serialization.
        /// </summary>
        public abstract string XmlName
        {
            get;
        }
        
        /// <summary>
        /// Event raised when the value is changed. (call RaiseValueChanged() to trigger)
        /// The mini editor container will hook to this and update main screen accordingly to enable real time update.
        /// </summary>
        public event EventHandler ValueChanged;
        #endregion
        
        #region Members
        protected internal StyleHelper m_BindTarget;		// An object containing a property that needs to be updated each time the style element value change.
        private string m_BindTargetProperty;	// Name of the property to update when the style element value changes.
        #endregion
        
        #region Public Methods
        public abstract Control GetEditor();
        public abstract AbstractStyleElement Clone();
        public abstract void WriteXml(XmlWriter _xmlWriter);
        public abstract void ReadXML(XmlReader _xmlReader);
        
        public void Bind(StyleHelper _target, string _targetProperty)
        {
            m_BindTarget = _target;
            m_BindTargetProperty = _targetProperty;
            
            // On bind, we push the style element value to the internal property.
            RaiseValueChanged();
        }
        public void Bind(AbstractStyleElement _original)
        {
            // This function is used in the context of cloning, to clone the target data.
            m_BindTarget = _original.m_BindTarget;
            m_BindTargetProperty = _original.m_BindTargetProperty;
        }
        public void RaiseValueChanged()
        {
            // 1. Update bound data
            if(m_BindTarget != null && m_BindTarget.BindWrite != null && !string.IsNullOrEmpty(m_BindTargetProperty))
            {
                m_BindTarget.BindWrite(m_BindTargetProperty, Value);
            }
            
            // 2. Raise event
            if(ValueChanged != null) ValueChanged(null, EventArgs.Empty);
        }
        public void ReadValue()
        {
            // Update in case the value has been modified externally.
            // Caveat: affecting Value will raise back BindWrite().
            if(m_BindTarget != null && m_BindTarget.BindRead != null && !string.IsNullOrEmpty(m_BindTargetProperty))
            {
                Value = m_BindTarget.BindRead(m_BindTargetProperty, Value.GetType());
            }
        }
        public override string ToString()
        {
            return String.Format("{0} Bound to:{1}",
                                 Value.ToString(), string.IsNullOrEmpty(m_BindTargetProperty) ? "Nothing":m_BindTargetProperty);
        }
        #endregion
    }
}
