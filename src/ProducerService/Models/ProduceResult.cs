namespace ProducerService.Models
{
    public class ProduceResult
    {
        public bool Success { get; set; }
        public long? MessageId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}