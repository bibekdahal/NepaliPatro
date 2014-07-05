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

        string GetNepVal(int num)
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


        XmlDocument CreateTile(DateTime td, int y, int m, int d)
        {
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
            return tileXml;
        }

        void ShowNotification()
        {
            int y, m, d;
            y = m = d = 0;
            DateTime td = DateTime.Today;
            nd.ConvertFromEng(td.Year, td.Month, td.Day, ref y, ref m, ref d);

            TileNotification tileNotification = new TileNotification(CreateTile(td, y, m, d));
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

            ScheduledTileNotification scheduledTile = new ScheduledTileNotification(CreateTile(td, y, m, d), td);
            scheduledTile.ExpirationTime = td.AddDays(1);
            TileUpdateManager.CreateTileUpdaterForApplication().AddToSchedule(scheduledTile);
            //}
        }

    }
}
