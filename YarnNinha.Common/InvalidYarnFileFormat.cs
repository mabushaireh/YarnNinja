using System.Runtime.Serialization;

namespace YarnNinja.Common
{
    [Serializable]
    public class InvalidYarnFileFormat : Exception
    {
        public InvalidYarnFileFormat()
        {
        }

        public InvalidYarnFileFormat(string? message) : base(message)
        {
        }

        public InvalidYarnFileFormat(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidYarnFileFormat(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}