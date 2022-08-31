using System;
using System.Numerics;
using ImGuiNET;

namespace Example
{
	class Game
	{
		MemoryEditor me;
		TextEditor te;
		NodeEditor ne;
		Computer cp;
        private bool _quit;
        private bool show = true;

        public Game ()
		{
			me = new MemoryEditor();
			te = new TextEditor();
			ne = new NodeEditor();
			cp = new Computer();
		}

		public void Update()
		{
			ShowExampleAppMainMenuBar();
			me.Draw("Memory Editor", new byte[] {0,1,0,1,0,0,0,0,1}, 9, 0);
			te.imGuiUpdate();
			ne.ShowExampleAppCustomNodeGraph(ref show);
			cp.Update();
		}

		public bool ShouldQuit() => _quit;

		void ShowExampleAppMainMenuBar()
		{
			if (ImGui.BeginMainMenuBar())
			{
				if (ImGui.BeginMenu("File"))
				{
					ShowExampleMenuFile();
					ImGui.EndMenu();
				}
				if (ImGui.BeginMenu("Edit"))
				{
					if (ImGui.MenuItem("Undo", "CTRL+Z")) {}
					if (ImGui.MenuItem("Redo", "CTRL+Y", false, false)) {}  // Disabled item
					ImGui.Separator();
					if (ImGui.MenuItem("Cut", "CTRL+X")) {}
					if (ImGui.MenuItem("Copy", "CTRL+C")) {}
					if (ImGui.MenuItem("Paste", "CTRL+V")) {}
					ImGui.EndMenu();
				}
				ImGui.EndMainMenuBar();
			}
		}

		void ShowExampleMenuFile()
		{
			// IMGUI_DEMO_MARKER("Examples/Menu");
			ImGui.MenuItem("(demo menu)", null, false, false);
			if (ImGui.MenuItem("New")) {}
			if (ImGui.MenuItem("Open", "Ctrl+O")) {}
			if (ImGui.BeginMenu("Open Recent"))
			{
				ImGui.MenuItem("fish_hat.c");
				ImGui.MenuItem("fish_hat.inl");
				ImGui.MenuItem("fish_hat.h");
				if (ImGui.BeginMenu("More.."))
				{
					ImGui.MenuItem("Hello");
					ImGui.MenuItem("Sailor");
					if (ImGui.BeginMenu("Recurse.."))
					{
						ShowExampleMenuFile();
						ImGui.EndMenu();
					}
					ImGui.EndMenu();
				}
				ImGui.EndMenu();
			}
			if (ImGui.MenuItem("Save", "Ctrl+S")) {}
			if (ImGui.MenuItem("Save As..")) {}

			ImGui.Separator();
			// IMGUI_DEMO_MARKER("Examples/Menu/Options");
			if (ImGui.BeginMenu("Options"))
			{
				bool enabled = true;
				ImGui.MenuItem("Enabled", "", enabled);
				ImGui.BeginChild("child", new Vector2(0, 60), true);
				for (int i = 0; i < 10; i++)
					ImGui.Text($"Scrolling Text {i}");
				ImGui.EndChild();
				float f = 0.5f;
				int n = 0;
				ImGui.SliderFloat("Value", ref f, 0.0f, 1.0f);
				ImGui.InputFloat("Input", ref f, 0.1f);
				ImGui.Combo("Combo", ref n, "Yes\0No\0Maybe\0\0");
				ImGui.EndMenu();
			}

			// IMGUI_DEMO_MARKER("Examples/Menu/Colors");
			if (ImGui.BeginMenu("Colors"))
			{
				float sz = ImGui.GetTextLineHeight();
				for (int i = 0; i < (int)ImGuiCol.COUNT; i++)
				{
					string name = ImGui.GetStyleColorName((ImGuiCol)i);
					Vector2 p = ImGui.GetCursorScreenPos();
					ImGui.GetWindowDrawList().AddRectFilled(p, new Vector2(p.X + sz, p.Y + sz), ImGui.GetColorU32((ImGuiCol)i));
					ImGui.Dummy(new Vector2(sz, sz));
					ImGui.SameLine();
					ImGui.MenuItem(name);
				}
				ImGui.EndMenu();
			}

			// Here we demonstrate appending again to the "Options" menu (which we already created above)
			// Of course in this demo it is a little bit silly that this function calls BeginMenu("Options") twice.
			// In a real code-base using it would make senses to use this feature from very different code locations.
			// if (ImGui.BeginMenu("Options")) // <-- Append!
			// {
			// 	// IMGUI_DEMO_MARKER("Examples/Menu/Append to an existing menu");
			// 	bool b = true;
			// 	ImGui.Checkbox("SomeOption", ref b);
			// 	ImGui.EndMenu();
			// }

			if (ImGui.BeginMenu("Disabled", false)) // Disabled
			{
				// IM_ASSERT(0);
			}
			if (ImGui.MenuItem("Checked", null, true)) {}
			if (ImGui.MenuItem("Quit", "Alt+F4")) { _quit = true; }
		}
    }
}