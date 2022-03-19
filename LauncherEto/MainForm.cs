using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text.Json;
using Eto.Forms;
using Eto.Drawing;

namespace LauncherEto
{
	public static class Util
    {
		//wnck_window_is_minimized
		// GdkPixbuf*  wnck_window_get_icon            (WnckWindow *window);

		//Icon.FromResource
        public static ListItem listItem(string i)
        {
            var item = new ListItem();
            item.Text = i;
            return item;
        }
        public static void makeFileList(string path, Action<ListItem> f)
        {
            var pathList = File.ReadAllLines(path);
            foreach (var item in pathList)
            {
                f(listItem(item));
            }
        }
    }
	public partial class MainForm : Form
	{
		string FirstCurrent;
		string QsavePath = "QSave.txt";
		string Filer = "";
		ListBox list = new ListBox();
		void StartProc(string fileName)
		{
			if (fileName != "")
			{
				if(Directory.Exists(fileName))
				{
					var parent = Directory.GetParent(fileName).FullName;
					Environment.CurrentDirectory = parent;
					var proc = new ProcessStartInfo(Filer);
					var pr = Process.Start(proc);
					Environment.CurrentDirectory = FirstCurrent;
				}
				else if (!fileName.StartsWith("--"))
                {
                    var proc = new ProcessStartInfo("xfce4-terminal", "-x " + fileName);
                    proc.CreateNoWindow = true;
                    proc.UseShellExecute = true;
                    Process.Start(proc);
                }
			}
			else
            {
                var proc = new ProcessStartInfo(Filer , Environment.CurrentDirectory);
                Process.Start(proc);
            }
		}
		void OnClick(MouseEventArgs e){
			var pos = list.PointFromScreen(Mouse.Position);
			var ind = (int)(pos.Y / 20.0f);
			if(ind < list.Items.Count)
			{
				var item = list.Items[ind];
				var v = item.Text;
				if(Mouse.Buttons == MouseButtons.Primary)
				{
					StartProc(v);
				}
				else if(Mouse.Buttons == MouseButtons.Alternate)
				{
					Clipboard.Instance.Text = v;
				}
				else
				{
					var parent = Directory.GetParent(v).FullName;
					StartProc(parent);
				}
			}
        }
        void AddFromCB()
        {
            list.Items.Add(Clipboard.Instance.Text);
			File.WriteAllLines(QsavePath , list.Items.Select( i=> i.Text ) );
        }

		public MainForm()
		{
			Title = "My Eto Form";
			MinimumSize = new Size(600, 600);
			var layout = new TableLayout(1,2);
			layout.SetColumnScale(0);
			layout.SetColumnScale(0);
			layout.SetRowScale(0,false);
			layout.SetRowScale(1);
			Content = layout;

			string appPath = Directory.GetParent( System.Reflection.Assembly.GetExecutingAssembly().Location).FullName;
			Environment.CurrentDirectory = appPath;
			FirstCurrent = appPath;
			var json = JsonDocument.Parse(File.ReadAllText ("LauncherSetting.txt"));
			var filer = json.RootElement.GetProperty("filer").GetString();
			Filer = filer;
			Util.makeFileList(QsavePath, list.Items.Add);
			layout.Add(list,0,1);
			list.MouseDown += (sender,e)=> OnClick(e);
			// create a few commands that can be used for the menu and toolbar
			var clickMe = new Command { MenuText = "Click Me!", ToolBarText = "Click Me!" };
			clickMe.Executed += (sender, e) => MessageBox.Show(this, "I was clicked!");
			var addCBCom = new Command { MenuText = "AddCB", ToolBarText = "AddCB!" };
			addCBCom.Executed += (sender, e) => AddFromCB();

			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			var aboutCommand = new Command { MenuText = "About..." };
			aboutCommand.Executed += (sender, e) => new AboutDialog().ShowDialog(this);
			var addFromCB = new ButtonMenuItem{ Text = "&AddFromCB", Items = { addCBCom} };
			// create menu
			Menu = new MenuBar
			{
				Items =
				{
					// File submenu
					new SubMenuItem { Text = "&File", Items = { clickMe } },
					addFromCB,
				},
				// ApplicationItems =
				// {
				// 	new ButtonMenuItem { Text = "&Preferences..." },
				// },
				// QuitItem = quitCommand,
				// AboutItem = aboutCommand
			};

			// create toolbar			
			//ToolBar = new ToolBar { Items = { clickMe } };
		}
	}
}
