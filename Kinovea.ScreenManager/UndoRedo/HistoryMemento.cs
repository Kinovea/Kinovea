using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Abstraction over a reversible state.
    /// The memento stores the state prior to the change and will be capable to restore this state.
    /// It is constructed before the state-changing operation is performed.
    /// The constructor of the memento should store any relevant information to be able to recall the state later.
    ///
    /// Any context objects that are required to rebuild the state but that are themselves deleteable should be stored by value.
    /// For example, a drawing is stored in a keyframe. When adding a drawing, we should store the unique id of the keyframe, not 
    /// a .NET reference to the keyframe. If the keyframe is subsequently deleted and un-deleted, we will still need to be able to 
    /// undo the drawing addition relatively to the correct keyframe.
    /// </summary>
    public abstract class HistoryMemento
    {
        /// <summary>
        /// Name of the undoable command to be displayed in the menu.
        /// </summary>
        public abstract string CommandName { get; set; }
        
        /// <summary>
        /// Perform the necessary work required to reverse the state to how it was at construction time. 
        /// Must return a Memento that can be used to redo the operation, that is, undo the undo.
        /// </summary>
        public abstract HistoryMemento PerformUndo();
    }
}
