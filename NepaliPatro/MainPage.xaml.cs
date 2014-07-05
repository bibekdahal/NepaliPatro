using NepaliPatro.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.UI.Popups;

using Windows.ApplicationModel.Background;


namespace NepaliPatro
{
    public sealed partial class MainPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        TextBlock[,] tboxes;
        Grid[,] panels;
        TextBlock[] dboxes;
        static public string[] weeks = { "आईतबार", "सोमबार", "मंगलबार", "बुधबार", "बिहीबार", "शुक्रबार", "शनिबार" };
        static public string[] months = { "बैशाख", "जेष्ठ", "अषाढ", "श्रावण", "भाद्र", "असोज", "कात्तिक", "मंसिर", "पौष", "माघ", "फाल्गुन", "चैत्र" };

        int currentMonth, currentYear, defYear, defMonth, defDay;

        Brush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        Brush redBrush2 = new SolidColorBrush(Windows.UI.Colors.DarkRed);
        Brush blueBrush = new SolidColorBrush(Windows.UI.Colors.DarkBlue);
        Brush grayBrush = new SolidColorBrush(Windows.UI.ColorHelper.FromArgb(255, 70, 70, 70));
        Brush grayBrush2 = new SolidColorBrush(Windows.UI.ColorHelper.FromArgb(255, 200, 200, 200));
        Brush grayBrush3 = new SolidColorBrush(Windows.UI.ColorHelper.FromArgb(255, 220, 220, 220));
        Brush grayBrush4 = new SolidColorBrush(Windows.UI.ColorHelper.FromArgb(255, 209, 209, 209));

        NepDate nd = new NepDate();

