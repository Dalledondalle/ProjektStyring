using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ProjektStyring
{
    public static class EasyHandler
    {
        //Properties to quickly change the value of students, admin, instructor and teacher through out the entire problem
        public static string Student { get; } = "Student";
        public static string Admin { get; } = "Administrator";
        public static string Instructor { get;  } = "Instructor";
        public static string Teacher { get; } = "StockManager";

        //Property to keep change between summer and winter time if the server is running UTC
        public static int Gmt 
        {
            get
            {
                DateTime theDate = DateTime.Now;
                TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
                bool isCurrentlyDaylightSavings = tzi.IsDaylightSavingTime(theDate);
                if (isCurrentlyDaylightSavings)
                    return 2;
                else
                    return 1;
            }
        } //Sommertid +2, Vintertid +1
    }
}
