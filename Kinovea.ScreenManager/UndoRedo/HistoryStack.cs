using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Loosely based on Paint.NET history stack.
    /// </summary>
    public class HistoryStack
    {
        private List<HistoryMemento> undoStack = new List<HistoryMemento>();
        private List<HistoryMemento> redoStack = new List<HistoryMemento>();

        public bool CanUndo
        {
            get { return undoStack.Count > 0; }
        }

        public bool CanRedo
        {
            get { return redoStack.Count > 0; }
        }

        public HistoryStack()
        {
        }

        public void PushNewCommand(HistoryMemento value)
        {
            ClearRedoStack();
            undoStack.Add(value);
        }

        /// <summary>
        /// Undoes the top of the undo stack, then places the created redo command to the top of the redo stack.
        /// </summary>
        public void StepBackward()
        {
            if (undoStack.Count == 0)
                return;

            HistoryMemento undoCommand = undoStack[undoStack.Count - 1];
            HistoryMemento redoCommand = undoCommand.PerformUndo();
            undoStack.RemoveAt(undoStack.Count - 1);
            redoStack.Add(redoCommand);
        }

        /// <summary>
        /// Redoes the top of the redo stack, then places the created undo command to the top of the undo stack.
        /// </summary>
        public void StepForward()
        {
            if (redoStack.Count == 0)
                return;

            HistoryMemento redoCommand = redoStack[redoStack.Count - 1];
            HistoryMemento undoCommand = redoCommand.PerformUndo();
            redoStack.RemoveAt(redoStack.Count - 1);
            undoStack.Add(undoCommand);
        }

        public void ClearAll()
        {
            undoStack.Clear();
            redoStack.Clear();
        }

        public void ClearRedoStack()
        {
            redoStack.Clear();
        }
    }
}
