using ServiceWorker;
 public class PlanDTO
    {
        public string CustomerName { get; set; }
        public DateTime StartTime { get; set; }
        public string StartLocation { get; set; }
        public string EndLocation { get; set; }
    
    public string ToCsvString()
        {
            // Customize the formatting based on your requirements
            return $"{CustomerName},{StartTime},{StartLocation},{EndLocation}";
        }
}
