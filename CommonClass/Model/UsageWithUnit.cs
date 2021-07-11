using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClass.Model
{
    public enum UsageUnit {Byte = 1, MB = 2, GB = 3};

    public class UsageWithUnit
    {
        public UsageWithUnit(double value, UsageUnit unit)
        {
            Value = value;
            Unit = unit;
        }

        public double Value { get; set; }

        public UsageUnit Unit { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", Value, Unit.ToString());
        }

        public UsageWithUnit ConvertUnitToMB()
        {
            if (Unit == UsageUnit.GB)
                return new UsageWithUnit(Value * 1024, UsageUnit.MB);
            if (Unit == UsageUnit.Byte)
                return new UsageWithUnit(Math.Round(Value / (1024 * 1024), 2), UsageUnit.MB);
            return this; // It is MB
        }

        public static UsageWithUnit operator * (double multiply, UsageWithUnit usage)
        {
            return new UsageWithUnit(usage.Value * multiply, usage.Unit);
        }
        public static UsageWithUnit operator /(UsageWithUnit usage, double divide)
        {
            return new UsageWithUnit(Math.Round(usage.Value / divide, 2), usage.Unit);
        }

        public static UsageWithUnit operator -(UsageWithUnit usage1, UsageWithUnit usage2)
        {
            double tempUsage1Value = ConvertToByte(usage1);
            double tempUsage2Value = ConvertToByte(usage2);
            return new UsageWithUnit(tempUsage1Value - tempUsage2Value, UsageUnit.Byte);
        }

        public static UsageWithUnit operator +(UsageWithUnit usage1, UsageWithUnit usage2)
        {
            double tempUsage1Value = ConvertToByte(usage1);
            double tempUsage2Value = ConvertToByte(usage2);
            return new UsageWithUnit(tempUsage1Value + tempUsage2Value, UsageUnit.Byte);
        }

        public static bool operator <(UsageWithUnit usage1, UsageWithUnit usage2)
        {
            double tempUsage1Value = ConvertToByte(usage1);
            double tempUsage2Value = ConvertToByte(usage2);
            return tempUsage1Value < tempUsage2Value;
        }

        public static bool operator >(UsageWithUnit usage1, UsageWithUnit usage2)
        {
            return !(usage1 < usage2);
        }

        public static UsageWithUnit CheckUsageUnit(UsageWithUnit usage)
        {
            if (usage.Unit == UsageUnit.Byte && usage.Value <= (1024 * 1024 * 1024))
                return new UsageWithUnit(Math.Round(usage.Value / (1024 * 1024), 2), UsageUnit.MB);
            if (usage.Unit == UsageUnit.Byte && usage.Value >= (1024 * 1024 * 1024))
                return new UsageWithUnit(Math.Round(usage.Value / (1024 * 1024 * 1024), 2), UsageUnit.GB);
            if (usage.Unit == UsageUnit.MB && usage.Value >= 1024)
                return new UsageWithUnit(Math.Round(usage.Value / 1024, 2), UsageUnit.GB);
            if (usage.Unit == UsageUnit.GB && usage.Value < 1)
                return new UsageWithUnit(Math.Round(usage.Value * 1024, 2), UsageUnit.MB);
            return usage;
        }

        private static double ConvertToByte(UsageWithUnit usage)
        {
            switch (usage.Unit)
            {
                case UsageUnit.Byte:
                    return usage.Value;
                case UsageUnit.MB:
                    return (usage.Value * 1024 * 1024);
                case UsageUnit.GB:
                    return (usage.Value * 1024 * 1024 * 1024);
            }
            throw new Exception("Invalid usage unit");
        }
    }
}
