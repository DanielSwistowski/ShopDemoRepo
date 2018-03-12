namespace DataAccessLayer.Models
{
    public class Photo
    {
        public int PhotoId { get; set; }
        public string PhotoPath { get; set; }
        public string PhotoThumbPath { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}