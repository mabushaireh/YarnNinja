namespace YarnNinja.Common
{
    public class YarnApplicationContainer
    {

        public string Id { get; set; }
        public int Order
        {
            get
            {
                return int.Parse(Id.Substring(Id.Length - 6, 6));
            }
        }
        public string WorkerNode { get; set; }
        public DateTime Start { get; set; }
        public string Status { get; set; }
        public List<YarnApplicationContainerLog> Logs { get; set; } = new List<YarnApplicationContainerLog>();
        public DateTime Finish { get; set; }
    }
}