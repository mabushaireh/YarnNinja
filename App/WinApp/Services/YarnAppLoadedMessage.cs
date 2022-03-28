using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YarnNinja.Common;

namespace YarnNinja.App.WinApp.Services
{
    public class YarnAppLoadedMessage : ValueChangedMessage<YarnApplication>
    {
        public YarnAppLoadedMessage(YarnApplication value) : base(value)
        {
        }
    }
}
