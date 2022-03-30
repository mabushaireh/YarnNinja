using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace YarnNinja.App.WinApp.Models
{
    public partial class WorkerNode : ObservableObject
    {
        [ObservableProperty]
        private string name;

        public override string ToString()
        {
            return Name;
        }
    }
}
