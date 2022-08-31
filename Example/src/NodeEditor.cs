using System;
using System.Collections.Generic;
using System.Numerics;

using ImGuiNET;

namespace Example;

public class NodeEditor
{

    struct Node
    {
        public int ID = -1;
        public string Name; //32
        public Vector2 Pos;
        public Vector2 Size;
        public float Value;
        public Vector4 Color;
        public int InputsCount, OutputsCount;

        public Node(int id, string name, Vector2 pos, float value, Vector4 color, int inputs_count, int outputs_count)
        {
            ID = id;
            Name = name;
            Pos = pos;
            Value = value;
            Color = color;
            InputsCount = inputs_count;
            OutputsCount = outputs_count;
            Size = new Vector2();
        }

        public Vector2 GetInputSlotPos(int slot_no)
        {
            return new Vector2(Pos.X, Pos.Y + Size.Y* ((float) slot_no + 1) / ((float) InputsCount + 1));
        }
        public Vector2 GetOutputSlotPos(int slot_no)
        {
            return new Vector2(Pos.X + Size.X, Pos.Y + Size.Y* ((float) slot_no + 1) / ((float) OutputsCount + 1));
        }
    }
    struct NodeLink
    {
        public int InputIdx, InputSlot, OutputIdx, OutputSlot;

        public NodeLink(int input_idx, int input_slot, int output_idx, int output_slot)
        {
            InputIdx = input_idx;
            InputSlot = input_slot;
            OutputIdx = output_idx;
            OutputSlot = output_slot;
        }
    }

    // State
    private List<Node> nodes = new List<Node>();
    private List<NodeLink> links = new List<NodeLink>();
    private Vector2 scrolling = new Vector2(0.0f, 0.0f);
    private bool inited = false;
    private bool show_grid = true;
    private int node_selected = -1;


    public NodeEditor()
    {
        if (!inited)
        {
            nodes.Add(new Node(0, "MainTex", new Vector2(40, 50), 0.5f, new Vector4(255, 100, 100, 255), 1, 1));
            nodes.Add(new Node(1, "BumpMap", new Vector2(40, 150), 0.42f, new Vector4(200, 100, 200, 255), 1, 1));
            nodes.Add(new Node(2, "Combine", new Vector2(270, 80), 1.0f, new Vector4(0, 200, 100, 255), 2, 2));
            links.Add(new NodeLink(0, 0, 2, 0));
            links.Add(new NodeLink(1, 0, 2, 1));
            inited = true;
        }
    }

