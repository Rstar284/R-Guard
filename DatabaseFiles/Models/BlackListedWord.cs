namespace RGuard.Database.Models
{
    public class BlackListedWord
    {
        public int Id { get; set; }
        public Guild Guild { get; set; }
        public string Word { get; set; }
    }
}