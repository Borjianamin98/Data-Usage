using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization;

namespace CommonClass.Model
{
    public class CustomDateTime
    {
        public CustomDateTime(string calendarType, int year, int month, int day)
        {
            Day = day;
            Month = month;
            Year = year;
            Hour = 0;
            Minute = 0;
            Second = 0;
            CalendarType = calendarType;
        }
        public CustomDateTime(string calendarType, int year, int month, int day, int hour, int minute, int second)
        {
            Day = day;
            Month = month;
            Year = year;
            Hour = hour;
            Minute = minute;
            Second = second;
            CalendarType = calendarType;
        }

        public CustomDateTime(string calendarType, int year, int month, int day, TimeSpan timeSpan)
        {
            Day = day;
            Month = month;
            Year = year;
            Hour = timeSpan.Hours;
            Minute = timeSpan.Minutes;
            Second = timeSpan.Seconds;
            CalendarType = calendarType;
        }

        public CustomDateTime(string calendarType, DateTime dateTime)
        {
            Day = dateTime.Day;
            Month = dateTime.Month;
            Year = dateTime.Year;
            Hour = dateTime.Hour;
            Minute = dateTime.Minute;
            Second = dateTime.Second;
            CalendarType = calendarType;
        }
        private void Update(CustomDateTime dateTime)
        {
            Day = dateTime.Day;
            Month = dateTime.Month;
            Year = dateTime.Year;
            Hour = dateTime.Hour;
            Minute = dateTime.Minute;
            Second = dateTime.Second;
            CalendarType = dateTime.CalendarType;
        }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        public string CalendarType { get; set; } = "GregorianCalendar";
        public int NumberOfDaysInThisMonth => GlobalCalendar.GetNumberOfDaysInThisMonth(this);
        public CustomDateTime AddDays(int days) => GlobalCalendar.AddDate(this, new CustomDateTime(CalendarType, 0, 0, days));
        public CustomDateTime AddMonths(int months) => GlobalCalendar.AddDate(this, new CustomDateTime(CalendarType, 0, months, 0));
        public CustomDateTime AddYears(int year) => GlobalCalendar.AddDate(this, new CustomDateTime(CalendarType, year, 0, 0));
        public CustomDateTime AddHours(int hours) => GlobalCalendar.AddDate(this, new CustomDateTime(CalendarType, 0, 0, 0, hours, 0, 0));
        public CustomDateTime AddMinutes(int minutes) => GlobalCalendar.AddDate(this, new CustomDateTime(CalendarType, 0, 0, 0, 0, minutes, 0));
        public CustomDateTime AddSeconds(int seconds) => GlobalCalendar.AddDate(this, new CustomDateTime(CalendarType, 0, 0, 0, 0, 0, seconds));
        public string GetMonthName() => GlobalCalendar.GetMonthName(this);
        public string GetDayOfWeekName() => GlobalCalendar.GetDayOfWeekName(this);

        /// <summary>
        /// Convert CustomDateTime to DateTime
        /// </summary>
        /// <param name="AutoConvertToGregorian">if calendar type isn't gregorian then app throws exception.</param>
        /// <returns></returns>
        public DateTime ConvertToDateTime(bool AutoConvertToGregorian)
        {
            if (!AutoConvertToGregorian)
            {
                if (this.CalendarType != CalendarIdentifiers.Gregorian)
                    throw new Exception("Calendar types are differnet!") { Source = ".../CommonClass.Model.CustomDateTime.ConvertToDateTime" };
            }

            var gregorianDate = GlobalCalendar.ConvertDateTo(this, CalendarIdentifiers.Gregorian);

            return new DateTime(gregorianDate.Year, gregorianDate.Month, gregorianDate.Day, gregorianDate.Hour, gregorianDate.Minute, gregorianDate.Second);
        }
        public double GetCompareTotalDays(CustomDateTime dateTime)
        {
            var tempDateTime1 = this.ConvertToDateTime(true);
            var tempDateTime2 = dateTime.ConvertToDateTime(true);
            return (tempDateTime2 - tempDateTime1).TotalDays;
        }
        public CustomDateTime CopyTo() => new CustomDateTime(this.CalendarType, new DateTime(this.Year, this.Month, this.Day, this.Hour, this.Minute, this.Second));
        public CustomDateTime GetFirstDayOfThisMonth() => new CustomDateTime(this.CalendarType, this.Year, this.Month, 1);
        public CustomDateTime GetFirstDayOfThisWeek(string startOfWeek) => GlobalCalendar.GetFirstDayOfWeek(this, startOfWeek);
        public CustomDateTime GetFirstMinuteOfThisHour() => new CustomDateTime(this.CalendarType, this.Year, this.Month, this.Day, this.Hour, 0, 0);
        public CustomDateTime GetFirstHourOfThisDay() => new CustomDateTime(this.CalendarType, this.Year, this.Month, this.Day, 0, 0, 0);
    }
}
