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
        private static readonly Window win = new ("YarnNinja")
        {
            X = 0,
            Y = 1, // Leave one row for the toplevel menu

            // By using Dim.Fill(), it will automatically resize without manual intervention
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ColorScheme = Colors.TopLevel
        };

        private static YarnApplication? app = null;

        private static ListView? lstVewContainers = null;

        static async Task Main(string[] args)
        {
            Application.Init();
            var top = Application.Top;


            top.Add(win);

            // Creates a menubar, the item "New" has a help menu.
            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_File", new MenuItem [] {
                    new MenuItem ("_Open", "Yarn App Log", () => BtnOpen()),
                    new MenuItem ("_Close", "",null),
                    new MenuItem ("_Quit", "", () => BtnQuit ())
                }),
                new MenuBarItem ("_Help", new MenuItem [] {
                    new MenuItem ("_Documentation", "", null),
                    new MenuItem ("_Contribute", "", null),
                    new MenuItem ("_About", "", null)
                })
            });

            top.Add(menu);


            var statusBar = new StatusBar(Array.Empty<StatusItem>());

            top.Add(menu);

            top.Add(statusBar);


            if (args.Length > 0)
            {
                var path = args[0];

                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    return;
                }

                OpenAsync(File.ReadAllText(path));
            }

            Application.Run();
        }

        private static async Task RefreshYarnAppInfo()
        {
            var header = await BuildHeaderAsync();

            var workernodesWin = new Window("Workers")
            {
                X = 0,
                Y = 10,

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = Dim.Sized(10)
            };


            var lstVewWorkerNodes = new ListView()
            {
                X = 0,
                Y = 0,
                Height = Dim.Fill(),
                Width = Dim.Fill(),
                AllowsMarking = false,
                AllowsMultipleSelection = false
            };

            workernodesWin.Add(lstVewWorkerNodes);

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

            var workers = app.WorkerNodes.OrderBy(t => t).ToList();
            workers.Insert(0, "ALL");
            lstVewWorkerNodes.SetSource(workers);


            workernodesWin.Add(lstVewWorkerNodes);

            var containers = new Window("Containers")
            {
                X = 0,
                Y = 16,

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

            string applicatgionMasterId = "NA";

            if (app.ApplicationMaster is not null)
            {
                applicatgionMasterId = app.ApplicationMaster.Id;
            }


            lstVewContainers.SetSource(app.Containers.OrderBy(t => t.Id).Select(p =>
            {
                var postfix = "";
                if (p.Id.Equals(applicatgionMasterId))
                    postfix = "*";

                return $"{p.Id}{postfix}";
            }).ToList());


            var _scrollBarContainers = new ScrollBarView(lstVewContainers, true);

            _scrollBarContainers.ChangedPosition += () =>
                {
                    lstVewContainers.TopItem = _scrollBarContainers.Position;
                    if (lstVewContainers.TopItem != _scrollBarContainers.Position)
                    {
                        _scrollBar.Position = lstVewContainers.TopItem;
                    }
                    lstVewContainers.SetNeedsDisplay();
                };

            _scrollBarContainers.OtherScrollBarView.ChangedPosition += () =>
            {
                lstVewContainers.LeftItem = _scrollBarContainers.OtherScrollBarView.Position;
                if (lstVewContainers.LeftItem != _scrollBarContainers.OtherScrollBarView.Position)
                {
                    _scrollBarContainers.OtherScrollBarView.Position = lstVewContainers.LeftItem;
                }
                lstVewContainers.SetNeedsDisplay();
            };

            lstVewContainers.DrawContent += (e) =>
            {
                _scrollBarContainers.Size = lstVewContainers.Source.Count - 1;
                _scrollBarContainers.Position = lstVewContainers.TopItem;
                _scrollBarContainers.OtherScrollBarView.Size = lstVewContainers.Maxlength - 1;
                _scrollBarContainers.OtherScrollBarView.Position = lstVewContainers.LeftItem;
                _scrollBarContainers.Refresh();
            };


            lstVewWorkerNodes.SelectedItemChanged += LstVewWorkerNodes_SelectedItemChanged;
            lstVewContainers.OpenSelectedItem += OpenCountainer;



            win.Add(header);
            win.Add(workernodesWin);
            win.Add(containers);
        }



        private static void LstVewWorkerNodes_SelectedItemChanged(ListViewItemEventArgs obj)
        {
            string applicatgionMasterId = "NA";

            if (app.ApplicationMaster is not null)
            {
                applicatgionMasterId = app.ApplicationMaster.Id;
            }
            lstVewContainers.SetSource(app.Containers.Where(p => p.WorkerNode.Equals(obj.Value.ToString()) || obj.Value.Equals("ALL")).Select(p =>
            {
                var postfix = "";
                if (p.Id.Equals(applicatgionMasterId))
                    postfix = "*";

                return $"{p.Id}{postfix}";
            }).OrderBy(p => p).ToList());
        }

        private static void BtnQuit()
        {
            var n = MessageBox.Query(50, 7, "Quit Demo", "Are you sure you want to quit this YarnNinja?", "Yes", "No");
            if (n == 0) Application.RequestStop();
        }

        private static void BtnOpen()
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

                    OpenAsync(File.ReadAllText(path));
                }
            }

        }

        private static async Task OpenAsync(string initialText)
        {
            app = new YarnApplication(initialText);

            await RefreshYarnAppInfo();
        }


        private static async Task<FrameView> BuildHeaderAsync()
        {
            var header = new FrameView("Yarn Application Info")
            {
                X = Pos.Center(),
                Y = 0,
                Width = Dim.Percent(100),
                Height = Dim.Sized(10)
            };

            //Headder
            var dt = new DataTable();
            dt.Columns.Add("Application ID");
            dt.Columns.Add("Application Type");
            dt.Columns.Add("Start");
            dt.Columns.Add("Finish");
            dt.Columns.Add("Duration");
            dt.Columns.Add("Number of Containers");
            dt.Columns.Add("Status");

            var newRow = dt.NewRow();
            newRow[0] = app.Header.Id;
            newRow[1] = app.Header.Type;
            newRow[2] = app.Header.Start;
            newRow[3] = app.Header.Finish;
            newRow[4] = app.Header.Duration.ToString(@"hh\:mm\:ss");
            newRow[5] = app.Containers.Count;
            newRow[6] = app.Header.Status.ToString();
            dt.Rows.Add(newRow);

            var table1 = new TableView()
            {
                Table = dt,
                Width = Dim.Fill(),
                Height = Dim.Sized(4),
            };
            table1.Style.ExpandLastColumn = true;
            header.Add(table1);


            dt = new DataTable();
            if (app.Header.Type == YarnApplicationType.Tez)
            {
                dt.Columns.Add("DAG");
            }
            else if (app.Header.Type == YarnApplicationType.MapReduce)
            {
                dt.Columns.Add("Mappers/Reducers");
            }
            dt.Columns.Add("User");
            dt.Columns.Add("Queue");


            newRow = dt.NewRow();
            if (app.Header.Type == YarnApplicationType.Tez)
            {
                newRow[0] = $"Submitted: {app.Header.SubmittedDags}, Successfull: {app.Header.SuccessfullDags}, Failed: {app.Header.FailedDags}, Killed: {app.Header.KilledDags}";
            }
            else if (app.Header.Type == YarnApplicationType.MapReduce)
            {
                newRow[0] = $"Completed Mappers: {app.Header.CompletedMappers}, Completed Reducers: {app.Header.CompletedReducers}";
            }

            newRow[1] = app.Header.User;
            newRow[2] = app.Header.QueueName;

            dt.Rows.Add(newRow);

            var table2 = new TableView()
            {
                Table = dt,
                X = Pos.X(table1),
                Y = Pos.Bottom(table1),
                Width = Dim.Fill(),
                Height = Dim.Sized(4),
            };
            table2.Style.ExpandLastColumn = true;
            header.Add(table2);

            return header;
        }



        #region Container Dialog

        private static void OpenCountainer(ListViewItemEventArgs obj)
        {
            var container = app.Containers.Where(p => obj.Value.ToString().StartsWith(p.Id)).FirstOrDefault();

            if (container is null)
            {
                Application.RequestStop();
            }

            var buttons = new List<Button>();

            var button = new Button("Close", is_default: true);

            button.Clicked += () =>
            {
                Application.RequestStop();
            };

            buttons.Add(button);

            var dialog = new Dialog($"Container :{container.Id}", 0, 0, buttons.ToArray());


            var header = new Window("Container Header")
            {
                X = Pos.Center(),
                Y = 0,
                Width = Dim.Percent(100),
                Height = Dim.Sized(3)
            };


            var startLable = new Label("Start:")
            {
                X = 0,
                Y = 0,
            };

            header.Add(startLable);

            var startValue = new Label()
            {
                X = Pos.Right(startLable) + 1,
                Y = Pos.Y(startLable),
                Width = Dim.Sized(container.Start.ToString().Length),
                Height = 1,
                ColorScheme = Colors.TopLevel,
                Text = container.Start.ToString()
            };
            header.Add(startValue);

            var finishLable = new Label("Finish:")
            {
                X = Pos.Right(startValue) + 1,
                Y = Pos.Y(startLable),
            };

            header.Add(finishLable);
            var finsihValue = new Label()
            {
                X = Pos.Right(finishLable) + 1,
                Y = Pos.Y(finishLable),
                Width = Dim.Sized(container.Finish.ToString().Length),
                Height = 1,
                ColorScheme = Colors.TopLevel,
                Text = container.Finish.ToString()
            };
            header.Add(finsihValue);

            var durationLable = new Label("Duration:")
            {
                X = Pos.Right(finsihValue) + 1,
                Y = Pos.Y(startLable),
            };

            header.Add(durationLable);
            var durationValue = new Label()
            {
                X = Pos.Right(durationLable) + 1,
                Y = Pos.Y(durationLable),
                Width = Dim.Sized(container.Duration.ToString(@"hh\:mm\:ss").Length),
                Height = 1,
                ColorScheme = Colors.TopLevel,
                Text = container.Duration.ToString(@"hh\:mm\:ss")
            };
            header.Add(durationValue);

            dialog.Add(header);

            var logTypesWin = new Window("Log Types")
            {
                X = 0,
                Y = 4,
                Width = Dim.Percent(15),
                Height = 24
            };

            var lstLogTypes = new ListView()
            {
                X = 0,
                Y = 0,
                Height = Dim.Fill(),
                Width = Dim.Fill(),
                AllowsMarking = false,
                AllowsMultipleSelection = false
            };

            logTypesWin.Add(lstLogTypes);

            var _scrollBar = new ScrollBarView(lstLogTypes, true);

            _scrollBar.ChangedPosition += () =>
            {
                lstLogTypes.TopItem = _scrollBar.Position;
                if (lstLogTypes.TopItem != _scrollBar.Position)
                {
                    _scrollBar.Position = lstLogTypes.TopItem;
                }
                lstLogTypes.SetNeedsDisplay();
            };

            _scrollBar.OtherScrollBarView.ChangedPosition += () =>
            {
                lstLogTypes.LeftItem = _scrollBar.OtherScrollBarView.Position;
                if (lstLogTypes.LeftItem != _scrollBar.OtherScrollBarView.Position)
                {
                    _scrollBar.OtherScrollBarView.Position = lstLogTypes.LeftItem;
                }
                lstLogTypes.SetNeedsDisplay();
            };

            lstLogTypes.DrawContent += (e) =>
            {
                _scrollBar.Size = lstLogTypes.Source.Count - 1;
                _scrollBar.Position = lstLogTypes.TopItem;
                _scrollBar.OtherScrollBarView.Size = lstLogTypes.Maxlength - 1;
                _scrollBar.OtherScrollBarView.Position = lstLogTypes.LeftItem;
                _scrollBar.Refresh();
            };

            var logtypes = container.Logs.Select(p => p.YarnLogType).Distinct().ToList();

            lstLogTypes.SetSource(logtypes);
            lstLogTypes.SelectedItemChanged += (obj) => {
                LstLogTypes_SelectedItemChanged(container, obj.Value.ToString());
            }  ;

            var logContentWin = new Window("Log Content")
            {
                X = Pos.Right(logTypesWin),
                Y = 4,
                Width = Dim.Fill(),
                Height = 24
            };
            dialog.Add(logTypesWin);

            logContentTxt = new TextView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),

                Height = Dim.Fill()
            };

            logContentWin.Add(logContentTxt);

            dialog.Add(logContentWin);

            lstLogTypes.FocusFirst();

            Application.Run(dialog);
        }

        static TextView logContentTxt;

        private static void LstLogTypes_SelectedItemChanged(YarnApplicationContainer container, string selectedLogType)
        {

            logContentTxt.Text = container.Logs.FirstOrDefault(p => p.YarnLogType == selectedLogType).LogText;
        }


        #endregion
    }
}