using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Options are flags that enable/disable visibility of objects or usage of constraints/impacts.
    /// Each object can declare being bound to one or more options, separated by '|'.
    /// The options are exposed as entries in the context menu of the tool and the user can toggle them.
    /// </summary>
    public class GenericPostureOption
    {
        /// <summary>
        /// Key used to index the option in the option collection.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// User-friendly label displayed in the contextual menu.
        /// Does not support localization.
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// Default value for the option.
        /// </summary>
        public bool DefaultValue { get; private set; }

        /// <summary>
        /// Current value for the option.
        /// </summary>
        public bool Value { get; set; }

        /// <summary>
        /// Whether this option is shown in the contextual menu.
        /// Hidden options are useful when creating instances of a tool programmatically and some points aren't valid.
        /// For example when rebuilding human models from partial data, some joints may not be visible.
        /// In this case the option is only used internally and shouldn't be visible to the user.
        /// </summary>
        public bool Hidden { get; private set; }


        public GenericPostureOption(XmlReader r)
        {
            Key = "";
            Label = "";
            DefaultValue = false;
            Hidden = false;

            if (r.MoveToAttribute("key"))
                Key = r.ReadContentAsString();

            if (r.MoveToAttribute("label"))
                Label = r.ReadContentAsString();

            if (r.MoveToAttribute("default"))
                DefaultValue = XmlHelper.ParseBoolean(r.ReadContentAsString());

            if (r.MoveToAttribute("hidden"))
                Hidden = XmlHelper.ParseBoolean(r.ReadContentAsString());

            r.ReadStartElement();

            Value = DefaultValue;
        }

        public GenericPostureOption(string key, string label, bool defaultValue, bool hidden)
        {
            Key = key;
            Label = label;
            DefaultValue = defaultValue;
            Hidden = hidden;

            Value = DefaultValue;
        }
    }
}
