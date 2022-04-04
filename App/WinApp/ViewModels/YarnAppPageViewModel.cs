using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging; // Hosts the 'Register' extension method without token
using Microsoft.Toolkit.Mvvm.Input;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using YarnNinja.App.WinApp.Models;
using YarnNinja.App.WinApp.Services;
using YarnNinja.Common;
using System;
using YarnNinja.Common.Core;
using System.Collections.Generic;

namespace YarnNinja.App.WinApp.ViewModels
{
    public partial class YarnAppPageViewModel : ObservableRecipient
    {
        private WorkerNode current;

        public YarnApplication YarnApp { get; set; }
        public YarnAppPageViewModel()
        {
            
        }

        public string YarnType { get { 
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

        public List<WorkerNode> WorkerNodes
        {
            get
            {
                var workers = YarnApp.WorkerNodes.OrderBy(t => t).ToList().Select( p => new WorkerNode() { Name=p}).ToList();
                workers.Insert(0, new WorkerNode { Name = "ALL" });
                return workers;
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            // Does not see the messages with a token.
            

        }

        public bool HasCurrent => current is not null;

        public WorkerNode Current
        {
            get => current;
            set
            {
                SetProperty(ref current, value);
                OnPropertyChanged(nameof(HasCurrent));
            }
        }

        private YarnApplicationContainer currentContainer;
        public bool HasCurrentContainer => currentContainer is not null;

        public YarnApplicationContainer CurrentContainer
        {
            get => currentContainer;
            set
            {
                SetProperty(ref currentContainer, value);
                OnPropertyChanged(nameof(HasCurrentContainer));
            }
        }
        public List<YarnApplicationContainer> Containers
        {
            get
            {
                if (!HasCurrent)
                    return new List<YarnApplicationContainer>();

                var conainers = YarnApp.Containers.OrderBy(t => t.Order).Where(p => (current.Name == "ALL" || p.WorkerNode == current.Name) && p.Id.Contains(QueryText)).ToList();

                return conainers;
            }

            set { }
        }

        private string queryText = string.Empty;
        public string QueryText
        {
            get { return queryText; }
            set
            {
                SetProperty(ref queryText, value);
            }
        }
    }
}
