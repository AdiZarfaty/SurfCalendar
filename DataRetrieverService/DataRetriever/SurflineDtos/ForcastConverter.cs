using SurfCalendar.DataRetriever.Contract.Forcast;
using SurfCalendar.DataRetriever.SurflineDtos.Forcast;
using Microsoft.VisualBasic;

namespace SurfCalendar.DataRetriever.SurflineDtos.Convertion
{
    public class ForcastConverter
    {

        public ForcastResponseDto ConvertForcast(SurflineDtos.Forcast.ForcastRoot data)
        {
            VerifyMetricsUnits(data);

            Contract.Forcast.ForcastResponseDto result = new ForcastResponseDto();

            var daysList = new LinkedList<DayDto>();

            int utcOffset = data.utcOffset;
            data.data.forecasts.Sort((new ForecastPointComp()));

            Contract.Forcast.DayDto? lastDay = null;

            foreach (ForecastPoint point in data.data.forecasts)
            {
                var pointDate = UnixTimeStampToDateTime(point.timestamp).Date;

                // add new day
                if ((lastDay == null) || 
                    (!lastDay.Date.Equals(pointDate)))
                {
                    lastDay = new DayDto();
                    lastDay.Date = pointDate;
                    lastDay.MinWaveHeight = DayDto.Consts.UNSET_MAX_WAVE_HEIGHT;
                    lastDay.MaxWaveHeight = DayDto.Consts.UNSET_MIN_WAVE_HEIGHT;
                    daysList.AddLast(lastDay);
                }

                // fill data
                if (point.surf.min < (double) lastDay.MinWaveHeight)
                {
                    lastDay.MinWaveHeight = decimal.Round((decimal)point.surf.min, DayDto.Consts.PRECISION);
                }
                if (point.surf.max > (double)lastDay.MaxWaveHeight)
                {
                    lastDay.MaxWaveHeight = decimal.Round((decimal) point.surf.max, DayDto.Consts.PRECISION);
                }
            }

            result.Days = daysList.ToArray();

            return result;
        }

        private void VerifyMetricsUnits(ForcastRoot data)
        {
            //TODO add support of other metrics
            if (data.units.swellHeight != SurflineDtos.Consts.METERS)
            {
                throw new ArgumentException("Data is not in meters");
            }

        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }

    public static class ForecastPointExtention
    {
        public static int Compare(this SurflineDtos.Forcast.ForecastPoint forecast1, SurflineDtos.Forcast.ForecastPoint forecast2)
        {
            return forecast1.timestamp.CompareTo(forecast2.timestamp);
        }
    }

    public class ForecastPointComp : IComparer<SurflineDtos.Forcast.ForecastPoint>
    {
        // Compares by Height, Length, and Width.
        public int Compare(ForecastPoint x, ForecastPoint y)
        {
            return x.Compare(y);
        }
    }
}
