﻿using Microsoft.Toolkit.Graph.Providers;
using System;
using System.Collections.Generic;
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
using PayrollCore.Entities;
using ServiceHelpers;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AppInitPage : Page
    {
        DispatcherTimer loadTimer = new DispatcherTimer();
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            loadTimer.Interval = new TimeSpan(0, 0, 1);
            loadTimer.Tick += LoadTimer_Tick;
            base.OnNavigatedTo(e);
        }

        private async void LoadTimer_Tick(object sender, object e)
        {
            loadTimer.Stop();
            
            if (SettingsHelper.Instance.InitState == SettingsHelper.InitStates.InProgress)
            {
                loadTimer.Interval = new TimeSpan(0, 0, 5);
                loadTimer.Start();
                return;
            }

            if (SettingsHelper.Instance.InitState == SettingsHelper.InitStates.Success)
            {
                this.Frame.Navigate(typeof(LoginPagev2), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
            else
            {
                this.Frame.Navigate(typeof(FirstRunSetup.WelcomePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }

        public AppInitPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            loadTimer.Start();
        }
    }
}
