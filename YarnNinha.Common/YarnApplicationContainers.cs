namespace YarnNinja.Common
{
    public class YarnApplicationContainer
    {
        public string Id { get; set; }
        public string WorkerNode { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Status { get; set; }
        public List<YarnApplicationContainerLog> Logs { get; set; } = new List<YarnApplicationContainerLog>();

    }
}