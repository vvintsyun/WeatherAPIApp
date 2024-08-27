using System.ComponentModel.DataAnnotations;

namespace WeatherAppAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        public Units Units { get; set; }
        [Required]
        public int AllowedRate { get; set; }
    }
}
