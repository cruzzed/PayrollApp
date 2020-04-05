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
using ServiceHelpers;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Windows.Storage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views.NewUserOnboarding
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FaceRecSetupPage : Page
    {

        DispatcherTimer timeUpdater = new DispatcherTimer();
        DispatcherTimer loadTimer = new DispatcherTimer();
        string username = SettingsHelper.Instance.userState.user.userID;

        public FaceRecSetupPage()
        {
            this.InitializeComponent();
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

            this.cameraControl.FilterOutSmallFaces = true;
        }

        private async void LoadTimer_Tick(object sender, object e)
        {
            loadTimer.Stop();

            bool LoadPersonResult = await SettingsHelper.Instance.LoadRegisteredPeople();
            if (LoadPersonResult == true)
            {
                bool IsUserFound = SettingsHelper.Instance.SelectPeople(username);
                if (IsUserFound != true)
                {
                    bool CreatePersonSuccess = await SettingsHelper.Instance.CreatePersonAsync(username);

                    if (CreatePersonSuccess == true)
                    {
                        loadGrid.Visibility = Visibility.Collapsed;
                        return;
                    }
                }
            }
            
            ContentDialog contentDialog = new ContentDialog
            {
                Title = "Unable to register your face at this time.",
                Content = "A problem occurred that prevents us to setup your facial recognition settings. You can register your face by selecting the 'Improve recognition' button after you login to Payroll.",
                CloseButtonText = "Ok"
            };
            
            await contentDialog.ShowAsync();
            this.Frame.Navigate(typeof(UserProfile.UserProfilePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void cameraControl_ImageCaptured(object sender, ImageAnalyzer e)
        {
            loadText.Text = "Detecting faces...";
            loadGrid.Visibility = Visibility.Visible;

            // Detects and identify if there is any faces on the image.
            await e.DetectFacesAsync();
            if (!e.DetectedFaces.Any())
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "No faces detected",
                    Content = "There are no faces detected on the image. Please stand in front of the camera and make sure that a box is drawn around your face and select the capture button.",
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
                this.Frame.Navigate(typeof(UserProfile.UserProfilePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }

            await e.IdentifyFacesAsync();
            if (e.IdentifiedPersons.Any())
            {
                string identifiedUsername = e.IdentifiedPersons.First().Person.Name;
                if (identifiedUsername != SettingsHelper.Instance.SelectedPerson.Name)
                {
                    ContentDialog contentDialog = new ContentDialog
                    {
                        Title = "Another person is detected in the photo",
                        Content = "Another person is identified in the submitted photo. Make sure that you are registering your face for one account only or make sure that there is only one person captured on the image. If you believe that there is an error, please contact Chiefs or HR Functional Unit to help you register for facial recognition.",
                        CloseButtonText = "Ok"
                    };

                    await contentDialog.ShowAsync();
                    this.Frame.Navigate(typeof(UserProfile.UserProfilePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
            }

            // Crop the primary face
            FaceRectangle rect = e.DetectedFaces.First().FaceRectangle;
            double heightScaleFactor = 1.8;
            double widthScaleFactor = 1.8;
            FaceRectangle biggerRectangle = new FaceRectangle
            {
                Height = Math.Min((int)(rect.Height * heightScaleFactor), e.DecodedImageHeight),
                Width = Math.Min((int)(rect.Width * widthScaleFactor), e.DecodedImageWidth)
            };
            biggerRectangle.Left = Math.Max(0, rect.Left - (int)(rect.Width * ((widthScaleFactor - 1) / 2)));
            biggerRectangle.Top = Math.Max(0, rect.Top - (int)(rect.Height * ((heightScaleFactor - 1) / 1.4)));

            StorageFile tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
                                                    "FaceRecoCameraCapture.jpg",
                                                    CreationCollisionOption.GenerateUniqueName);

            await Util.CropBitmapAsync(e.GetImageStreamCallback, biggerRectangle, tempFile);

            var croppedImage = new ImageAnalyzer(tempFile.OpenStreamForReadAsync, tempFile.Path);

            loadText.Text = "Uploading your image...";

            try
            {

            }
        }
    }
}
