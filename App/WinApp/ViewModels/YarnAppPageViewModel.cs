﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging; // Hosts the 'Register' extension method without token
using Microsoft.Toolkit.Mvvm.Input;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using YarnNinja.App.WinApp.Models;
using YarnNinja.App.WinApp.Services;
using YarnNinja.Common;
using System;
using System.Collections.Generic;

namespace YarnNinja.App.WinApp.ViewModels
{
    public partial class YarnAppPageViewModel : ObservableRecipient
    {
        private string current;

        public YarnApplication YarnApp { get; set; }
        public YarnAppPageViewModel()
        {

        }

        public string YarnType
        {
            get
            {
                return YarnApp.Header.Type.ToString();
            }
        }

        public string Start
        {
            get
            {
                return YarnApp.Header.Start.ToString("yyyy-MM-dd HH:mm:ss,fff");
            }
        }

        public string Finish
        {
            get
            {
                return YarnApp.Header.Finish.ToString("yyyy-MM-dd HH:mm:ss,fff");
            }
        }

        public string Duration
        {
            get
            {
                return YarnApp.Header.Duration.ToString(@"hh\:mm\:ss");
            }
        }

        public int ContainersCount
        {
            get
            {
                return YarnApp.Containers.Count;
            }
        }

        public YarnApplicationStatus Status
        {
            get
            {
                return YarnApp.Header.Status;
            }
        }

        public string DagMappersReduces
        {
            get
            {
                if (YarnApp.Header.Type == YarnApplicationType.Tez)
                    return $"Submitted: {YarnApp.Header.SubmittedDags}, Successfull: {YarnApp.Header.SuccessfullDags}, Failed: {YarnApp.Header.FailedDags}, Killed: {YarnApp.Header.KilledDags}";
                else if (YarnApp.Header.Type == YarnApplicationType.MapReduce)
                    return $"Completed Mappers: {YarnApp.Header.CompletedMappers}, Completed Reducers: {YarnApp.Header.CompletedReducers}";
                else
                {
                    return "Not Implemented Yet";
                }
            }
        }

        public string User
        {
            get
            {
                return YarnApp.Header.User;
            }
        }

        public string Queue
        {
            get
            {
                return YarnApp.Header.QueueName;
            }
        }

        public List<string> WorkerNodes
        {
            get
            {
                var workers = YarnApp.WorkerNodes.OrderBy(t => t).ToList();
                workers.Insert(0, "ALL");
                return workers;
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            // Does not see the messages with a token.


        }

        public bool HasCurrentWorkerNode
        {
            get
            {
                try
                {
                    return !string.IsNullOrEmpty(AppState.GetStateFor(StatePurpose.SelectedWorkerNode, this.YarnApp.Header.Id));
                }
                catch
                {
                    return false;
                }
            }
        }

        public string CurrentWorkerNode
        {
            get => AppState.GetStateFor(StatePurpose.SelectedWorkerNode, this.YarnApp.Header.Id);
            set
            {
                try
                {
                    if (value == CurrentWorkerNode)
                        return;
                }
                catch { }
                

                AppState.SetStateFor(StatePurpose.SelectedWorkerNode, value, this.YarnApp.Header.Id);
                OnPropertyChanged(nameof(CurrentWorkerNode));
                OnPropertyChanged(nameof(HasCurrentWorkerNode));
            }
        }

        public bool HasCurrentContainer
        {
            get
            {
                try
                {
                    return !string.IsNullOrEmpty(AppState.GetStateFor(StatePurpose.SelectedContainer, this.YarnApp.Header.Id));
                }
                catch
                {
                    return false;
                }
            }
        }

        public YarnApplicationContainer CurrentContainer
        {
            get
            {
                try
                {
                    var containerId = AppState.GetStateFor(StatePurpose.SelectedContainer, this.YarnApp.Header.Id);

                    return YarnApp.Containers.Where(p => p.Id == containerId).FirstOrDefault();
                }
                catch
                {
                    return null;
                }
            }

            set
            {
                if (value == null)
                    return;

                AppState.SetStateFor(StatePurpose.SelectedContainer, value.Id, this.YarnApp.Header.Id);
                OnPropertyChanged(nameof(CurrentContainer));
                OnPropertyChanged(nameof(HasCurrentContainer));
            }
        }
        public List<YarnApplicationContainer> Containers
        {
            get
            {
                if (!HasCurrentWorkerNode)
                    return new List<YarnApplicationContainer>();

                var conainers = YarnApp.Containers.OrderBy(t => t.Order).Where(p => (CurrentWorkerNode == "ALL" || p.WorkerNode == CurrentWorkerNode) && p.Id.Contains(QueryText)).ToList();

                return conainers;
            }

            set { }
        }

        public string QueryText
        {
            get
            {
                try
                {
                    return AppState.GetStateFor(StatePurpose.QueryText, this.YarnApp.Header.Id);

                }
                catch
                {
                    return string.Empty;
                }
            }
            set
            {
                AppState.SetStateFor(StatePurpose.QueryText, value, this.YarnApp.Header.Id);

                OnPropertyChanged(nameof(QueryText));
            }
        }
    }
}
