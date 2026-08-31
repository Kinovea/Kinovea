using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kinovea.Services
{
    public class CommandBindings
    {
        private Dictionary<string, List<HotkeyCommand>> bindings = new Dictionary<string, List<HotkeyCommand>>();

        public void Initialize(Dictionary<string, List<HotkeyCommand>> initialBindings)
        {
            bindings = initialBindings;
        }

        public CommandBindings Clone()
        {
            CommandBindings clone = new CommandBindings();
            if (bindings != null)
            {
                foreach (var category in bindings.Keys)
                {
                    List<HotkeyCommand> commands = bindings[category].Select(c => new HotkeyCommand(c.Name, c.KeyData)).ToList();
                    clone.AddCategory(category, commands);
                }
            }
            return clone;
        }

        public List<string> GetCategories()
        {
            if (bindings == null)
                return new List<string>();

            return bindings.Keys.ToList();
        }

        public bool HasCategory(string category)
        {
            return bindings != null && bindings.ContainsKey(category);
        }

        public void AddCategory(string category, List<HotkeyCommand> commands)
        {
            if (bindings == null)
                bindings = new Dictionary<string, List<HotkeyCommand>>();

            bindings[category] = commands;
        }


        /// <summary>
        /// Get all commands in the passed category.
        /// </summary>
        public List<HotkeyCommand> GetCommandBindings(string category)
        {
            if (bindings == null)
                return null;

            return bindings.ContainsKey(category) ? bindings[category] : null;
        }

        /// <summary>
        /// Get the command object for a specific category and name.
        /// </summary>
        public HotkeyCommand FindByName(string category, string name)
        {
            if (bindings == null || !bindings.ContainsKey(category))
                return null;

            return bindings[category].FirstOrDefault(c => c.Name == name);
        }

        /// <summary>
        /// Get the command object for a specific key data.
        /// </summary>
        public HotkeyCommand FindByKeyData(string category, Keys keys)
        {
            if (bindings == null || !bindings.ContainsKey(category))
                return null;

            return bindings[category].FirstOrDefault(c => c.KeyData == keys);
        }

        /// <summary>
        /// Import the passed bindings into the local collection.
        /// Only import known categories and command names.
        /// This is used to load the values from the preferences.
        /// </summary>
        public void Load(CommandBindings other)
        {
            if (bindings == null)
                return;

            foreach (string category in other.GetCategories())
            {
                List<HotkeyCommand> commands = other.GetCommandBindings(category);
                if (!bindings.ContainsKey(category))
                    continue;

                foreach (HotkeyCommand command in commands)
                {
                    Update(category, command.Name, command.KeyData);
                }
            }
        }

        /// <summary>
        /// Update a command to a new key binding.
        /// </summary>
        public void Update(string category, string name, Keys keyData)
        {
            if (bindings == null || !bindings.ContainsKey(category))
                return;

            var command = bindings[category].FirstOrDefault(c => c.Name == name);
            if (command == null)
                return;

            command.KeyData = keyData;
        }

        /// <summary>
        /// Returns false if there is a conflict on the binding in this category.
        /// </summary>
        public bool IsUnique(string category, string name, Keys keyData)
        {
            if (bindings == null || !bindings.ContainsKey(category))
                return true;

            if (keyData == Keys.None)
                return true;

            foreach (HotkeyCommand c in bindings[category])
            {
                if (c.Name == name || c.KeyData != keyData)
                    continue;

                // Same binding on different command in the same category.
                return false;
            }

            return true;
        }
    }
}
