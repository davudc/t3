﻿using System;
using System.Linq;
using ImGuiNET;
using T3.Core.Operator;
using T3.Core.Operator.Slots;
using T3.Gui.UiHelpers;

namespace T3.Gui.Graph.Dialogs
{
    public class EditNodeOutputDialog : ModalDialog
    {
        public void Draw()
        {
            if (BeginDialog("Edit node output"))
            {
                ImGui.Text("Define the dirty flag behavior for " + _outputDefinition.Name);

                var symbolChild = _symbolChildUi.SymbolChild;
                var outputEntry = symbolChild.Outputs[_outputDefinition.Id];
                var enumType = typeof(DirtyFlagTrigger);
                var values = Enum.GetValues(enumType);
                var valueNames = Enum.GetNames(enumType);
                var currentValueName = Enum.GetName(enumType, outputEntry.DirtyFlagTrigger);

                int index = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    if (valueNames[i] == currentValueName)
                    {
                        index = i;
                        break;
                    }
                }

                if (ImGui.Combo("##dirtyFlagTriggerDropDownParam", ref index, valueNames, valueNames.Length))
                {
                    var trigger = (DirtyFlagTrigger)Enum.Parse(enumType, valueNames[index]);
                    outputEntry.DirtyFlagTrigger = trigger;
                    foreach (var compositionInstance in _compositionSymbol.InstancesOfSymbol)
                    {
                        ISlot outputSlot = (from instance in compositionInstance.Children
                                            where instance.SymbolChildId == symbolChild.Id
                                            from output in instance.Outputs
                                            where output.Id == _outputDefinition.Id
                                            select output).Single();
                        outputSlot.DirtyFlag.Trigger = trigger;
                    }
                }

                if (ImGui.Button("Close"))
                {
                    ImGui.CloseCurrentPopup();
                }

                EndDialogContent();
            }

            EndDialog();
        }

        public void OpenForOutput(Symbol compositionSymbol, SymbolChildUi symbolChildUi, Symbol.OutputDefinition outputDefinition)
        {
            _compositionSymbol = compositionSymbol;
            _symbolChildUi = symbolChildUi;
            _outputDefinition = outputDefinition;
            ShowNextFrame();
        }

        private SymbolChildUi _symbolChildUi;
        private Symbol.OutputDefinition _outputDefinition;
        private Symbol _compositionSymbol;
    }
}