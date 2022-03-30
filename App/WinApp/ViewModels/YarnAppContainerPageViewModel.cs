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

       
      
        protected override void OnActivated()
        {
            base.OnActivated();

            // Does not see the messages with a token.
            

        }
    }
}
