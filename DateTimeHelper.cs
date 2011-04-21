using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace UtilityCodeAsset.Helper
{
    public class DateTimeHelper
    {
        private const int WrongDurationInputFormat = -100;
        /// <summary>
        /// Get the time the last Mobilink related log entry was written
        /// 
        /// </summary>
        /// <param name="log">Log entries</param>
        /// <returns>Last time log was written</returns>
        public static DateTime GetLastLogWrittenTime(string log)
        {
            int position = log.LastIndexOf("E. ");
            if (position < 0)
                return DateTime.MinValue;
            position += 3;
            string dt_str = log.Substring(position, 19);
            DateTime rc = DateTime.MinValue;
            DateTime.TryParse(dt_str, out rc);
            return rc;
        }

        

        /// <summary>
        /// Get the time by parsing the time string provided
        /// </summary>
        /// <param name="timestr"></param>
        /// <returns>DateTime object of the given time</returns>
        public static DateTime GetTimeFromString(string timestr)
        {
            DateTime rc;
            int timeint, min, hour;
            string timestr4digits;

            Regex r = new Regex(@"\s+");
            string tstr = r.Replace(timestr, @"");
            if (!(DateTime.TryParse(tstr, out rc)))
            {
                if (tstr.Length > 2 && tstr.Length < 10)
                {
                    try
                    {
                        timeint = int.Parse(tstr);
                        hour = Math.DivRem(timeint, 100, out min);
                        timestr4digits = String.Format("{0}:{1:d2}", hour, min);
                        if (!(DateTime.TryParse(timestr4digits, out rc)))
                        {
                            return DateTime.MinValue;
                        }
                    }
                    catch
                    {
                        return DateTime.MinValue;
                    }
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
            return rc;
        }

        

        /// <summary>
        /// Return the total minutes from the two formats (w or w/o : standing for timestamp)
        /// </summary>
        /// <param name="timestr"></param>
        /// <returns>Total minutes</returns>
        public static int GetMinutesString(string timestr)
        {
            TimeSpan rc;
            int timeint, min, hour;
            string timestr4digits;

            if (!(timestr.Contains(":")))
            {
                if (timestr.Length < 5)
                {
                    try
                    {
                        timeint = int.Parse(timestr);
                        hour = Math.DivRem(timeint, 60, out min);
                        // Special case where the minutes entered is between 0 to 99
                        if (hour == 0)
                            return min;
                        timestr4digits = String.Format("{0}:{1:d2}", hour, min);
                    }
                    catch
                    {
                        return WrongDurationInputFormat;
                    }
                }
                else
                {
                    return WrongDurationInputFormat;
                }
            }
            else
            {
                timestr4digits = timestr;
            }

            if (!(TimeSpan.TryParse(timestr4digits, out rc)))
            {
                return WrongDurationInputFormat;
            }
            return (int)rc.TotalMinutes;
        }

        public static T[] ToArray<T>(IList<T> list)
        {
            if (list == null)
                return null;
            if (list is Array) return (T[]) list;

            T[] retval = new T[list.Count];
            for (int i = 0; i < retval.Length; i++) 
                retval[i] = list[i];

            return retval;
        }

        public static bool DateTimePickerAdjustAt59(DateTime oldvalue, ref DateTimePicker newvalue)
        {
            if (oldvalue.Second == 0 && newvalue.Value.Second == 59 &&
                oldvalue.Minute == newvalue.Value.Minute)
            {
                newvalue.Value = newvalue.Value.AddMinutes(-1);
                return true;
            }
            if (oldvalue.Minute == 0 && newvalue.Value.Minute == 59 &&
                oldvalue.Hour == newvalue.Value.Hour)
            {
                newvalue.Value = newvalue.Value.AddHours(-1);
                return true;
            }
            if (oldvalue.Second == 59 && newvalue.Value.Second == 0 &&
                oldvalue.Minute == newvalue.Value.Minute)
            {
                newvalue.Value = newvalue.Value.AddMinutes(1);
                return true;
            }
            if (oldvalue.Minute == 59 && newvalue.Value.Minute == 0 &&
                oldvalue.Hour == newvalue.Value.Hour)
            {
                newvalue.Value = newvalue.Value.AddHours(1);
                return true;
            }
            return false;
        }
    }
}
