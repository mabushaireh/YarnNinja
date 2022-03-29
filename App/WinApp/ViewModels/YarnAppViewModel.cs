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

namespace YarnNinja.App.WinApp.ViewModels
{
    public partial class YarnAppPageViewModel : ObservableRecipient
    {
        public YarnApplication YarnApp { get; set; }
        public YarnAppPageViewModel()
        {
            
        }

        public String YarnType { get { 
                return YarnApp.Header.Type.ToString();
            } 
        }

        public String Start
        {
            get
            {
                return YarnApp.Header.Start.ToString("yyyy-MM-dd HH:mm:ss,fff");
            }
        }

        public String Finish
        {
            get
            {
                return YarnApp.Header.Finish.ToString("yyyy-MM-dd HH:mm:ss,fff");
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            // Does not see the messages with a token.
            

        }

        private void OnYarnAppLoadedMessageReceived(object r, YarnAppLoadedMessage m)
        {
        }
    }
}