    // Dummy data structure provided for the example.
    // Note that we storing links as indices (not ID) to make example code shorter.
    public void ShowExampleAppCustomNodeGraph(ref bool opened)
    {
        ImGui.SetNextWindowSize(new Vector2(700, 600), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin("Example: Custom Node Graph", ref opened))
        {
            ImGui.End();
            return;
        }

        // Dummy

        // State

        // Initialization
        ImGuiIOPtr io = ImGui.GetIO();


        // Draw a list of nodes on the left side
        bool open_context_menu = false;
        int node_hovered_in_list = -1;
        int node_hovered_in_scene = -1;
        ImGui.BeginChild("node_list", new Vector2(100, 0));
        ImGui.Text("Nodes");
        ImGui.Separator();
        for (int node_idx = 0; node_idx < nodes.Count; node_idx++)
        {
            Node node = nodes[node_idx];
            ImGui.PushID(node.ID);
            if (ImGui.Selectable(node.Name, node.ID == node_selected))
                node_selected = node.ID;
            if (ImGui.IsItemHovered())
            {
                node_hovered_in_list = node.ID;
                open_context_menu |= ImGui.IsMouseClicked(1);
            }
            ImGui.PopID();
        }
        ImGui.EndChild();

        ImGui.SameLine();
        ImGui.BeginGroup();

        float NODE_SLOT_RADIUS = 4.0f;
        Vector2 NODE_WINDOW_PADDING = new Vector2(8.0f, 8.0f);

        // Create our child canvas
        ImGui.Text($"Hold middle mouse button to scroll ({scrolling.X}, {scrolling.Y})");
        ImGui.SameLine(ImGui.GetWindowWidth() - 100);
        ImGui.Checkbox("Show grid", ref show_grid);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(1, 1));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0, 0, 0, 255)); //new Vector4(60, 60, 70, 200));
        ImGui.BeginChild("scrolling_region", new Vector2(0, 0), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove);
        ImGui.PopStyleVar(); // WindowPadding
        ImGui.PushItemWidth(120.0f);

        Vector2 offset = ImGui.GetCursorScreenPos() + scrolling;
        ImDrawListPtr draw_list = ImGui.GetWindowDrawList();
        
        // Display grid
        if (show_grid)
        {
            Vector4 GRID_COLOR = new Vector4(200, 200, 200, 40);
            float GRID_SZ = 64.0f;
            Vector2 win_pos = ImGui.GetCursorScreenPos();
            Vector2 canvas_sz = ImGui.GetWindowSize();
            for (float x = scrolling.X % GRID_SZ; x < canvas_sz.X; x += GRID_SZ)
                draw_list.AddLine(new Vector2(x, 0.0f) + win_pos, new Vector2(x, canvas_sz.Y) + win_pos, GRID_COLOR.ToUint());
            for (float y = scrolling.Y % GRID_SZ; y < canvas_sz.Y; y += GRID_SZ)
                draw_list.AddLine(new Vector2(0.0f, y) + win_pos, new Vector2(canvas_sz.X, y) + win_pos, GRID_COLOR.ToUint());
        }

        // Display links
        draw_list.ChannelsSplit(2);
        draw_list.ChannelsSetCurrent(0); // Background
        for (int link_idx = 0; link_idx < links.Count; link_idx++)
        {
            NodeLink link = links[link_idx];
            Node node_inp = nodes[link.InputIdx];
            Node node_out = nodes[link.OutputIdx];
            Vector2 p1 = offset + node_inp.GetOutputSlotPos(link.InputSlot);
            Vector2 p2 = offset + node_out.GetInputSlotPos(link.OutputSlot);
            draw_list.AddBezierCurve(p1, p1 + new Vector2(+50, 0), p2 + new Vector2(-50, 0), p2, new Vector4(200, 200, 100, 255).ToUint(), 3.0f);
        }

        // Display nodes
        for (int node_idx = 0; node_idx < nodes.Count; node_idx++)
        {
            Node node = nodes[node_idx];
            ImGui.PushID(node.ID);
            Vector2 node_rect_min = offset + node.Pos;

            // Display node contents first
            draw_list.ChannelsSetCurrent(1); // Foreground
            bool old_any_active = ImGui.IsAnyItemActive();
            ImGui.SetCursorScreenPos(node_rect_min + NODE_WINDOW_PADDING);
            ImGui.BeginGroup(); // Lock horizontal position
            ImGui.Text(node.Pos.ToString());
            ImGui.SliderFloat("##value", ref node.Value, 0.0f, 1.0f, "Alpha %.2f");
            ImGui.ColorEdit4("##color", ref node.Color);
            ImGui.EndGroup();

            // Save the size of what we have emitted and whether any of the widgets are being used
            bool node_widgets_active = (!old_any_active && ImGui.IsAnyItemActive());
            node.Size = ImGui.GetItemRectSize() + NODE_WINDOW_PADDING + NODE_WINDOW_PADDING;
            Vector2 node_rect_max = node_rect_min + node.Size;

            // Display node box
            draw_list.ChannelsSetCurrent(0); // Background
            ImGui.SetCursorScreenPos(node_rect_min);
            ImGui.InvisibleButton("node", node.Size);
            if (ImGui.IsItemHovered())
            {
                node_hovered_in_scene = node.ID;
                open_context_menu |= ImGui.IsMouseClicked(1);
            }
            bool node_moving_active = ImGui.IsItemActive();
            if (node_widgets_active || node_moving_active)
                node_selected = node.ID;
            if (node_moving_active && ImGui.IsMouseDragging(0)) //ImGuiMouseButton_Left
                node.Pos = node.Pos + io.MouseDelta;

            uint node_bg_color = (node_hovered_in_list == node.ID || node_hovered_in_scene == node.ID || (node_hovered_in_list == -1 && node_selected == node.ID)) ? new Vector4(75, 75, 75, 255).ToUint() : new Vector4(60, 60, 60, 255).ToUint();
            draw_list.AddRectFilled(node_rect_min, node_rect_max, node_bg_color, 4.0f);
            draw_list.AddRect(node_rect_min, node_rect_max, new Vector4(100, 100, 100, 255).ToUint(), 4.0f);
            for (int slot_idx = 0; slot_idx < node.InputsCount; slot_idx++)
                draw_list.AddCircleFilled(offset + node.GetInputSlotPos(slot_idx), NODE_SLOT_RADIUS, new Vector4(150, 150, 150, 150).ToUint());
            for (int slot_idx = 0; slot_idx < node.OutputsCount; slot_idx++)
                draw_list.AddCircleFilled(offset + node.GetOutputSlotPos(slot_idx), NODE_SLOT_RADIUS, new Vector4(150, 150, 150, 150).ToUint());

            ImGui.PopID();
            nodes[node_idx] = node;
        }
        draw_list.ChannelsMerge();

        // Open context menu
        if (ImGui.IsMouseReleased(1)) //ImGuiMouseButton.Right
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByPopup) || !ImGui.IsAnyItemHovered())
            {
                node_selected = node_hovered_in_list = node_hovered_in_scene = -1;
                open_context_menu = true;
            }
        if (open_context_menu)
        {
            ImGui.OpenPopup("context_menu");
            if (node_hovered_in_list != -1)
                node_selected = node_hovered_in_list;
            if (node_hovered_in_scene != -1)
                node_selected = node_hovered_in_scene;
        }

        // Draw context menu
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8, 8));
        if (ImGui.BeginPopup("context_menu"))
        {
            Node node = node_selected != -1 ? nodes[node_selected] : default;
            Vector2 scene_pos = ImGui.GetMousePosOnOpeningCurrentPopup() - offset;
            if (node.ID != -1)
            {
                ImGui.Text($"Node '{node.Name}'");
                ImGui.Separator();
                if (ImGui.MenuItem("Rename..", null, false, false)) { }
                if (ImGui.MenuItem("Delete", null, false, false)) { }
                if (ImGui.MenuItem("Copy", null, false, false)) { }
            }
            else
            {
                if (ImGui.MenuItem("Add")) { nodes.Add(new Node(nodes.Count, "New node", scene_pos, 0.5f, new Vector4(100, 100, 200, 255), 2, 2)); }
                if (ImGui.MenuItem("Paste", null, false, false)) { }
            }
            ImGui.EndPopup();
        }
        ImGui.PopStyleVar();

        // Scrolling
        if (ImGui.IsWindowHovered() && !ImGui.IsAnyItemActive() && ImGui.IsMouseDragging(2, 0.0f)) //ImGuiMouseButton_Middle
            scrolling = scrolling + io.MouseDelta;

        ImGui.PopItemWidth();
        ImGui.EndChild();
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
        ImGui.EndGroup();

        ImGui.End();
    }
}

