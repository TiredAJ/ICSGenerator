using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using AJICal;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using NodaTime;

namespace ICSGenerator.Views
{
    public partial class MainView : UserControl
    {
        DateTime StartDT, EndDT;

        public MainView()
        {
            InitializeComponent();

            btn_ClickMe.Click += btn_Clicked;
            cdp_StartDate.SelectedDateChanged += cdp_StartDate_SelectionChanged;
            tswc_AllDay.IsCheckedChanged += tswc_AllDay_IsChecked;

            cdp_StartDate.SelectedDate = DateTime.Now.Date;
            cdp_EndDate.SelectedDate = DateTime.Now.Date;

            cdp_StartDate.DisplayDateStart = DateTime.Now.Date;
            cdp_EndDate.DisplayDateStart = DateTime.Now.Date;

            tp_StartTime.SelectedTime = DateTime.Now.TimeOfDay;
            tp_EndTime.SelectedTime = ((TimeSpan)tp_StartTime.SelectedTime).Add(new TimeSpan(0, 0, 1));

            StartDT = DateTime.Now;
            EndDT = DateTime.Now.Add(new TimeSpan(0, 0, 1));
        }

        public void btn_Clicked(object _Source, RoutedEventArgs _Args)
        {
            Debug.WriteLine($"cdp_Start:{cdp_StartDate.SelectedDate}");
            Debug.WriteLine($"tp_Start:{tp_StartTime.SelectedTime}");

            CalEvent NewEvent = new CalEvent()
            {
                Start = cdp_StartDate.SelectedDate.Value.Add(tp_StartTime.SelectedTime.Value),
                End = cdp_EndDate.SelectedDate.Value.Add(tp_EndTime.SelectedTime.Value),
                Title = txt_EventName.Text,
                AllDay = false,
                TimeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault(),
            };

            SaveICSFile(NewEvent);
        }

        public void cdp_StartDate_SelectionChanged(object _Source, SelectionChangedEventArgs _Args)
        {
            StartDT = cdp_StartDate.SelectedDate.Value.Add(tp_StartTime.SelectedTime.Value);

            Debug.WriteLine(cdp_StartDate.SelectedDate.Value);

            if(cdp_StartDate.SelectedDate.Value > cdp_EndDate.SelectedDate.Value)
            { cdp_EndDate.SelectedDate = cdp_EndDate.SelectedDate; }

            if(cdp_StartDate.SelectedDate.Value == cdp_EndDate.SelectedDate.Value)
            {
                if(tp_StartTime.SelectedTime.Value >= tp_EndTime.SelectedTime.Value)
                { tp_EndTime.SelectedTime = tp_StartTime.SelectedTime.Value.Add(new TimeSpan(0, 1, 0)); }
            }
        }

        public void tswc_AllDay_IsChecked(object _Source, RoutedEventArgs _Args)
        {
            if((bool)tswc_AllDay.IsChecked)
            {
                tp_StartTime.IsVisible = false;
                tp_EndTime.IsVisible = false;
            }
            else
            {
                tp_StartTime.IsVisible = true;
                tp_EndTime.IsVisible = true;
            }
        }

        public async void SaveICSFile(CalEvent _CE)
        {
            TopLevel TL = TopLevel.GetTopLevel(stkpnl_Base);

            FilePickerSaveOptions FPSO = new FilePickerSaveOptions
            {
                DefaultExtension = "ics", SuggestedFileName = _CE.Title,
                Title = $"Please choose where to save {_CE.Title}.ics"
            };

            //if(PlatformImpl)
            //{

            //}

            Debug.WriteLine($"Platform: {TL.PlatformImpl.ToString()}");

            var Storage = TL.StorageProvider;;

            if(Storage.CanSave && Storage.CanPickFolder)
            {
                //Storage.SaveFilePickerAsync(FPSO).Result;

                using(IStorageFile? ISF = await Storage.SaveFilePickerAsync(FPSO))
                {
                    if(ISF != null)
                    {
                        using(Stream Writer = await ISF.OpenWriteAsync())
                        {_CE.Serialise(Writer);}
                    }
                    else
                    {throw new ArgumentNullException("ISF returned null?");}

                }
                
            }
        }
    }
}