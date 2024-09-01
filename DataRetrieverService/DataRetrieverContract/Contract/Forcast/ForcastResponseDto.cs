
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SurfCalendar.Common.DTOs;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SurfCalendar.DataRetriever.Contract.Forcast
{
    /// <summary>
    /// docu ForcastRequestDto
    /// </summary>
    public class ForcastRequestDto : BaseSerializableDto, IModelBinder
    {
        /// <summary>
        /// spot id
        /// </summary>
        [Required]
        [DefaultValue("584204204e65fad6a7709ab7")]
        public string SpotId { get; set; }

        /// <summary>
        /// days forward
        /// </summary>
        [Required]
        [DefaultValue(5)]
        public int Days { get; set; }

        Task IModelBinder.BindModelAsync(ModelBindingContext bindingContext)
        {
            throw new NotImplementedException();
        }
    }

    public class ForcastResponseDto: BaseSerializableDto
    {
        /// <summary>
        /// Sorted array of days, by date
        /// </summary>
        public DayDto[] Days;

    }

    public class DayDto
    {
        public static class Consts
        {
            public static int PRECISION = 1; // decimal places for wave height
            public static decimal UNSET_MIN_WAVE_HEIGHT = -1000;
            public static decimal UNSET_MAX_WAVE_HEIGHT = 1000;
        }

        public decimal MaxWaveHeight;
        public decimal MinWaveHeight;
        public DateTime Date;

        public string HumanReadableDate => Date.ToShortDateString();
    }
}
