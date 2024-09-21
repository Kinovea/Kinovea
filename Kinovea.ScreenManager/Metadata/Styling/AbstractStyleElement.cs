#region License
/*
Copyright © Joan Charmant 2011.
jcharmant@gmail.com 
 
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
using System.Windows.Forms;
using System.Xml;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A styling property for a drawing / drawing tool.
    /// Concrete style elements are wrappers for a particular data type.
    /// Style elements wraps the value, metadata like min/max and display name,
    /// and give access to an editor that can update the value.
    /// These elements are bound to properties of the style data. 
    /// The style data is a union of all styling properties possible.
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
        /// By the time this event is raised the underlying data has been updated already.
        /// </summary>
        public event EventHandler<EventArgs<string>> ValueChanged;
        #endregion
        
        #region Members

        // The style data object that the style element is bound to.
        protected internal StyleData styleData;

        // The name of the property inside the style data object that we are bound to.
        private string targetProperty;
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// This should return a mini editor.
        /// Any change in the editor should trigger the binding mechanics
        /// to update the corresponding data field in the style data.
        /// </summary>
        /// <returns></returns>
        public abstract Control GetEditor();

        /// <summary>
        /// The value was changed externally and the passed editor needs to be updated.
        /// </summary>
        public abstract void UpdateEditor(Control control);

        
        /// <summary>
        /// Deep clone of the full style element.
        /// This should include value and metadata.
        /// </summary>
        public abstract AbstractStyleElement Clone();
        
        /// <summary>
        /// Save the style element to XML.
        /// This is always in the context of saving a drawing or 
        /// a preset so this should only writes the value, 
        /// not the metadata like min/max and display name.
        /// </summary>
        public abstract void WriteXml(XmlWriter xmlWriter);

        /// <summary>
        /// Import the style element from XML.
        /// There are several contexts where we read a style element from XML.
        /// 1. We are importing the tool itself, in this case we need 
        /// to read the metadata like display name and min/max values.
        /// 2. We are importing a drawing or a preset. In this case we are only 
        /// interested in the value.
        /// </summary>
        public abstract void ReadXML(XmlReader xmlReader);
        
        /// <summary>
        /// Create a link between this style element and a particular 
        /// property in a style data object.
        /// </summary>
        public void SetBindTarget(StyleData styleData, string targetProperty)
        {
            this.styleData = styleData;
            this.targetProperty = targetProperty;
        }

        /// <summary>
        /// Bind this style element to the same target data than the 
        /// passed style element.
        /// This is used in the context of cloning.
        /// </summary>
        public void BindClone(AbstractStyleElement original)
        {
            styleData = original.styleData;
            targetProperty = original.targetProperty;
        }

        /// <summary>
        /// Update the underlying style data from this style element.
        /// Signal the update to any listeners.
        /// </summary>
        public void ExportValueToData()
        {
            if (styleData == null || string.IsNullOrEmpty(targetProperty))
                return;

            bool updated = styleData.Set(targetProperty, Value);
            if (updated)
                ValueChanged?.Invoke(this, new EventArgs<string>(string.Format("{0}={1}", targetProperty, Value.ToString())));
        }

        /// <summary>
        /// Import style data into style element.
        /// </summary>
        public void ImportValueFromData()
        {
            if (styleData == null || string.IsNullOrEmpty(targetProperty))
                return;

            // Caveat: affecting Value will raise back BindWrite().
            Value = styleData.Get(targetProperty, Value.GetType());
        }

        /// <summary>
        /// Format the value and its target prop to a string.
        /// </summary>
        public override string ToString()
        {
            return String.Format("{0} Bound to:{1}",
                Value.ToString(), string.IsNullOrEmpty(targetProperty) ? "Nothing" : targetProperty);
        }
        #endregion
    }
}
