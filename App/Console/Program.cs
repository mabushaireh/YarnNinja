using System;

using Terminal.Gui;
using NStack;

using YarnNinja.Common.Core;
using System.Data;
using YarnNinja.Common.Utils;
using YarnNinja.Common;
using System.Collections;

namespace YarnNinja.App.Console
{
    internal enum ApplicationOptions
    {
        Exit = 0,
        Containers = 1,
    }
    class Program
    {
        private static string yarnLogText = "";
        private static Window win = new Window("YarnNinja")
        {
            X = 0,
            Y = 1, // Leave one row for the toplevel menu

            // By using Dim.Fill(), it will automatically resize without manual intervention
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        private static YarnApplication app = null;
        private static ListView lstVewContainers = null;

        static async Task Main(string[] args)
        {
            Application.Init();
            var top = Application.Top;


            top.Add(win);

            // Creates a menubar, the item "New" has a help menu.
            var menu = new MenuBar(new MenuBarItem[] {
            new MenuBarItem ("_File", new MenuItem [] {
                new MenuItem ("_Open", "Yarn App Log", () => Open()),
                new MenuItem ("_Close", "",null),
                new MenuItem ("_Quit", "", () => Quit ())
            }),
            new MenuBarItem ("_Help", new MenuItem [] {
                new MenuItem ("_Documentation", "", null),
                new MenuItem ("_Contribute", "", null),
                new MenuItem ("_About", "", null)
            })
        });
            top.Add(menu);


            var statusBar = new StatusBar(new StatusItem[] {

            });

            top.Add(menu);

            top.Add(statusBar);

            if (args.Length > 0)
            {
                var path = args[0];

                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    return;
                }

                OpenAsync(File.ReadAllText(path), new FileInfo(path), Path.GetFileName(path));
            }
            Application.Run();


        }

