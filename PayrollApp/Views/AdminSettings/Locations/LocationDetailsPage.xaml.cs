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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views.AdminSettings.Locations
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LocationDetailsPage : Page
    {

        DispatcherTimer timeUpdater = new DispatcherTimer();
        DispatcherTimer loadTimer = new DispatcherTimer();
        PayrollCore.Entities.Location location;
        Shift specialTask;
        Shift shiftless;

        public LocationDetailsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                location = (e.Parameter as PayrollCore.Entities.Location);
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

            // Make loadGrid to visible when loading location data.
            // Starts timer that will get data on first tick.
            loadTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            loadTimer.Tick += LoadTimer_Tick;
            loadTimer.Start();

            loadGrid.Visibility = Visibility.Visible;


            if (location == null)
            {
                pageTitle.Text = "New location";
                enableButton.Visibility = Visibility.Collapsed;
                deleteButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (location.enableGM)
                {
                    enableMeetingSwitch.IsOn = true;
                }
                locationName.Text = location.locationName;
                if (location.isDisabled == true)
                {
                    deleteButton.Visibility = Visibility.Collapsed;
                    enableButton.Visibility = Visibility.Visible;
                }
                else
                {
                    deleteButton.Visibility = Visibility.Visible;
                    enableButton.Visibility = Visibility.Collapsed;
                }
            }

            if (SettingsHelper.Instance.InitState == SettingsHelper.InitStates.InitDb)
            {
                rootGrid.Children.Remove(manageMeetingsBtn);
            }
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private async void LoadTimer_Tick(object sender, object e)
        {
            loadTimer.Stop();

            ObservableCollection<Rate> rates = await SettingsHelper.Instance.op2.GetAllRates(false);
            defaultRateBox.ItemsSource = rates;
            defaultSpecialTaskRateBox.ItemsSource = rates;


            if (location != null)
            {
                shiftless = await SettingsHelper.Instance.op2.GetSpecialShift(location.locationID, "Normal sign in");
                specialTask = await SettingsHelper.Instance.op2.GetSpecialShift(location.locationID, "Special Task");

                // Assigns the default rate for shiftless task
                for (int i = 0; i < rates.Count - 1; i++)
                {
                    if (rates[i].rateID == shiftless.DefaultRate.rateID)
                    {
                        defaultRateBox.SelectedIndex = i;
                        break;
                    }
                }

                // Assigns the default rate for special task
                for (int i = 0; i < rates.Count -1; i++)
                {
                    if (rates[i].rateID == specialTask.DefaultRate.rateID)
                    {
                        defaultSpecialTaskRateBox.SelectedIndex = i;
                        break;
                    }
                }

                enableShiftlessMode.IsOn = location.Shiftless;
            }

            loadGrid.Visibility = Visibility.Collapsed;
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsHelper.Instance.InitState == SettingsHelper.InitStates.InitDb)
            {
                this.Frame.Navigate(typeof(FirstRunSetup.DbSetupPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            else if (SettingsHelper.Instance.InitState == SettingsHelper.InitStates.Setup)
            {
                this.Frame.Navigate(typeof(FirstRunSetup.LocationSetupPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            else
            {
                this.Frame.Navigate(typeof(LocationListPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
        }

        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            bool IsSuccess = await SaveLocation();
            if (IsSuccess)
            {
                if (SettingsHelper.Instance.InitState == SettingsHelper.InitStates.InitDb)
                {
                    this.Frame.Navigate(typeof(UserGroups.UserGroupDetailsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                }
                else
                {
                    this.Frame.GoBack();
                }
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Unable to save settings!",
                    Content = "Something happened that prevents location to be updated. Please try again later.",
                    PrimaryButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
        }

        private async void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            location.isDisabled = true;
            bool IsSuccess = await SaveLocation();
            if (IsSuccess)
            {
                this.Frame.GoBack();
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Unable to save settings!",
                    Content = "Something happened that prevents location to be updated. Please try again later.",
                    PrimaryButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
        }

        private void enableMeetingSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            location.enableGM = enableMeetingSwitch.IsOn;
        }

        private async void enableButton_Click(object sender, RoutedEventArgs e)
        {
            location.isDisabled = false;
            bool IsSuccess = await SaveLocation();
            if (IsSuccess)
            {
                this.Frame.GoBack();
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Unable to save settings!",
                    Content = "Something happened that prevents location to be updated. Please try again later.",
                    PrimaryButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
        }

        private async void manageMeetingsBtn_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = "Ready to save location?",
                Content = "Any changes will be saved and then we'll bring you to manage meetings in this location. Tap on Cancel to not make any changes.",
                PrimaryButtonText = "Save and manage meetings",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await contentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                bool IsSuccess = await SaveLocation();
                if (IsSuccess)
                {
                    this.Frame.Navigate(typeof(Meetings.MeetingListPage), location, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
                else
                {
                    contentDialog = new ContentDialog()
                    {
                        Title = "Unable to save location",
                        Content = "There is a problem in saving the location. Please try again later.",
                        CloseButtonText = "Ok"
                    };

                    await contentDialog.ShowAsync();
                }
            }
        }

        private async Task<bool> SaveLocation()
        {
            bool IsSuccess;
            loadGrid.Visibility = Visibility.Visible;

            if (location == null)
            {
                location = new PayrollCore.Entities.Location();
                location.isNewLocation = true;
                specialTask = new Shift()
                {
                    shiftName = "Special Task",
                    isDisabled = true
                };

                shiftless = new Shift()
                {
                    shiftName = "Normal sign in",
                    isDisabled = true
                };
            }

            location.locationName = locationName.Text;
            location.enableGM = enableMeetingSwitch.IsOn;
            specialTask.DefaultRate = defaultSpecialTaskRateBox.SelectedItem as Rate;
            shiftless.DefaultRate = defaultRateBox.SelectedItem as Rate;
            location.Shiftless = enableShiftlessMode.IsOn;

            if (location.isNewLocation == true)
            {
                int locationID = await SettingsHelper.Instance.op2.AddNewLocation(location);
                specialTask.locationID = locationID;
                shiftless.locationID = locationID;
                location.locationID = locationID;
                IsSuccess = await SettingsHelper.Instance.op2.AddNewShift(specialTask);
                IsSuccess = await SettingsHelper.Instance.op2.AddNewShift(shiftless);
            }
            else
            {
                IsSuccess = await SettingsHelper.Instance.op2.UpdateLocation(location);
                if (IsSuccess)
                {
                    IsSuccess = await SettingsHelper.Instance.op2.UpdateShift(specialTask);
                    IsSuccess = await SettingsHelper.Instance.op2.UpdateShift(shiftless);
                }
            }

            return IsSuccess;
        }

        private void enableMeetingSwitch_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Debug.WriteLine("switch clicked");
        }
    }
}
