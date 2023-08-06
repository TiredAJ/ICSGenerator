using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NodaTime;

namespace AJICal
{
    public class CalEvent
    {
        private Dictionary<string, string?> Parameters = new Dictionary<string, string?>()
        {
            { "TZID", "" }, //timezone
            { "ORGANIZER", "" },
            { "DTSTART", "" },
            { "DTEND", "" },
            { "SUMMARY", "" },
            { "DESCRIPTION", "" },
            { "LOCATION", "" }
        };

        public string Title
        {
            get => Parameters["SUMMARY"]; 
            set 
            {Parameters["SUMMARY"] = value;}
        }
        public string Description 
        {
            get => Parameters["DESCRIPTION"];
            set
            {Parameters["DESCRIPTION"] = value;}
        }
        public string Location 
        {
            get => Parameters["LOCATION"];
            set
            {Parameters["LOCATION"] = value;}
        }
        public DateTimeZone TimeZone 
        {
            get => DateTimeZoneProviders.Tzdb.GetZoneOrNull(Parameters["TZID"]);
            set 
            {
                Debug.WriteLine("code doesn't run unless I add this here (don't ask)");
                Parameters["TZID"] = value.Id.ToString();

            }
        }
        public bool AllDay { get; set; } = false;
        public DateTime Start 
        {
            get => DateTime.Parse(Parameters["DTSTART"]);
            set
            {Parameters["DTSTART"] = value.ToString("yyyyMMddTHHmmss");}
        }
        public DateTime End 
        {
            get => DateTime.Parse(Parameters["DTEND"]);
            set
            {Parameters["DTEND"] = value.ToString("yyyyMMddTHHmmss");}
        }

        public CalEvent()
        { }

        public CalEvent
        (string _Title, string _Desc, string _Loc, DateTimeZone _TimeZone, bool _AllDay,
            DateTime _Start, DateTime _End)
        {
            Title = _Title;
            Description = _Desc;
            Location = _Loc;
            TimeZone = _TimeZone;
            AllDay = _AllDay;
            Start = _Start;
            End = _End;
        }

        public void Serialise(Stream _Stream)
        {
            using(StreamWriter Writer = new StreamWriter(_Stream))
            {
                Writer.Write(Serialise());
                Writer.Close();
            }
        }

        public string Serialise()
        {
            StringBuilder Builder = new StringBuilder(Properties.Resources.ICSInit);

            Builder.Append($"UID:{DateTime.Now.Nanosecond}@TiredAJ" + "\r\n");

            Builder.Append($"CALSCALE:GREGORIAN" + "\r\n");
            
            Builder.Append(KVPStr(new KeyValuePair<string, string?>("TZID", Parameters["TZID"])));

            Builder.Append("BEGIN:VEVENT\r\n");

            Builder.Append($"DTSTAMP:{DateTime.UtcNow.ToString("yyyyMMddTHHmmss")}" + "\r\n");

            foreach(var KVP in Parameters.Where(x => x.Key != "TZID"))
            {Builder.Append(KVPStr(KVP));}

            Builder.Append("END:VEVENT\r\nEND:VCALENDAR");


            return Builder.ToString();
                //$@"
                //    BEGIN: VCALENDAR
                //    VERSION:2.0
                //    PRODID:-//github.com/TiredAJ
                //    UID:{DateTime.Now.Ticks}@TiredAJ
                //    CALSCALE:GREGORIAN
                //    TZID:
                //    BEGIN:VEVENT
                //    DTSTAMP:{DateTime.UtcNow.ToString("yyyyMMddTHHmmss")}
                //    ORGANIZER;CN=John Doe:MAILTO:john.doe@example.com
                //    DTSTART:{Start.ToUniversalTime()}
                //    DTEND:{End.ToUniversalTime()}
                //    SUMMARY:{Title}
                //    DESCRIPTION:
                //    LOCATION:
                //    END:VEVENT
                //    END:VCALENDAR
                //";
        }

        private string KVPStr(KeyValuePair<string, string?> _KVP)
        {
            if(_KVP.Value != null)
            {
                if(_KVP.Key.Contains("DT") && AllDay)
                {return $"{_KVP.Key}:{DateTime.Parse(_KVP.Value).Date.ToString("yyyyMMdd")}\r\n";}
                else
                {return $"{_KVP.Key}:{_KVP.Value}\r\n";}
            }
            else
            {return string.Empty;}
        }
    }
}