        public MainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            tboxes = new TextBlock[7, 7];
            panels = new Grid[7, 7];
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 7; j++)
                {
                    Grid pnl = new Grid();
                    //pnl.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;

                    TheGrid.Children.Add(pnl);

                    TextBlock tb = new TextBlock();
                    tb.TextAlignment = TextAlignment.Center;
                    tb.Height = 50;
                    tb.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;

                    Border bd = new Border();
                    bd.BorderThickness = new Thickness(2);
                    bd.BorderBrush = grayBrush2;
                    TheGrid.Children.Add(bd);
                    Grid.SetRow(bd, i);
                    Grid.SetColumn(bd, j);

                    pnl.Children.Add(tb);

                    Grid.SetRow(pnl, i);
                    Grid.SetColumn(pnl, j);

                    tboxes[i, j] = tb;
                    panels[i, j] = pnl;
                    panels[i, j].Tag = i * 7 + j;
                    panels[i, j].PointerPressed += MainPage_PointerPressed;
                }

            dboxes = new TextBlock[7];
            for (int i = 0; i < 7; i++)
            {
                TextBlock tb = new TextBlock();
                tb.TextAlignment = TextAlignment.Center;
                tb.Height = 40;
                tb.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                tb.Text = weeks[i];
                tb.FontSize = 30;


                if (i == 6) tb.Foreground = redBrush;
                else tb.Foreground = grayBrush3;

                DayGrid.Children.Add(tb);
                Grid.SetColumn(tb, i);
                dboxes[i] = tb;
            }

            RegisterBackTask();

            int y, m, d;
            y = m = d = 0;
            DateTime td = DateTime.Today;
            nd.ConvertFromEng(td.Year, td.Month, td.Day, ref y, ref m, ref d);
            defYear = currentYear = y;
            defMonth = currentMonth = m;
            defDay = d;

            for (int i = 0; i < 12; i++)
                MonthsList.Items.Add(months[i]);
            for (int i = 0; i <= 90; i++)
                YearsList.Items.Add("वि.सं. " + GetNepVal(nd.days[i][0]));


            SetNotification();
            ShowNotification();
        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (e.PageState != null)
            {
                if (e.PageState.ContainsKey("currentMonth")) currentMonth = (int)e.PageState["currentMonth"];
                if (e.PageState.ContainsKey("currentYear")) currentYear = (int)e.PageState["currentYear"];
            }
            MonthsList.SelectedIndex = currentMonth - 1;
            YearsList.SelectedIndex = currentYear - 2000;
            FillCalendar();
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            e.PageState["currentMonth"] = currentMonth;
            e.PageState["currentYear"] = currentYear;
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion



        void FillCalendar()
        {
            int col = nd.GetFirstDay(currentYear, currentMonth);
            int row = 0;
            for (int i = 0; i < col; i++)
                tboxes[row, i].Text = "";
            for (int j = 1; j <= nd.days[currentYear - 2000][currentMonth]; j++)
            {
                if (col > 6)
                {
                    col = 0;
                    row++;
                }
                if (row > 4) row = 0;

                tboxes[row, col].Text = GetNepVal(j);
                if (currentYear == defYear && currentMonth == defMonth && j == defDay)
                {
                    if (col == 6)
                        tboxes[row, col].Foreground = redBrush2;
                    else
                        tboxes[row, col].Foreground = blueBrush;
                    tboxes[row, col].FontSize = 50;

                    panels[row, col].Background = grayBrush3;
                }
                else
                {
                    tboxes[row, col].FontSize = 30;
                    if (col == 6) tboxes[row, col].Foreground = redBrush;
                    else tboxes[row, col].Foreground = grayBrush;

                    panels[row, col].Background = grayBrush4;
                }
                col++;

            }

            if (row == 4 && col <= 6)
                for (int i = col; i < 7; i++)
                    tboxes[row, i].Text = "";
        }


        static public string GetNepVal(int num)
        {
            string str = "";
            while (num != 0)
            {
                int a = (num % 10);
                str = (char)((int)'०' + a) + str;

                num /= 10;
            }

            //while (str.Length < 2) str = '०' + str;
            ///else if (str.Length == 0) str = "००";
            return str;
        }

        void ShowNotification()
        {
            int y, m, d;
            y = m = d = 0;
            DateTime td = DateTime.Today;
            nd.ConvertFromEng(td.Year, td.Month, td.Day, ref y, ref m, ref d);

            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Block);
            tileXml.GetElementsByTagName("text")[0].InnerText = GetNepVal(d);
            tileXml.GetElementsByTagName("text")[1].InnerText = months[m - 1] + ", " + weeks[(int)td.DayOfWeek];
            ((XmlElement)tileXml.GetElementsByTagName("binding").Item(0)).SetAttribute("branding", "none");


            XmlDocument tileXmlWide = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150BlockAndText01);
            tileXmlWide.GetElementsByTagName("text")[4].InnerText = GetNepVal(d);
            tileXmlWide.GetElementsByTagName("text")[3].InnerText = weeks[(int)td.DayOfWeek];
            tileXmlWide.GetElementsByTagName("text")[2].InnerText = months[m - 1] + "  " + GetNepVal(y);
            tileXmlWide.GetElementsByTagName("text")[0].InnerText = td.ToString("D");
            ((XmlElement)tileXmlWide.GetElementsByTagName("binding").Item(0)).SetAttribute("branding", "none");

            IXmlNode node = tileXml.ImportNode(tileXmlWide.GetElementsByTagName("binding").Item(0), true);
            tileXml.GetElementsByTagName("visual").Item(0).AppendChild(node);

            XmlDocument tileXmlLarge = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare310x310BlockAndText02);
            tileXmlLarge.GetElementsByTagName("text")[0].InnerText = GetNepVal(d);
            tileXmlLarge.GetElementsByTagName("text")[1].InnerText = weeks[(int)td.DayOfWeek];
            tileXmlLarge.GetElementsByTagName("text")[2].InnerText = months[m - 1] + "  " + GetNepVal(y);
            tileXmlLarge.GetElementsByTagName("text")[3].InnerText = td.ToString("D");
            ((XmlElement)tileXmlLarge.GetElementsByTagName("binding").Item(0)).SetAttribute("branding", "name");

            IXmlNode node2 = tileXml.ImportNode(tileXmlLarge.GetElementsByTagName("binding").Item(0), true);
            tileXml.GetElementsByTagName("visual").Item(0).AppendChild(node2);


            TileNotification tileNotification = new TileNotification(tileXml);
            tileNotification.ExpirationTime = td.AddDays(1);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }

        void ClearNotifications()
        {
            var notifier = TileUpdateManager.CreateTileUpdaterForApplication();
            var scheduled = notifier.GetScheduledTileNotifications();

            for (int i = 0, len = scheduled.Count; i < len; i++)
                notifier.RemoveFromSchedule(scheduled[i]);
        }

        void SetNotification()
        {
            ClearNotifications();
            //for (int i = 1; i < 3; i++)
            //{
            int y, m, d;
            y = m = d = 0;
            DateTime td = DateTime.Today.AddDays(1);
            nd.ConvertFromEng(td.Year, td.Month, td.Day, ref y, ref m, ref d);

            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Block);
            tileXml.GetElementsByTagName("text")[0].InnerText = GetNepVal(d);
            tileXml.GetElementsByTagName("text")[1].InnerText = months[m - 1] + ", " + weeks[(int)td.DayOfWeek];
            ((XmlElement)tileXml.GetElementsByTagName("binding").Item(0)).SetAttribute("branding", "none");


            XmlDocument tileXmlWide = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150BlockAndText01);
            tileXmlWide.GetElementsByTagName("text")[4].InnerText = GetNepVal(d);
            tileXmlWide.GetElementsByTagName("text")[3].InnerText = weeks[(int)td.DayOfWeek];
            tileXmlWide.GetElementsByTagName("text")[2].InnerText = months[m - 1] + "  " + GetNepVal(y);
            tileXmlWide.GetElementsByTagName("text")[0].InnerText = td.ToString("D");
            ((XmlElement)tileXmlWide.GetElementsByTagName("binding").Item(0)).SetAttribute("branding", "none");

            IXmlNode node = tileXml.ImportNode(tileXmlWide.GetElementsByTagName("binding").Item(0), true);
            tileXml.GetElementsByTagName("visual").Item(0).AppendChild(node);

            XmlDocument tileXmlLarge = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare310x310BlockAndText02);
            tileXmlLarge.GetElementsByTagName("text")[0].InnerText = GetNepVal(d);
            tileXmlLarge.GetElementsByTagName("text")[1].InnerText = weeks[(int)td.DayOfWeek];
            tileXmlLarge.GetElementsByTagName("text")[2].InnerText = months[m - 1] + "  " + GetNepVal(y);
            tileXmlLarge.GetElementsByTagName("text")[3].InnerText = td.ToString("D");
            ((XmlElement)tileXmlLarge.GetElementsByTagName("binding").Item(0)).SetAttribute("branding", "name");

            IXmlNode node2 = tileXml.ImportNode(tileXmlLarge.GetElementsByTagName("binding").Item(0), true);
            tileXml.GetElementsByTagName("visual").Item(0).AppendChild(node2);

            ScheduledTileNotification scheduledTile = new ScheduledTileNotification(tileXml, td);
            scheduledTile.ExpirationTime = td.AddDays(1);
            TileUpdateManager.CreateTileUpdaterForApplication().AddToSchedule(scheduledTile);
            //}
        }

        async void RegisterBackTask()
        {
            var taskRegistered = false;
            var taskName = "NepCalBack";

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == taskName)
                {
                    taskRegistered = true;
                    break;
                }
            }
            if (!taskRegistered)
            {
                TimeTrigger dailyTrigger = new TimeTrigger(60 * 24, false);
                await BackgroundExecutionManager.RequestAccessAsync();

                var builder = new BackgroundTaskBuilder();

                builder.Name = taskName;
                builder.TaskEntryPoint = "Tasks.NepCalBack";
                builder.SetTrigger(dailyTrigger);
                builder.AddCondition(new SystemCondition(SystemConditionType.UserPresent));

                BackgroundTaskRegistration task = builder.Register();

            }
        }

        private void MonthsList_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            currentMonth = MonthsList.SelectedIndex + 1;
            FillCalendar();
        }

        private void YearsList_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            currentYear = YearsList.SelectedIndex + 2000;
            FillCalendar();
        }

        private void LeftNavig_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (currentMonth == 0 && currentYear == 2000) return;
            currentMonth--;
            if (currentMonth < 1)
            {
                currentMonth = 12;
                currentYear--;
            }
            MonthsList.SelectedIndex = currentMonth - 1;
            YearsList.SelectedIndex = currentYear - 2000;
        }

        private void RtNavig_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (currentMonth == 12 && currentYear == 2090) return;
            currentMonth++;
            if (currentMonth > 12)
            {
                currentMonth = 1;
                currentYear++;
            }
            MonthsList.SelectedIndex = currentMonth - 1;
            YearsList.SelectedIndex = currentYear - 2000;
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string str = "";
            var notifier = TileUpdateManager.CreateTileUpdaterForApplication();
            var scheduled = notifier.GetScheduledTileNotifications();

            /*
            for (int i = 0, len = scheduled.Count; i < len; i++)
                str += scheduled[i].DeliveryTime.ToString() + "\n";
            */

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                str += task.Value.Name + "\n";

            }


            MessageDialog msg = new MessageDialog(str);
            await msg.ShowAsync();
        }

        void MainPage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            int tag = (int)((Grid)sender).Tag;
            int i = tag / 7;
            int j = tag % 7;
            int col = nd.GetFirstDay(currentYear, currentMonth);
            if (i == 0 && j < col) return;

            int day = tag - col + 1;
            int ey = 0, em = 0, ed = 0;
            nd.ConvertToEng(ref ey, ref em, ref ed, currentYear, currentMonth, day);

            if (new DateTime(ey, em, ed) < DateTime.Today) return;

            int[] dts = new int[] { ey, em, ed, currentYear, currentMonth, day };
            this.Frame.Navigate(typeof(NotificationsPage), dts);
        }
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NotificationsListPage));
        }

    }
}