static class Util
{
    public static uint ToUint(this Vector4 c)
    {
        return (uint)((((int)c.W << 24) | ((int)c.X << 16) | ((int)c.Y << 8) | (int)c.Z) & 0xffffffffL);
    }

    public static Vector4 ToColor(this uint value)
    {
        return new Vector4((byte)((value >> 24) & 0xFF),
            (byte)((value >> 16) & 0xFF),
            (byte)((value >> 8) & 0xFF),
            (byte) (value & 0xFF));
    }
}

/*

// Creating a node graph editor for Dear ImGui
// Quick sample, not production code! This is more of a demo of how to use Dear ImGui to create custom stuff.
// See https://github.com/ocornut/imgui/issues/306 for details
// And more fancy node editors: https://github.com/ocornut/imgui/wiki#Useful-widgets--references

// Changelog
// - v0.04 (2020-03): minor tweaks
// - v0.03 (2018-03): fixed grid offset issue, inverted sign of 'scrolling'

#include <math.h> // fmodf

// NB: You can use math functions/operators on ImVec2 if you #define IMGUI_DEFINE_MATH_OPERATORS and #include "imgui_internal.h"
// Here we only declare simple +/- operators so others don't leak into the demo code.
static inline ImVec2 operator+(const ImVec2& lhs, const ImVec2& rhs) { return ImVec2(lhs.x + rhs.x, lhs.y + rhs.y); }
static inline ImVec2 operator-(const ImVec2& lhs, const ImVec2& rhs) { return ImVec2(lhs.x - rhs.x, lhs.y - rhs.y); }

// Dummy data structure provided for the example.
// Note that we storing links as indices (not ID) to make example code shorter.
static void ShowExampleAppCustomNodeGraph(bool* opened)
{
    ImGui.SetNextWindowSize(ImVec2(700, 600), ImGuiCond_FirstUseEver);
    if (!ImGui.Begin("Example: Custom Node Graph", opened))
    {
        ImGui.End();
        return;
    }

    // Dummy
    struct Node
    {
        int     ID;
        char    Name[32];
        ImVec2  Pos, Size;
        float   Value;
        ImVec4  Color;
        int     InputsCount, OutputsCount;

        Node(int id, const char* name, const ImVec2& pos, float value, const ImVec4& color, int inputs_count, int outputs_count) { ID = id; strcpy(Name, name); Pos = pos; Value = value; Color = color; InputsCount = inputs_count; OutputsCount = outputs_count; }

        ImVec2 GetInputSlotPos(int slot_no) const { return ImVec2(Pos.x, Pos.y + Size.y * ((float)slot_no + 1) / ((float)InputsCount + 1)); }
        ImVec2 GetOutputSlotPos(int slot_no) const { return ImVec2(Pos.x + Size.x, Pos.y + Size.y * ((float)slot_no + 1) / ((float)OutputsCount + 1)); }
    };
    struct NodeLink
    {
        int     InputIdx, InputSlot, OutputIdx, OutputSlot;

        NodeLink(int input_idx, int input_slot, int output_idx, int output_slot) { InputIdx = input_idx; InputSlot = input_slot; OutputIdx = output_idx; OutputSlot = output_slot; }
    };

    // State
    static ImVector<Node> nodes;
    static ImVector<NodeLink> links;
    static ImVec2 scrolling = ImVec2(0.0f, 0.0f);
    static bool inited = false;
    static bool show_grid = true;
    static int node_selected = -1;

    // Initialization
    ImGuiIO& io = ImGui.GetIO();
    if (!inited)
    {
        nodes.push_back(Node(0, "MainTex", ImVec2(40, 50), 0.5f, ImColor(255, 100, 100), 1, 1));
        nodes.push_back(Node(1, "BumpMap", ImVec2(40, 150), 0.42f, ImColor(200, 100, 200), 1, 1));
        nodes.push_back(Node(2, "Combine", ImVec2(270, 80), 1.0f, ImColor(0, 200, 100), 2, 2));
        links.push_back(NodeLink(0, 0, 2, 0));
        links.push_back(NodeLink(1, 0, 2, 1));
        inited = true;
    }

    // Draw a list of nodes on the left side
    bool open_context_menu = false;
    int node_hovered_in_list = -1;
    int node_hovered_in_scene = -1;
    ImGui.BeginChild("node_list", ImVec2(100, 0));
    ImGui.Text("Nodes");
    ImGui.Separator();
    for (int node_idx = 0; node_idx < nodes.Size; node_idx++)
    {
        Node* node = &nodes[node_idx];
        ImGui.PushID(node.ID);
        if (ImGui.Selectable(node.Name, node.ID == node_selected))
            node_selected = node.ID;
        if (ImGui.IsItemHovered())
        {
            node_hovered_in_list = node.ID;
            open_context_menu |= ImGui.IsMouseClicked(1);
        }
        ImGui.PopID();
    }
    ImGui.EndChild();

    ImGui.SameLine();
    ImGui.BeginGroup();

    const float NODE_SLOT_RADIUS = 4.0f;
    const ImVec2 NODE_WINDOW_PADDING(8.0f, 8.0f);

    // Create our child canvas
    ImGui.Text("Hold middle mouse button to scroll (%.2f,%.2f)", scrolling.x, scrolling.y);
    ImGui.SameLine(ImGui.GetWindowWidth() - 100);
    ImGui.Checkbox("Show grid", &show_grid);
    ImGui.PushStyleVar(ImGuiStyleVar_FramePadding, ImVec2(1, 1));
    ImGui.PushStyleVar(ImGuiStyleVar_WindowPadding, ImVec2(0, 0));
    ImGui.PushStyleColor(ImGuiCol_ChildBg, IM_COL32(60, 60, 70, 200));
    ImGui.BeginChild("scrolling_region", ImVec2(0, 0), true, ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_NoMove);
    ImGui.PopStyleVar(); // WindowPadding
    ImGui.PushItemWidth(120.0f);

    const ImVec2 offset = ImGui.GetCursorScreenPos() + scrolling;
    ImDrawList* draw_list = ImGui.GetWindowDrawList();

    // Display grid
    if (show_grid)
    {
        ImU32 GRID_COLOR = IM_COL32(200, 200, 200, 40);
        float GRID_SZ = 64.0f;
        ImVec2 win_pos = ImGui.GetCursorScreenPos();
        ImVec2 canvas_sz = ImGui.GetWindowSize();
        for (float x = fmodf(scrolling.x, GRID_SZ); x < canvas_sz.x; x += GRID_SZ)
            draw_list.AddLine(ImVec2(x, 0.0f) + win_pos, ImVec2(x, canvas_sz.y) + win_pos, GRID_COLOR);
        for (float y = fmodf(scrolling.y, GRID_SZ); y < canvas_sz.y; y += GRID_SZ)
            draw_list.AddLine(ImVec2(0.0f, y) + win_pos, ImVec2(canvas_sz.x, y) + win_pos, GRID_COLOR);
    }

    // Display links
    draw_list.ChannelsSplit(2);
    draw_list.ChannelsSetCurrent(0); // Background
    for (int link_idx = 0; link_idx < links.Size; link_idx++)
    {
        NodeLink* link = &links[link_idx];
        Node* node_inp = &nodes[link.InputIdx];
        Node* node_out = &nodes[link.OutputIdx];
        ImVec2 p1 = offset + node_inp.GetOutputSlotPos(link.InputSlot);
        ImVec2 p2 = offset + node_out.GetInputSlotPos(link.OutputSlot);
        draw_list.AddBezierCurve(p1, p1 + ImVec2(+50, 0), p2 + ImVec2(-50, 0), p2, IM_COL32(200, 200, 100, 255), 3.0f);
    }

    // Display nodes
    for (int node_idx = 0; node_idx < nodes.Size; node_idx++)
    {
        Node* node = &nodes[node_idx];
        ImGui.PushID(node.ID);
        ImVec2 node_rect_min = offset + node.Pos;

        // Display node contents first
        draw_list.ChannelsSetCurrent(1); // Foreground
        bool old_any_active = ImGui.IsAnyItemActive();
        ImGui.SetCursorScreenPos(node_rect_min + NODE_WINDOW_PADDING);
        ImGui.BeginGroup(); // Lock horizontal position
        ImGui.Text("%s", node.Name);
        ImGui.SliderFloat("##value", &node.Value, 0.0f, 1.0f, "Alpha %.2f");
        ImGui.ColorEdit3("##color", &node.Color.x);
        ImGui.EndGroup();

        // Save the size of what we have emitted and whether any of the widgets are being used
        bool node_widgets_active = (!old_any_active && ImGui.IsAnyItemActive());
        node.Size = ImGui.GetItemRectSize() + NODE_WINDOW_PADDING + NODE_WINDOW_PADDING;
        ImVec2 node_rect_max = node_rect_min + node.Size;

        // Display node box
        draw_list.ChannelsSetCurrent(0); // Background
        ImGui.SetCursorScreenPos(node_rect_min);
        ImGui.InvisibleButton("node", node.Size);
        if (ImGui.IsItemHovered())
        {
            node_hovered_in_scene = node.ID;
            open_context_menu |= ImGui.IsMouseClicked(1);
        }
        bool node_moving_active = ImGui.IsItemActive();
        if (node_widgets_active || node_moving_active)
            node_selected = node.ID;
        if (node_moving_active && ImGui.IsMouseDragging(ImGuiMouseButton_Left))
            node.Pos = node.Pos + io.MouseDelta;

        ImU32 node_bg_color = (node_hovered_in_list == node.ID || node_hovered_in_scene == node.ID || (node_hovered_in_list == -1 && node_selected == node.ID)) ? IM_COL32(75, 75, 75, 255) : IM_COL32(60, 60, 60, 255);
        draw_list.AddRectFilled(node_rect_min, node_rect_max, node_bg_color, 4.0f);
        draw_list.AddRect(node_rect_min, node_rect_max, IM_COL32(100, 100, 100, 255), 4.0f);
        for (int slot_idx = 0; slot_idx < node.InputsCount; slot_idx++)
            draw_list.AddCircleFilled(offset + node.GetInputSlotPos(slot_idx), NODE_SLOT_RADIUS, IM_COL32(150, 150, 150, 150));
        for (int slot_idx = 0; slot_idx < node.OutputsCount; slot_idx++)
            draw_list.AddCircleFilled(offset + node.GetOutputSlotPos(slot_idx), NODE_SLOT_RADIUS, IM_COL32(150, 150, 150, 150));

        ImGui.PopID();
    }
    draw_list.ChannelsMerge();

    // Open context menu
    if (ImGui.IsMouseReleased(ImGuiMouseButton_Right))
        if (ImGui.IsWindowHovered(ImGuiHoveredFlags_AllowWhenBlockedByPopup) || !ImGui.IsAnyItemHovered())
        {
            node_selected = node_hovered_in_list = node_hovered_in_scene = -1;
            open_context_menu = true;
        }
    if (open_context_menu)
    {
        ImGui.OpenPopup("context_menu");
        if (node_hovered_in_list != -1)
            node_selected = node_hovered_in_list;
        if (node_hovered_in_scene != -1)
            node_selected = node_hovered_in_scene;
    }

    // Draw context menu
    ImGui.PushStyleVar(ImGuiStyleVar_WindowPadding, ImVec2(8, 8));
    if (ImGui.BeginPopup("context_menu"))
    {
        Node* node = node_selected != -1 ? &nodes[node_selected] : NULL;
        ImVec2 scene_pos = ImGui.GetMousePosOnOpeningCurrentPopup() - offset;
        if (node)
        {
            ImGui.Text("Node '%s'", node.Name);
            ImGui.Separator();
            if (ImGui.MenuItem("Rename..", NULL, false, false)) {}
            if (ImGui.MenuItem("Delete", NULL, false, false)) {}
            if (ImGui.MenuItem("Copy", NULL, false, false)) {}
        }
        else
        {
            if (ImGui.MenuItem("Add")) { nodes.push_back(Node(nodes.Size, "New node", scene_pos, 0.5f, ImColor(100, 100, 200), 2, 2)); }
            if (ImGui.MenuItem("Paste", NULL, false, false)) {}
        }
        ImGui.EndPopup();
    }
    ImGui.PopStyleVar();

    // Scrolling
    if (ImGui.IsWindowHovered() && !ImGui.IsAnyItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton_Middle, 0.0f))
        scrolling = scrolling + io.MouseDelta;

    ImGui.PopItemWidth();
    ImGui.EndChild();
    ImGui.PopStyleColor();
    ImGui.PopStyleVar();
    ImGui.EndGroup();

    ImGui.End();
}


*/