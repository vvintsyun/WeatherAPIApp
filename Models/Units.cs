using System.ComponentModel.DataAnnotations;

namespace WeatherAppAPI.Models
{
    public enum Units
    {
        [Display(Name = "standard")]
        Standard,
        [Display(Name = "metric")]
        Metric,
        [Display(Name = "imperial")]
        Imperial
    }
}
