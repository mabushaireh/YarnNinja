using System.Linq;
using System.Windows.Input;
using YarnNinja.App.WinApp.Models;
using YarnNinja.Common;
using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

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
                    return $"Submitted: {YarnApp.Header.SubmittedDags}, Successfull: {YarnApp.Header.SuccessfullDags}, Failed: {YarnApp.Header.FailedDags}, Killed: {YarnApp.Header.KilledTasksCount}";
                else if (YarnApp.Header.Type == YarnApplicationType.MapReduce)
                    return $"Completed Mappers: {YarnApp.Header.CompletedMappers}, Completed Reducers: {YarnApp.Header.CompletedReducers}";
                else if (YarnApp.Header.Type == YarnApplicationType.Spark)
                    return $"Task: {YarnApp.Header.TasksCount}, Completed: {YarnApp.Header.SuccessTasksCount}, Killed: {YarnApp.Header.KilledTasksCount}, Failed: {YarnApp.Header.FailedTasksCount}";
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
                var containerId = AppState.GetStateFor(StatePurpose.SelectedContainer, this.YarnApp.Header.Id);
                return !string.IsNullOrEmpty(containerId);
            }
        }

        public YarnApplicationContainer CurrentContainer
        {
            get
            {
                var containerId = AppState.GetStateFor(StatePurpose.SelectedContainer, this.YarnApp.Header.Id);

                return (containerId != "" ? YarnApp.Containers.Where(p => p.Id == containerId).FirstOrDefault() : null);
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
                return AppState.GetStateFor(StatePurpose.QueryText, this.YarnApp.Header.Id);
            }
            set
            {
                AppState.SetStateFor(StatePurpose.QueryText, value, this.YarnApp.Header.Id);

                OnPropertyChanged(nameof(QueryText));
            }
        }
    }
}
