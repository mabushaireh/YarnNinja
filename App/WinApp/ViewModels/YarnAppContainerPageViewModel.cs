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

                var containersLogTypeLines = YarnAppContainer.GetLogsByType(Current);

                return containersLogTypeLines;
            }

            set { }
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            // Does not see the messages with a token.
            

        }
    }
}
