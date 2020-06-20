﻿using PayrollCore.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views.AdminSettings.UserManagement
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddMeetingPage : Page
    {

        DispatcherTimer timeUpdater = new DispatcherTimer();
        DispatcherTimer loadTimer = new DispatcherTimer();
        User user;

        public AddMeetingPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                user = (e.Parameter as User);
                return;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            timeUpdater.Interval = new TimeSpan(0, 0, 30);
            timeUpdater.Tick += TimeUpdater_Tick;
            timeUpdater.Start();

            loadTimer.Interval = new TimeSpan(0, 0, 1);
            loadTimer.Tick += LoadTimer_Tick;
            loadTimer.Start();
        }

        private async void LoadTimer_Tick(object sender, object e)
        {
            loadTimer.Stop();
            user = await SettingsHelper.Instance.da.GetUserFromDbById(user.userID);
            loadGrid.Visibility = Visibility.Collapsed;
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserListPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        private async void signInAsBtn_Click(object sender, RoutedEventArgs e)
        {
            Meeting meeting = meetingBox.SelectedItem as Meeting;
            confirmMeetingText.Text = meeting.meetingName;

            var result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Rate meetingRate = await SettingsHelper.Instance.da.GetRateById(meeting.rate.rateID);
                var newActivity = await SettingsHelper.Instance.op.GenerateMeetingAttendance(user, meeting.meetingID);
                newActivity.inTime = datePicker1.Date.DateTime + inTimeBox.Time;
                newActivity.outTime = datePicker1.Date.DateTime + outTimeBox.Time;
                newActivity.meeting.rate = meetingRate;
                var completedActivity = SettingsHelper.Instance.op.CompleteMeetingAttendance(newActivity, user, true);

                bool IsSuccess = await SettingsHelper.Instance.da.AddNewActivity(completedActivity);
                newActivity = await SettingsHelper.Instance.da.GetLatestMeeting(user.userID, completedActivity.locationID);
                completedActivity.ActivityID = newActivity.ActivityID;
                IsSuccess = await SettingsHelper.Instance.da.UpdateActivityInfo(completedActivity);
                if (IsSuccess)
                {
                    ContentDialog contentDialog = new ContentDialog()
                    {
                        Title = "Added successfully",
                        Content = "The duty has been added successfully.",
                        CloseButtonText = "Ok"
                    };

                    await contentDialog.ShowAsync();
                    this.Frame.GoBack();
                }
                else
                {
                    ContentDialog contentDialog = new ContentDialog()
                    {
                        Title = "An error occurred",
                        Content = "There's a problem in adding the duty. Please try again later. Tap on More info to see what's wrong.",
                        CloseButtonText = "Ok",
                        PrimaryButtonText = "More info"
                    };

                    var result2 = await contentDialog.ShowAsync();
                    if (result2 == ContentDialogResult.Primary)
                    {
                        contentDialog = new ContentDialog()
                        {
                            Title = "More info",
                            Content = SettingsHelper.Instance.da.lastError.Message + "\n" + SettingsHelper.Instance.da.lastError.StackTrace,
                            CloseButtonText = "Ok"
                        };

                        await contentDialog.ShowAsync();
                    }
                }
            }
        }

        private async void datePicker1_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            loadGrid.Visibility = Visibility.Visible;

            ObservableCollection<Meeting> meetings = await SettingsHelper.Instance.da.GetMeetings(SettingsHelper.Instance.appLocation);
            meetingBox.ItemsSource = meetings;

            loadGrid.Visibility = Visibility.Collapsed;
        }
    }
}