using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using SurfCalendar.DataRetriever.Contract.Forcast;
using SurfCalendar.CalUpdater;
using System.Globalization;

namespace CalUpdater.Common.GoogleCalendar
{
    public class GCalendarWrapper
    {
        private readonly string _credentialsJsonPath;
        private CalendarService _service;
        private string _mainCalendarId = null;
        public static GCalendarWrapper _instance = null;
        public static GCalendarWrapper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GCalendarWrapper();
                }
                return _instance;
            }
        }

        private string MainCalendarId 
        { 
            get 
            {
                if (_mainCalendarId == null)
                {
                    _mainCalendarId = GetCalendarBySpotId(SurfCalendar.CalUpdater.Common.Const.DEFAULT_SPOT).Id;
                }
                    return _mainCalendarId;
            }
        }

        public GCalendarWrapper(): this(SurfCalendar.CalUpdater.Common.Const.CREDENTIAL_JSON_PATH) { }

        public GCalendarWrapper(string credentialsJsonPath) 
        {
            _credentialsJsonPath = credentialsJsonPath;
            InitConnection();
        }

        private void InitConnection()
        {
            var credential = GoogleCredential
                .FromFile(_credentialsJsonPath)
                .CreateScoped(CalendarService.Scope.Calendar);

            _service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "CalUpdaterService",
            });
        }

        private CalendarListEntry GetCalendarBySpotId(string spotId)
        {
            var calendars = _service.CalendarList.List().Execute().Items;
            string calendarSummary = GetCalendarSummaryFromSpotId(spotId);

            var spotcal = calendars.Where((cal) => cal.Summary == calendarSummary).Single();
            return spotcal;
        }

        public async Task<bool> CreateCalendar(string spotId)
        {
            var spotcal = GetCalendarBySpotId(spotId);
            if (spotcal != null)
            {
                return false;
            }

            var newCal = new Google.Apis.Calendar.v3.Data.Calendar();
            newCal.Description = "Cal Description: 584204204e65fad6a7709ab7";
            newCal.Summary = GetCalendarSummaryFromSpotId(spotId);
            _service.Calendars.Insert(newCal).Execute();

            CalendarListEntry calendar = GetCalendarBySpotId(spotId);
            AddDefaultAccessToNewCalendar(_service, calendar);

            return true;


            // for debug
            //bool delAllCalendars = false;
            //if (delAllCalendars)
            //{
            //    foreach (CalendarListEntry calendar in calendars)
            //    {
            //        var events2 = service.Events.List(calendar.Id).Execute();

            //        service.Calendars.Delete(calendar.Id).Execute();
            //    }
            //}

            //GetFutureEvents(service);
            //SyncFututreEvents(service);

            // TODO - show only future forecast in my user mail (cancel status on prev? hidden? a duplicated calendar for future (shows only when waves)?)
            // TODO - go over past events and mark them to hidden..

        }

        public async Task<bool> DeleteCalendar(string spotId)
        {
            var spotcal = GetCalendarBySpotId(spotId);
            if (spotcal == null)
            {
                return false;
            }

            _service.Calendars.Delete(spotcal.Id).Execute();
            return true;
        }

        public async Task<bool> AddParticipantToCalendar(string spotId, string participantEmail)
        {
            var spotcal = GetCalendarBySpotId(spotId);
            AddAclToCalendar(_service, spotcal.Id, participantEmail, Const.ACL_ROLE_READER);
            return true;
        }



        //private static void GetFutureEvents(CalendarService service)
        //{
        //    service.




        //    // Retrieve an event
        //    var events = service.Events.Get("primary", "eventId").Execute();
        //}

        private static string GetCalendarSummaryFromSpotId(string spotId)
        {
            return "SurCalSpot:" + spotId;
        }

        public class Const
        {
            public const string ACL_ROLE_OWNER = "owner";
            public const string ACL_ROLE_READER = "reader";
        }

        private static void AddDefaultAccessToNewCalendar(CalendarService service, CalendarListEntry calendar)
        {
            // main account owns all for easy manual edit
            AddAclToCalendar(service, calendar.Id, "surf.calendar.main@gmail.com", Const.ACL_ROLE_OWNER);

            // TODO - replace with a signup process
            if (calendar.Summary == GetCalendarSummaryFromSpotId(SurfCalendar.CalUpdater.Common.Const.DEFAULT_SPOT))
            {
                AddAclToCalendar(service, calendar.Id, "adi.zarfaty@gmail.com", Const.ACL_ROLE_READER);
            }
        }

        private static void AddAclToCalendar
            (CalendarService service,
            string calendarId,
            string email,
            string role)
        {
            var rule = new AclRule
            {
                Scope = new AclRule.ScopeData
                {
                    Type = "user",
                    Value = email
                },
                Role = role
            };

            // Insert the new rule
            var request = service.Acl.Insert(rule, calendarId);
            AclRule createdRule = request.Execute();

            Console.WriteLine($"Added {rule.Scope.Value} as a {rule.Role} to the calendar.");
        }

        public void MergeFillFututreEventsAsync(string spotId)
        {
            //TODO replace with dependency injection
            var dataRetriever = new DataRetrieverContract.Contract.DataRetrieverContractAdapter();

            ForcastResponseDto forcast = dataRetriever.GetForcastDataAsync(spotId, 5).Result;

            var calendar = GetCalendarBySpotId(spotId);

            // see https://developers.google.com/calendar/api/v3/reference/events/list
            var request = _service.Events.List(calendar.Id);
            request.TimeMinDateTimeOffset = DateTime.Now.Date;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            request.SingleEvents = true; // Force recurring events to be sorted by the first (non should exist anyway)

            var existingCalEvents = request.Execute();

            HelperMergeFillFututreEvents(existingCalEvents.Items.ToArray(), 0, forcast.Days, 0);

        }

        private void HelperMergeFillFututreEvents(Event[] events, int eventsIndex, DayDto[] days, int dayIndex)
        {
            if (events.Length <= eventsIndex)
            {
                for (int i = dayIndex; i < days.Length; i++) 
                {
                    AddEventToCalendar(days[i]); //TODO slow - improve with bulk insert
                    return;
                }
            }

            if (days.Length <= dayIndex)
            {
                return;
            }

            // we have forecast days + events to sync
            if (events[eventsIndex].Start.DateTimeDateTimeOffset.Value.Date == days[dayIndex].Date.Date) 
            {
                EditEventInCalendar(events[eventsIndex], days[dayIndex]); //TODO slow - improve with bulk update
                HelperMergeFillFututreEvents(events, eventsIndex + 1, days, dayIndex + 1);
                return;
            }

            if (events[eventsIndex].Start.DateTimeDateTimeOffset.Value.Date < days[dayIndex].Date.Date)
            {
                throw new Exception("Unexpected event - no forcast");
            }

            if (events[eventsIndex].Start.DateTimeDateTimeOffset.Value.Date > days[dayIndex].Date.Date)
            {
                throw new Exception("Unexpected forcast - missing event");
            }

            throw new Exception("Unexpected merge");
        }

        private void EditEventInCalendar(Event ev, DayDto dayDto)
        {
            SetEventData(ev, dayDto);

            _service.Events.Update(ev, MainCalendarId, ev.Id).Execute();
        }

        private void AddEventToCalendar(DayDto dayForcast)
        {
            var ev = new Google.Apis.Calendar.v3.Data.Event();
            SetEventData(ev, dayForcast);

            //TODO add waves to calendar only if > threshold?
            _service.Events.Insert(ev, MainCalendarId).Execute();
        }

        private void SetEventData(Event ev, DayDto dayForcast)
        {
            EventDateTime start = new EventDateTime();

            EventDateTime end = new EventDateTime();
            SetEventTime(dayForcast, start, end);

            ev.Summary = GetEventSummary(dayForcast);
            ev.Start = start;
            ev.End = end;
            ev.Description = "";
            ev.Transparency = GetEventTransperancy(dayForcast);
            //ev.ColorId = "1"; // blue
        }

        private static void SetEventTime(DayDto dayForcast, EventDateTime start, EventDateTime end)
        {
            start.DateTimeDateTimeOffset = new DateTime(dayForcast.Date.Year, dayForcast.Date.Month, dayForcast.Date.Day, 6, 0, 0);
            end.DateTimeDateTimeOffset = new DateTime(dayForcast.Date.Year, dayForcast.Date.Month, dayForcast.Date.Day, 19, 0, 0);
        }

        private string GetEventTransperancy(DayDto dayForcast)
        {

            // Whether the event blocks time on the calendar. Optional. Possible values are:   - "opaque" - Default value.
            // The event does block time on the calendar. This is equivalent to setting Show me as to Busy in the Calendar
            // UI.  - "transparent" - The event does not block time on the calendar. This is equivalent to setting Show me
            // as to Available in the Calendar UI.
            if (dayForcast.MinWaveHeight >= 0.2m)
            {
                return "opaque";
            }
            else
            {
                return "transparent";
            }
        }

        private string GetEventSummary(DayDto dayForcast)
        {
            //🌊🏄‍
            return $"🌊{dayForcast.MinWaveHeight}-{dayForcast.MaxWaveHeight}m";
        }

    }
}
