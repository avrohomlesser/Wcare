using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace ChartPractice
{
    class ezwChart
    {

        public string Join { get; set; }
        public string Metric { get; set; }
        public string Where { get; set; }
        public string From { get; set; }
        public string Select { get; set; }
        public string ChartType { get; set; }
        public string Quantifier { get; set; }
        public string SqlDate { get; set; }
        public string DateGroup { get; set; }
        public string DateFormat { get; set; }
        public string Range { get; set; }
        public string SeriesField { get; set; }

        public string Span { get; set; }
        public bool isMouseOverChart;
        public bool mouseHover;
        public List<string> Conditions { get; set; }
        public List<chartSeries> series { get; set; }

        public string GetFormattedDate()
        {


            return "format(" + SqlDate + "," + DateFormat + ")";
        }



        public class chartSeries
        {
            public string Category { get; set; }
            public int Amount { get; set; }
            public Boolean Enabeled { get; set; }
        }
    }
}
