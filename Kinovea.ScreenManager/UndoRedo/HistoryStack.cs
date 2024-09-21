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
        #region Events
        public EventHandler HistoryChanged;
        #endregion

        #region Properties
        public bool CanUndo
        {
            get { return undoStack.Count > 0; }
        }

        public bool CanRedo
        {
            get { return redoStack.Count > 0; }
        }

        public string UndoActionName
        {
            get { return CanUndo ? undoStack[undoStack.Count - 1].CommandName : ""; }
        }

        public string RedoActionName
        {
            get { return CanRedo ? redoStack[redoStack.Count - 1].CommandName : ""; }
        }

        /// <summary>
        /// Whether we are currently performing the undo or redo operation.
        /// </summary>
        public bool IsPerformingUndoRedo
        {
            get { return isPerformingUndoRedo; }
        }
        #endregion

        #region Members
        private List<HistoryMemento> undoStack = new List<HistoryMemento>();
        private List<HistoryMemento> redoStack = new List<HistoryMemento>();
        private bool isPerformingUndoRedo;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        /// <summary>
        /// Push a new command at the top of the undo stack.
        /// Clears the redo stack.
        /// </summary>
        public void PushNewCommand(HistoryMemento value)
        {
            //// Prevent duplicated commands being pushed to the undo stack.
            //// This may happen when several parts of the UI independently prepare themselves for changes.
            //// Commented out: there is currently no scenario where this happens.
            //// In particular the memento created by the select action contains the core data while the one 
            //// created by the side panel contains style data.
            //if (value is HistoryMementoModifyDrawing && undoStack.Count > 0 && undoStack[undoStack.Count - 1] is HistoryMementoModifyDrawing)
            //{
            //    if (((HistoryMementoModifyDrawing)value).IsSameState(((HistoryMementoModifyDrawing)undoStack[undoStack.Count - 1])))
            //    {
            //        log.Debug("Ignore history memento with the same state as the top of the undo stack.");
            //        return;
            //    }
            //}
            
            // Invalidate the redo stack.
            if (redoStack.Count > 0)
                ClearRedoStack();

            undoStack.Add(value);
            OnHistoryChanged();
        }

        /// <summary>
        /// Undo the top of the undo stack.
        /// Get the corresponding redo command and place at the top of the redo stack.
        /// </summary>
        public void StepBackward()
        {
            if (undoStack.Count == 0)
                return;

            HistoryMemento undoCommand = undoStack[undoStack.Count - 1];
            isPerformingUndoRedo = true;
            HistoryMemento redoCommand = undoCommand.PerformUndo();
            isPerformingUndoRedo = false;
            undoStack.RemoveAt(undoStack.Count - 1);
            redoStack.Add(redoCommand);
            OnHistoryChanged();
        }

        /// <summary>
        /// Redo the top of the redo stack. 
        /// Get the corresponding undo command and place it at the top of the undo stack.
        /// </summary>
        public void StepForward()
        {
            if (redoStack.Count == 0)
                return;

            HistoryMemento redoCommand = redoStack[redoStack.Count - 1];
            isPerformingUndoRedo = true;
            HistoryMemento undoCommand = redoCommand.PerformUndo();
            isPerformingUndoRedo = false;
            redoStack.RemoveAt(redoStack.Count - 1);
            undoStack.Add(undoCommand);
            OnHistoryChanged();
        }

        /// <summary>
        /// Clear the redo stack.
        /// </summary>
        public void ClearRedoStack()
        {
            redoStack.Clear();
            OnHistoryChanged();
        }

        private void OnHistoryChanged()
        {
            if (HistoryChanged != null)
                HistoryChanged(this, EventArgs.Empty);

            //DumpStacks();
        }

        private void DumpStacks()
        {
            log.Debug("------------");
            log.Debug("Undo stack:");
            foreach (var command in undoStack)
            {
                log.DebugFormat("\t{0}", command.CommandName);
            }

            log.Debug("Redo stack:");
            foreach (var command in redoStack)
            {
                log.DebugFormat("\t{0}", command.CommandName);
            }
        }
    }
}
