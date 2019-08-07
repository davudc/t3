using System;
using ImGuiNET;
using T3.Core.Logging;
using T3.Gui.Commands;
using T3.Gui.Selection;

namespace T3.Gui.Graph
{
    public static class SelectableMovement
    {
        private static ChangeSelectableCommand _moveCommand = null;

        public static void Handle(ISelectable selectable)
        {
            if (ImGui.IsItemActive())
            {
                if (ImGui.IsItemClicked(0))
                {
                    var handler = GraphCanvas.Current.SelectionHandler;
                    if (!handler.SelectedElements.Contains(selectable))
                    {
                        handler.SetElement(selectable);
                    }

                    Guid compositionSymbolId = GraphCanvas.Current.CompositionOp.Symbol.Id;
                    _moveCommand = new ChangeSelectableCommand(compositionSymbolId, handler.SelectedElements);
                    Log.Debug("start");
                }

                if (ImGui.IsMouseDragging(0))
                {
                    foreach (var e in GraphCanvas.Current.SelectionHandler.SelectedElements)
                    {
                        e.PosOnCanvas += GraphCanvas.Current.InverseTransformDirection(ImGui.GetIO().MouseDelta);
                    }
                }
            }
            else if (ImGui.IsMouseReleased(0) && _moveCommand != null)
            {
                Log.Debug("end");
                if (ImGui.GetMouseDragDelta(0).LengthSquared() > 0.0f)
                {
                    // add to stack
                    Log.Debug("Added to undo stack");
                    _moveCommand.StoreCurrentValues();
                    UndoRedoStack.Add(_moveCommand);
                }

                _moveCommand = null;
            }
        }
    }
}