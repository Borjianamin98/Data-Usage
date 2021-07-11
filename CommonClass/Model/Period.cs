using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClass.Model
{
    public class Period
    {
        public Period (DateTimeOffset start, DateTimeOffset end)
        {
            if (start > end)
                throw new Exception("Start must before End of period!");
            Start = start;
            End = end;
        }
        public DateTimeOffset Start { get; set; }

        public DateTimeOffset End { get; set; }

        public override string ToString()
        {
            return string.Format("{0} to {1}", Start, End);
        }

        public static Period IntersectionTwoPeriod(Period period1, Period period2)
        {
            if (period1.Start < period2.Start && period1.End < period2.End && period2.Start >= period1.End) // No Intersection
                return null;
            if (period1.Start <= period2.Start && period1.End <= period2.End && period2.Start < period1.End)
                return new Period(period2.Start, period1.End);
            if (period1.Start <= period2.Start && period1.End >= period2.End)
                return new Period(period2.Start, period2.End);
            if (period1.Start >= period2.Start && period1.End <= period2.End)
                return new Period(period1.Start, period1.End);
            if (period1.Start >= period2.Start && period1.End >= period2.End && period1.Start < period2.End)
                return new Period(period1.Start, period2.End);
            if (period1.Start >= period2.Start && period1.End >= period2.End && period1.Start >= period2.End) // No Intersection
                return null;
            throw new Exception(string.Format("Can't identify intersection between period1: {0} and period2: {1}!", period1, period2));
        }
    }
}
