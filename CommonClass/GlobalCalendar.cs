using CommonClass.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization;

namespace CommonClass
{
    public class GlobalCalendar
    {
        public static List<string> gregorianMonthName = new List<string> { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        public static List<string> persianMonthName = new List<string> { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };
        //private static string[] DaysOfWeek = new string[] { "", "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

        public static CustomDateTime ConvertDateTo(CustomDateTime FromDateTime, string ToCalendar)
        {
            if (FromDateTime.CalendarType == ToCalendar)
                return FromDateTime;

            var calendar = CreateCalendar(FromDateTime.CalendarType, FromDateTime);
            calendar.ChangeCalendarSystem(ToCalendar);
            return ConvertToCustomDateTime(calendar);
        }

        public static CustomDateTime GetFirstDayOfWeek(CustomDateTime dateTime, string startOfWeek)
        {
            var calendar = CreateCalendar(dateTime.CalendarType, dateTime);

            calendar.ChangeCalendarSystem(CalendarIdentifiers.Gregorian); // get calendar day of week name in international mode.
            var dayOfWeek = calendar.DayOfWeekAsString();
            while (true)
            {
                if (calendar.DayOfWeekAsString() == startOfWeek)
                    break;
                else
                    calendar.AddDays(-1);
            }
            calendar.ChangeCalendarSystem(dateTime.CalendarType);
            calendar.Hour = 0; calendar.Minute = 0; calendar.Second = 0;

            return ConvertToCustomDateTime(calendar);
        }

        public static string GetDayOfWeekName(CustomDateTime dateTime)
        {
            var calendar = CreateCalendar(dateTime.CalendarType, dateTime);
            return calendar.DayOfWeekAsString();
        }

        public static CustomDateTime AddDate(CustomDateTime dateTime , CustomDateTime ChangeDateTime)
        {
            if (dateTime.CalendarType != ChangeDateTime.CalendarType)
                throw new Exception("Calendar types are differnet!") { Source = ".../CommonClass.GlobalCalendar.AddDate" };
            var calendar = CreateCalendar(dateTime.CalendarType, dateTime);
            calendar.AddDays(ChangeDateTime.Day);
            calendar.AddMonths(ChangeDateTime.Month);
            calendar.AddYears(ChangeDateTime.Year);
            calendar.AddHours(ChangeDateTime.Hour);
            calendar.AddMinutes(ChangeDateTime.Minute);
            calendar.AddSeconds(ChangeDateTime.Second);
            return ConvertToCustomDateTime(calendar);
        }

        public static int GetNumberOfDaysInThisMonth(CustomDateTime dateTime)
        {
            var calendar = CreateCalendar(dateTime.CalendarType, dateTime);
            return calendar.NumberOfDaysInThisMonth;
        }

        public static string GetMonthName(CustomDateTime dateTime)
        {
            var calendar = CreateCalendar(dateTime.CalendarType, dateTime);
            return calendar.MonthAsString();            
        }

        private static Calendar CreateCalendar(string calendarType, CustomDateTime dateTime)
        {
            Calendar calendar = new Calendar();
            calendar.ChangeClock(ClockIdentifiers.TwentyFourHour);
            calendar.ChangeCalendarSystem(calendarType);
            calendar.Year = dateTime.Year;
            calendar.Month = dateTime.Month;
            calendar.Day = dateTime.Day;
            calendar.Hour = dateTime.Hour;
            calendar.Minute = dateTime.Minute;
            calendar.Second = dateTime.Second;
            return calendar;
        }

        public static CustomDateTime ConvertToCustomDateTime(Calendar calendar)
        {
            return new CustomDateTime(calendar.GetCalendarSystem(), calendar.Year, calendar.Month, calendar.Day, calendar.Hour, calendar.Minute, calendar.Second);
        }

    }
}
