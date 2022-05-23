using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using YarnNinja.App.WinApp.Models;
using YarnNinja.Common;

namespace YarnNinja.App.WinApp.ViewModels
{
    public partial class YarnAppContainerPageViewModel : ObservableRecipient
    {
        public YarnApplicationContainer YarnAppContainer { get; set; }
        public YarnAppContainerPageViewModel()
        {

        }

        public bool HasCurrent
        {
            get
            {
                try
                {
                    return !string.IsNullOrEmpty(AppState.GetStateFor(StatePurpose.SelectedLogType, this.YarnAppContainer.Id));
                }
                catch
                {
                    return false;
                }
            }
        }

        public string Current
        {
            get => AppState.GetStateFor(StatePurpose.SelectedLogType, this.YarnAppContainer.Id);
            set
            {
                try
                {
                    if (value == Current)
                        return;
                }
                catch { }


                AppState.SetStateFor(StatePurpose.SelectedLogType, value, this.YarnAppContainer.Id);

                OnPropertyChanged(nameof(Current));
                OnPropertyChanged(nameof(HasCurrent));
            }
        }

        public bool ShowErrors
        {
            get
            {
                var isShowError = AppState.GetStateFor(StatePurpose.IsShowError, this.YarnAppContainer.Id);
                if (string.IsNullOrEmpty(isShowError))
                    return true;
                else
                    return bool.Parse(isShowError);
            }
            set
            {
                try
                {
                    if (value == ShowErrors)
                        return;
                }
                catch { }


                AppState.SetStateFor(StatePurpose.IsShowError, value.ToString(), this.YarnAppContainer.Id);
                OnPropertyChanged(nameof(ShowErrors));
            }
        }

        public bool ShowWarnings
        {
            get
            {
                var isShowWarnings = AppState.GetStateFor(StatePurpose.IsShowWarnings, this.YarnAppContainer.Id);
                if (string.IsNullOrEmpty(isShowWarnings))
                    return true;
                else
                    return bool.Parse(isShowWarnings);
            }
            set
            {
                try
                {
                    if (value == ShowWarnings)
                        return;
                }
                catch { }


                AppState.SetStateFor(StatePurpose.IsShowWarnings, value.ToString(), this.YarnAppContainer.Id);
                OnPropertyChanged(nameof(ShowWarnings));
            }
        }

        public bool ShowInfo
        {
            get
            {
                var isShowInfo = AppState.GetStateFor(StatePurpose.IsShowInfo, this.YarnAppContainer.Id);
                if (string.IsNullOrEmpty(isShowInfo))
                    return true;
                else
                    return bool.Parse(isShowInfo);
            }
            set
            {
                try
                {
                    if (value == ShowInfo)
                        return;
                }
                catch { }


                AppState.SetStateFor(StatePurpose.IsShowInfo, value.ToString(), this.YarnAppContainer.Id);
                OnPropertyChanged(nameof(ShowInfo));
            }
        }

        public bool ShowDebug
        {
            get
            {
                var isShowDebug = AppState.GetStateFor(StatePurpose.IsShowDebug, this.YarnAppContainer.Id);
                if (string.IsNullOrEmpty(isShowDebug))
                    return true;
                else
                    return bool.Parse(isShowDebug);
            }
            set
            {
                try
                {
                    if (value == ShowInfo)
                        return;
                }
                catch { }


                AppState.SetStateFor(StatePurpose.IsShowDebug, value.ToString(), this.YarnAppContainer.Id);
                OnPropertyChanged(nameof(ShowDebug));
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

        public string QueryText
        {
            get => AppState.GetStateFor(StatePurpose.QueryText, this.YarnAppContainer.Id);
            set
            {
                try
                {
                    if (value == QueryText)
                        return;
                }
                catch { }


                AppState.SetStateFor(StatePurpose.QueryText, value, this.YarnAppContainer.Id);

                OnPropertyChanged(nameof(QueryText));
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
        }
    }
}
