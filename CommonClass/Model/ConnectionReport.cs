using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace CommonClass.Model
{
    public class ConnectionReport
    {
        public ConnectionReport()
        {
            Upload = new UsageWithUnit(0, UsageUnit.Byte);
            Download = new UsageWithUnit(0, UsageUnit.Byte);
            UploadDiscount = new UsageWithUnit(0, UsageUnit.Byte);
            DownloadDiscount = new UsageWithUnit(0, UsageUnit.Byte);
        }

        public string Date { get; set; }

        public UsageWithUnit Upload { get; set; }

        public UsageWithUnit UploadDiscount { get; set; }

        public UsageWithUnit Download { get; set; }

        public UsageWithUnit DownloadDiscount { get; set; }

        public Color TableColor { get; set; }

        public void ConvertDataToMB()
        {
            Upload = Upload.ConvertUnitToMB();
            UploadDiscount = UploadDiscount.ConvertUnitToMB();
            Download = Download.ConvertUnitToMB();
            DownloadDiscount = DownloadDiscount.ConvertUnitToMB();
        }
    }    
}
