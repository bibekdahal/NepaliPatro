using NepaliPatro.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

namespace NepaliPatro
{
    public sealed partial class NotificationsPage : Page
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


        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        int m_ey, m_em, m_ed;
        public NotificationsPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

      
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            int[] dates = e.NavigationParameter as int[];
            tbDate.Text = MainPage.GetNepVal(dates[3]) + "/" + MainPage.GetNepVal(dates[4]) + "/" + MainPage.GetNepVal(dates[5]);
            m_ey = dates[0]; m_em = dates[1]; m_ed = dates[2];

            if (new DateTime(m_ey, m_em, m_ed) == DateTime.Today)
                timepicker1.Time = DateTime.Now.TimeOfDay.Add(new TimeSpan(0, 1, 0));
            else
                timepicker1.Time = new TimeSpan(0);
        }
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            
            DateTime time = new DateTime(m_ey, m_em, m_ed).Add(timepicker1.Time);
            if (time > DateTime.Now)
            {
                ToastTemplateType toastTemplate = ToastTemplateType.ToastText01;
                XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

                XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
                toastTextElements[0].AppendChild(toastXml.CreateTextNode(tbMessage.Text));

                //IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
                //((XmlElement)toastNode).SetAttribute("duration", "long");

                //((XmlElement)toastNode).SetAttribute("launch", "{\"type\":\"toast\",\"param1\":\"12345\",\"param2\":\"67890\"}");

                ScheduledToastNotification scheduledToast = new ScheduledToastNotification(toastXml, time);
                ToastNotificationManager.CreateToastNotifier().AddToSchedule(scheduledToast);

                this.Frame.GoBack();
            }
            else
            {
                MessageDialog md = new MessageDialog("दिनुभएको समय बितिसकेको छ । अरू नै समय दिनुहोस् ।");
                await md.ShowAsync();
            }
        }
    }
}
