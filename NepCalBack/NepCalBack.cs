using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using NepaliPatro;

namespace Tasks
{
    public sealed class NepCalBack : IBackgroundTask
    {

        string[] weeks = { "आईतबार", "सोमबार", "मंगलबार", "बुधबार", "बिहीबार", "शुक्रबार", "शनिबार" };
        string[] months = { "बैशाख", "जेष्ठ", "अषाढ", "श्रावण", "भाद्र", "असोज", "कात्तिक", "मंसिर", "पोष", "माघ", "फाल्गुन", "चैत्र" };

        NepDate nd = new NepDate();
        public void Run(IBackgroundTaskInstance taskInstance)
        {


            /*BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
            // TODO: Insert code to start one or more asynchronous methods using the
            //       await keyword
            _deferral.Complete();*/

            SetNotification();
            ShowNotification();
        }

        string getNepVal(int num)
        {
            string str = "";
            while (num != 0)
            {
                int a = (num % 10);
                str = (char)((int)'०' + a) + str;

                num /= 10;
            }
            return str;
        }

        void ShowNotification()
        {
            int y, m, d;
            y = m = d = 0;
            DateTime td = DateTime.Today;
            nd.ConvertFromEng(td.Year, td.Month, td.Day, ref y, ref m, ref d);

            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Block);
            //TileSquare150x150Block); for windows 8.1+
            tileXml.GetElementsByTagName("text")[0].InnerText = getNepVal(d);
            tileXml.GetElementsByTagName("text")[1].InnerText = months[m - 1] + ", " + weeks[(int)td.DayOfWeek];


            XmlDocument tileXmlWide = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150BlockAndText01);
            tileXmlWide.GetElementsByTagName("text")[4].InnerText = getNepVal(d);
            tileXmlWide.GetElementsByTagName("text")[3].InnerText = weeks[(int)td.DayOfWeek];
            tileXmlWide.GetElementsByTagName("text")[2].InnerText = months[m - 1] + "  " + getNepVal(y);

            IXmlNode node = tileXml.ImportNode(tileXmlWide.GetElementsByTagName("binding").Item(0), true);
            tileXml.GetElementsByTagName("visual").Item(0).AppendChild(node);

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
            //TileSquare150x150Block); for windows 8.1+
            tileXml.GetElementsByTagName("text")[0].InnerText = getNepVal(d);
            tileXml.GetElementsByTagName("text")[1].InnerText = months[m - 1] + ", " + weeks[(int)td.DayOfWeek];


            XmlDocument tileXmlWide = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150BlockAndText01);
            tileXmlWide.GetElementsByTagName("text")[4].InnerText = getNepVal(d);
            tileXmlWide.GetElementsByTagName("text")[3].InnerText = weeks[(int)td.DayOfWeek];
            tileXmlWide.GetElementsByTagName("text")[2].InnerText = months[m - 1] + "  " + getNepVal(y);

            IXmlNode node = tileXml.ImportNode(tileXmlWide.GetElementsByTagName("binding").Item(0), true);
            tileXml.GetElementsByTagName("visual").Item(0).AppendChild(node);

            ScheduledTileNotification scheduledTile = new ScheduledTileNotification(tileXml, td);
            scheduledTile.ExpirationTime = td.AddDays(1);
            TileUpdateManager.CreateTileUpdaterForApplication().AddToSchedule(scheduledTile);
            //}
        }

    }
}
