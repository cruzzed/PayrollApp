﻿using PayrollCore.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views.UserProfile.SpecialTask
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SignOutPage : Page
    {
        public SignOutPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            timeUpdater.Stop();
            base.OnNavigatedFrom(e);
        }

        DispatcherTimer timeUpdater = new DispatcherTimer();
        DispatcherTimer loadTimer = new DispatcherTimer();
        bool IsSpecialTask = true;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                IsSpecialTask = (bool)e.Parameter;
            }
            base.OnNavigatedTo(e);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsSpecialTask == false)
            {
                actualContent.Visibility = Visibility.Collapsed;
            }
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

            if (SettingsHelper.Instance.userState != null)
            {
                Activity activity = await SettingsHelper.Instance.op2.CompleteWorkActivity(SettingsHelper.Instance.userState.LatestActivity,
                    SettingsHelper.Instance.userState.user, false);

                bool IsSuccess = await SettingsHelper.Instance.op2.UpdateActivity(activity);
                if (IsSuccess)
                {
                    pageContent.Visibility = Visibility.Visible;
                }
                else
                {
                    ContentDialog contentDialog = new ContentDialog
                    {
                        Title = "Unable to sign out",
                        Content = "There's a problem signing you out. Please try again later. If the problem persists, please contact Chiefs or HR Functional Unit to sign you out.",
                        PrimaryButtonText = "Ok",
                        SecondaryButtonText = "More info"
                    };

                    await contentDialog.ShowAsync();
                    this.Frame.Navigate(typeof(LoginPagev2), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                }
            }
            else
            {
                this.Frame.Navigate(typeof(LoginPagev2), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }

            await SettingsHelper.Instance.UpdateUserState(SettingsHelper.Instance.userState.user);

            loadGrid.Visibility = Visibility.Collapsed;
        }


        private void TimeUpdater_Tick(object sender, object e)
        {
            //SettingsHelper.Instance.userState = null;
            //this.Frame.Navigate(typeof(LoginPagev2), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserProfilePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
    }
}
