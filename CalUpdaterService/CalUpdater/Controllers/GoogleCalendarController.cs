using CalUpdater.Common.GoogleCalendar;
using Google.Apis.Http;
using Microsoft.AspNetCore.Mvc;

namespace SurfCalendar.CalUpdater.Controllers
{
    [ApiController]
    public class GoogleCalendarController : ControllerBase
    {


        private readonly ILogger<GoogleCalendarController> _logger;

        public GoogleCalendarController(ILogger<GoogleCalendarController> logger)
        {
            _logger = logger;
        }

        //[HttpPost(Name = "CreateDefaultCalendar")]
        //public IActionResult CreateDefaultCalendar()
        //{
        //    ConnectionTest.Run();
        //    return Ok("success");
        //}

        [HttpPost("calendar")]
        public async Task<IActionResult> CalendarAsync(string spotId = Common.Const.DEFAULT_SPOT)
        {
            var calWrapper = GCalendarWrapper.Instance; //TODO change to dependency injection
            var created = await calWrapper.CreateCalendar(spotId);
            if (created)
            {
                return StatusCode(201);
            }
            else
            {
                return StatusCode(208);
            }
        }

        [HttpDelete("calendar")]
        public async Task<IActionResult> CalendarDeleteAsync(string spotId = Common.Const.DEFAULT_SPOT)
        {
            var calWrapper = GCalendarWrapper.Instance; //TODO change to dependency injection
            var deleted = await calWrapper.DeleteCalendar(spotId);
            if (deleted)
            {
                return StatusCode(201);
            }
            else
            {
                return StatusCode(208);
            }
        }

        [HttpPost("calendar/reader")]
        public async Task<IActionResult> CalendarParticipantAddAsync(string spotId = Common.Const.DEFAULT_SPOT, string readerEmail = "adi.zarfaty@gmail.com")
        {
            var calWrapper = GCalendarWrapper.Instance; //TODO change to dependency injection
            var added = await calWrapper.AddParticipantToCalendar(spotId, readerEmail);
            if (added)
            {
                return StatusCode(201);
            }
            else
            {
                return StatusCode(208);
            }
        }


        [HttpPost("events")]
        public async Task<IActionResult> EventsAsync(string spotId = Common.Const.DEFAULT_SPOT)
        {
            //TODO - call dataretriever service via contract and get the data, push to cal.


            var calWrapper = GCalendarWrapper.Instance;
            calWrapper.MergeFillFututreEventsAsync(spotId);

            return Ok();

        }
    }
}