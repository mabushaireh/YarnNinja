using System.Runtime.Serialization;

namespace YarnNinja.Common
{
    [Serializable]
    public class InvalidYarnFileFormat : Exception
    {
        public InvalidYarnFileFormat(string? message) : base(message)
        {
        }
    }
}