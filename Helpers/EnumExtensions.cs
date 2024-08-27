using Microsoft.OpenApi.Extensions;
using System.ComponentModel.DataAnnotations;

namespace WeatherAppAPI.Helpers
{
    public static class EnumExtensions
    {
        public static string? GetDisplayName(this Enum enumValue)
        {
            var attribute = enumValue.GetAttributeOfType<DisplayAttribute>();
            return attribute is null
                ? enumValue.ToString() 
                : attribute.Name;
        }
    }
}
