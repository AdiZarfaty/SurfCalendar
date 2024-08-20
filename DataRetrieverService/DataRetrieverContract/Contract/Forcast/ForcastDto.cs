
using SurfCalendar.Common.DTOs;

namespace SurfCalendar.DataRetriever.Contract.Forcast
{
    public class ForcastDto: BaseSerializableDto
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
        public DateOnly Date;

        public string HumanReadableDate => Date.ToShortDateString();
    }
}
