using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace YarnNinja.App.WinApp.Services
{
    public interface INavigation
    {
        void AddMenuItem(string parent, string child);
        void RemoveMenuItem(string parent, string child);
    }
}
