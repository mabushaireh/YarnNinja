using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using YarnNinja.Common;

namespace YarnNinja.App.WinApp.ViewModels
{
    public partial class YarnAppContainerPageViewModel : ObservableRecipient
    {
        public YarnApplicationContainer YarnAppContainer { get; set; }
        public YarnAppContainerPageViewModel()
        {

        }

        private string current;
        public bool HasCurrent => current is not null;

        public string Current
        {
            get => current;
            set
            {
                SetProperty(ref current, value);
                OnPropertyChanged(nameof(HasCurrent));
            }
        }

        private bool showErrors = true;
        public bool ShowErrors
        {
            get => showErrors;
            set
            {
                SetProperty(ref showErrors, value);
            }
        }

        private bool showWarnings = true;
        public bool ShowWarnings
        {
            get => showWarnings;
            set
            {
                SetProperty(ref showWarnings, value);
            }
        }

        private bool showInfo = true;
        public bool ShowInfo
        {
            get => showInfo;
            set
            {
                SetProperty(ref showInfo, value);
            }
        }

        private bool showDebug = true;
        public bool ShowDebug
        {
            get => showDebug;
            set
            {
                SetProperty(ref showDebug, value);
            }
        }

        private bool showVerbos = true;
        public bool ShowVerbose
        {
            get => showVerbos;
            set
            {
                SetProperty(ref showVerbos, value);
            }
        }

        public List<string> LogTypes
        {
            get
            {
                return YarnAppContainer.Logs.Select(p => p.YarnLogType).Distinct().OrderBy(p => p).ToList();
            }
        }

        private YarnApplicationLogLine currentContainerLogLine;
        public bool HasCurrentContainerLogLine => currentContainerLogLine is not null;

        public YarnApplicationLogLine CurrentContainerLogLine
        {
            get => currentContainerLogLine;
            set
            {
                SetProperty(ref currentContainerLogLine, value);
                OnPropertyChanged(nameof(HasCurrentContainerLogLine));
            }
        }

        public List<YarnApplicationLogLine> ContainersLogTypeLines
        {
            get
            {
                if (!HasCurrent)
                    return new List<YarnApplicationLogLine>();

                var containersLogTypeLines = YarnAppContainer.GetLogsByType(Current).Where(p => p.Msg.Contains(QueryText)).ToList();

                //Filter based on types
                if (!ShowErrors)
                    containersLogTypeLines = containersLogTypeLines.Where(p => p.TraceLevel != TraceLevel.ERROR).ToList();
                if (!ShowWarnings)
                    containersLogTypeLines = containersLogTypeLines.Where(p => p.TraceLevel != TraceLevel.WARN).ToList();
                if (!ShowInfo)
                    containersLogTypeLines = containersLogTypeLines.Where(p => p.TraceLevel != TraceLevel.INFO).ToList();
                if (!ShowDebug)
                    containersLogTypeLines = containersLogTypeLines.Where(p => p.TraceLevel != TraceLevel.DEBUG).ToList();



                return containersLogTypeLines;
            }

            set { }
        }

        private string queryText = string.Empty;
        public string QueryText { get { return queryText; } set {
                SetProperty(ref queryText, value);
            } }

        protected override void OnActivated()
        {
            base.OnActivated();

            // Does not see the messages with a token.


        }
    }
}
