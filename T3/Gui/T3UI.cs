﻿using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using T3.Core.Operator;
using T3.graph;
using T3.Gui.Graph;
using T3.Logging;

namespace T3.Gui
{
    /// <summary>
    /// A singleton capsule T3 UI functionality from imgui-clutter in Program.cs
    /// </summary>
    public class T3UI
    {
        public T3UI()
        {
            _instance = this;

            // Open a default Window
            OpenNewGraphWindow();
            _quickCreateWindow = new QuickCreateWindow();
        }

        public static void OpenNewGraphWindow()
        {
            _instance._graphCanvasWindows.Add(new GraphCanvasWindow(_mockModel.MainOp, "Composition View " + _instance._graphCanvasWindows.Count));
        }


        public unsafe void DrawUI()
        {
            DrawGraphCanvasWindows();
            if (UiSettingsWindow.DemoWindowVisible)
                ImGui.ShowDemoWindow(ref UiSettingsWindow.DemoWindowVisible);

            if (UiSettingsWindow.ConsoleWindowVisible)
                _consoleWindow.Draw(ref UiSettingsWindow.ConsoleWindowVisible);

            _quickCreateWindow.Draw();

            SwapHoveringBuffers();
        }


        private unsafe void DrawGraphCanvasWindows()
        {
            GraphCanvasWindow obsoleteGraphWindow = null;
            foreach (var g in _graphCanvasWindows)
            {
                if (!g.Draw())
                    obsoleteGraphWindow = g;   // we assume that only one window can be close in per frame
            }
            if (obsoleteGraphWindow != null)
                _graphCanvasWindows.Remove(obsoleteGraphWindow);
        }


        public void DrawSelectionParameters()
        {
            ImGui.Begin("ParameterView");
            foreach (var pair in SymbolChildUiRegistry.Entries[_mockModel.MainOp.Symbol.Id])
            {
                var symbolChildUi = pair.Value;
                if (!symbolChildUi.IsSelected)
                    continue;

                var symbolChild = symbolChildUi.SymbolChild;
                foreach (var inputDefinition in symbolChild.Symbol.InputDefinitions)
                {
                    DrawParameterViewEntry(inputDefinition, symbolChild);
                }

                break; // only first selected atm
            }
            ImGui.End();
        }

        private static void DrawParameterViewEntry(Symbol.InputDefinition inputDefinition, SymbolChild symbolChild)
        {
            ImGui.PushID(inputDefinition.Id.GetHashCode());
            SymbolChild.Input input = symbolChild.InputValues[inputDefinition.Id];
            IInputUi inputUi = InputUiRegistry.Entries[input.Value.ValueType];
            inputUi.DrawInputEdit(inputDefinition.Name, input);
            ImGui.PopID();
        }

        public void DrawSelectedOutput()
        {
            ImGui.Begin("SelectionView");
            var compositionOp = _mockModel.MainOp;
            foreach (var pair in SymbolChildUiRegistry.Entries[compositionOp.Symbol.Id])
            {
                var symbolChildUi = pair.Value;
                if (!symbolChildUi.IsSelected)
                    continue;

                var symbolChild = symbolChildUi.SymbolChild;
                var selectedInstance = compositionOp.Children.Single(child => child.Id == symbolChild.Id);

                var firstOutput = selectedInstance.Outputs[0];
                IOutputUi outputUi = OutputUiRegistry.Entries[firstOutput.Type];
                outputUi.Draw(firstOutput);

                break; // only first selected atm
            }
            ImGui.End();
        }

        public static void AddHoveredId(Guid id)
        {
            _hoveredIdsForNextFrame.Add(id);
        }

        public void SwapHoveringBuffers()
        {
            HoveredIdsLastFrame = _hoveredIdsForNextFrame;
            _hoveredIdsForNextFrame = new HashSet<Guid>();
        }

        public static HashSet<Guid> _hoveredIdsForNextFrame = new HashSet<Guid>();
        public static HashSet<Guid> HoveredIdsLastFrame { get; set; } = new HashSet<Guid>();

        public unsafe void InitStyle()
        {
            var style = ImGui.GetStyle();
            style.WindowRounding = 0;

            style.FramePadding = new Vector2(7, 4);
            style.ItemSpacing = new Vector2(4, 3);
            style.ItemInnerSpacing = new Vector2(3, 2);
            style.GrabMinSize = 2;
            style.FrameBorderSize = 0;
            style.WindowRounding = 3;
            style.ChildRounding = 1;
            style.ScrollbarRounding = 3;

            style.WindowRounding = 5.3f;
            style.FrameRounding = 2.3f;
            style.ScrollbarRounding = 0;

            style.Colors[(int)ImGuiCol.Text] = new Vector4(1, 1, 1, 0.85f);
            style.Colors[(int)ImGuiCol.Border] = new Vector4(0, 0.00f, 0.00f, 0.97f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.13f, 0.13f, 0.13f, 0.80f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.38f, 0.38f, 0.38f, 0.40f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.00f, 0.55f, 0.8f, 1.00f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.12f, 0.12f, 0.12f, 0.53f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.31f, 0.31f, 0.31f, 0.33f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.00f, 0.00f, 0.00f, 0.25f);
        }

        private List<GraphCanvasWindow> _graphCanvasWindows = new List<GraphCanvasWindow>();
        private static MockModel _mockModel = new MockModel();
        private ConsoleLogWindow _consoleWindow = new ConsoleLogWindow();
        private static T3UI _instance = null;
        private QuickCreateWindow _quickCreateWindow = null;
    }
}