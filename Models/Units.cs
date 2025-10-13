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

    public static class UnitsHelper
    {
        public static string GetUnitValue(this Units units)
        {
            switch (units)
            {
                case Units.Imperial: return "imperial";
                case Units.Metric: return "metric";
                case Units.Standard: return "standard";
                default: throw new NotImplementedException();
            }
        }
    }
}
