using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YarnNinja.Common;

namespace YarnNinja.App.WinApp.Models
{

    public enum StatePurpose
    {
        SelectedWorkerNode = 0,
        SelectedContainer = 1,
        SelectedLogType = 2,
        SelectedLogLine = 3,
        IsShowError = 4,
        IsShowWarning = 5,
        IsShowInfo = 6,
        IsShowDebug = 7,
        QueryText = 8
    }

    internal static class AppState
    {
        private static Dictionary<string, string> cached_state = new();

        private static string GenerateKey(StatePurpose stateFor, params string[] list)
        {
            StringBuilder key = new();

            for (int i = 0; i < list.Length; i++)
            {
                key.Append(list[i]);
                key.Append("|");
            }
            key.Append(stateFor.ToString());

            return key.ToString();
        }

        public static string GetStateFor(StatePurpose stateFor, params string[] list) 
        {
            if (cached_state.ContainsKey(GenerateKey(stateFor, list)))
                return cached_state[GenerateKey(stateFor, list)];
            else
                return "";
        }

        public static void SetStateFor(StatePurpose stateFor, string state, params string[] list)
        {
            cached_state[GenerateKey(stateFor, list)] = state;
        }
    }
}