        static async Task RefreshYarnAppInfo()
        {
            var header = new Window("Yarn Application Info")
            {
                X = 0,
                Y = 1, // Leave one row for the toplevel menu

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = Dim.Sized(7)
            };


            var tableView = new TableView()
            {
                Table = await BuildHeaderAsync(2, 2),
                MultiSelect = true,
                Width = Dim.Fill(),
                Height = Dim.Fill(1),
            };
            tableView.Style.ExpandLastColumn = false;


            header.Add(tableView);

            var workernodes = new Window("Workers")
            {
                X = 0,
                Y = 8,

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

           

            var lstVewWorkerNodes = new ListView()
            {
                X = 0,
                Y = 0,
                Height = Dim.Fill(),
                Width = Dim.Sized(10),
                AllowsMarking = false,
                AllowsMultipleSelection = false
            };

            workernodes.Add(lstVewWorkerNodes);

            var _scrollBar = new ScrollBarView(lstVewWorkerNodes, true);

            _scrollBar.ChangedPosition += () =>
            {
                lstVewWorkerNodes.TopItem = _scrollBar.Position;
                if (lstVewWorkerNodes.TopItem != _scrollBar.Position)
                {
                    _scrollBar.Position = lstVewWorkerNodes.TopItem;
                }
                lstVewWorkerNodes.SetNeedsDisplay();
            };

            _scrollBar.OtherScrollBarView.ChangedPosition += () =>
            {
                lstVewWorkerNodes.LeftItem = _scrollBar.OtherScrollBarView.Position;
                if (lstVewWorkerNodes.LeftItem != _scrollBar.OtherScrollBarView.Position)
                {
                    _scrollBar.OtherScrollBarView.Position = lstVewWorkerNodes.LeftItem;
                }
                lstVewWorkerNodes.SetNeedsDisplay();
            };

            lstVewWorkerNodes.DrawContent += (e) =>
            {
                _scrollBar.Size = lstVewWorkerNodes.Source.Count - 1;
                _scrollBar.Position = lstVewWorkerNodes.TopItem;
                _scrollBar.OtherScrollBarView.Size = lstVewWorkerNodes.Maxlength - 1;
                _scrollBar.OtherScrollBarView.Position = lstVewWorkerNodes.LeftItem;
                _scrollBar.Refresh();
            };

            lstVewWorkerNodes.SetSource((await BuildWorkerNodesTableAsync()).OrderBy(t => t).ToList());

            lstVewWorkerNodes.SelectedItemChanged += LstVewWorkerNodes_SelectedItemChanged;
            workernodes.Add(lstVewWorkerNodes);

            var containers = new Window("Containers")
            {
                X = 0,
                Y = 20,

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            lstVewContainers = new ListView()
            {
                X = 0,
                Y = 0,
                Height = Dim.Fill(),
                Width = Dim.Fill(),
                AllowsMarking = false,
                AllowsMultipleSelection = false
            };

            containers.Add(lstVewContainers);

            win.Add(header);
            win.Add(workernodes);
            win.Add(containers);
        }

        private static void LstVewWorkerNodes_SelectedItemChanged(ListViewItemEventArgs obj)
        {
            var _scrollBar = new ScrollBarView(lstVewContainers, true);

            _scrollBar.ChangedPosition += () =>
            {
                lstVewContainers.TopItem = _scrollBar.Position;
                if (lstVewContainers.TopItem != _scrollBar.Position)
                {
                    _scrollBar.Position = lstVewContainers.TopItem;
                }
                lstVewContainers.SetNeedsDisplay();
            };

            _scrollBar.OtherScrollBarView.ChangedPosition += () =>
            {
                lstVewContainers.LeftItem = _scrollBar.OtherScrollBarView.Position;
                if (lstVewContainers.LeftItem != _scrollBar.OtherScrollBarView.Position)
                {
                    _scrollBar.OtherScrollBarView.Position = lstVewContainers.LeftItem;
                }
                lstVewContainers.SetNeedsDisplay();
            };

            lstVewContainers.DrawContent += (e) =>
            {
                _scrollBar.Size = lstVewContainers.Source.Count - 1;
                _scrollBar.Position = lstVewContainers.TopItem;
                _scrollBar.OtherScrollBarView.Size = lstVewContainers.Maxlength - 1;
                _scrollBar.OtherScrollBarView.Position = lstVewContainers.LeftItem;
                _scrollBar.Refresh();
            };

            lstVewContainers.SetSource((BuildWorkerNodesTableAsync().Result).OrderBy(t => t).ToList());
        }

        private static async Task<List<string>> BuildWorkerNodesTableAsync()
        {
            var workernodes = new List<string>();

            foreach (var item in app.Containers)
            {
                if (!workernodes.Any(p => p.Equals(item.WorkerNode)))
                { 
                    workernodes.Add(item.WorkerNode);
                }
            }

            return workernodes;

        }

        static void Quit()
        {
            var n = MessageBox.Query(50, 7, "Quit Demo", "Are you sure you want to quit this YarnNinja?", "Yes", "No");
            if (n == 0) Application.RequestStop();
        }

        static void Open()
        {

            var open = new OpenDialog("Open", "Open a file") { AllowsMultipleSelection = true };

            Application.Run(open);

            if (!open.Canceled)
            {

                foreach (var path in open.FilePaths)
                {

                    if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    {
                        return;
                    }

                    OpenAsync(File.ReadAllText(path), new FileInfo(path), Path.GetFileName(path));
                }
            }

        }

        static async Task OpenAsync(string initialText, FileInfo fileInfo, string tabName)
        {
            yarnLogText = initialText;
            await RefreshYarnAppInfo();
        }


        public static async Task<DataTable> BuildHeaderAsync(int cols, int rows)
        {
            var dt = new DataTable();
            dt.Columns.Add("Application ID");
            dt.Columns.Add("Application Type");
            dt.Columns.Add("Number of Containers");


            var newRow = dt.NewRow();
            app = await YarnLogParser.Parse(yarnLogText);

            newRow[0] = app.Header.Id;
            newRow[1] = app.Header.Type;
            newRow[2] = app.Containers.Count;

            dt.Rows.Add(newRow);

            return dt;
        }
    }
}