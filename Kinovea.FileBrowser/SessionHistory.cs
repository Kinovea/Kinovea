using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpTreeLib;

namespace Kinovea.FileBrowser
{
    /// <summary>
    /// This class holds the folder navigation history for the current session.
    /// </summary>
    public class SessionHistory
    {
        /// <summary>
        /// Get the current folder.
        /// </summary>
        public CShItem Current
        {
            get 
            {
                if (history.Count == 0 || index < 0 || index > history.Count - 1)
                    return null;

                return history[index]; 
            }
        }

        /// <summary>
        /// Get or set whether we are currently performing a navigation operation.
        /// This is set to true when we initiate the back or forward op, and 
        /// it's only set to false from the outside when the explorer is fully updated with
        /// the new path.
        /// </summary>
        public bool Navigating
        {
            get { return navigating; }
            set { navigating = value; }
        }

        private List<CShItem> history = new List<CShItem>();
        private int index = -1;
        private bool navigating;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Add new path and delete forward history.
        /// </summary>
        public void Add(CShItem item)
        {
            if (navigating)
                return;

            // Delete forward history if any.
            if (history.Count > 0 && index < history.Count - 1)
                history.RemoveRange(index + 1, history.Count - index - 1);

            bool isCurrent = history.Count > 0 && index >= 0 && history[index].Path == item.Path;
            if (!isCurrent)
                history.Add(item);

            index = history.Count - 1;
        }

        /// <summary>
        /// Go back one step in the history.
        /// </summary>
        public void Back()
        {
            if (history.Count == 0 || index < 1)
                return;

            index--;
            navigating = true;
        }

        /// <summary>
        /// Go forward one step in the history.
        /// </summary>
        public void Forward()
        {
            if (index >= history.Count - 1)
                return;

            index++;
            navigating = true;
        }
    }
}
