﻿//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Microsoft.Research.Input.Gaze;
using Windows.ApplicationModel;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EyeControlToolkitSettings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private GazePointer _gazePointer;

        public MainPage()
        {
            InitializeComponent();

            _gazePointer = new GazePointer(this);
            _gazePointer.OnGazePointerEvent += OnGazePointerEvent;

            var packagefamilyname = Package.Current.Id.FamilyName;

            var results = App.SettingsValues;

            BetaTextbox.Text = results["OneEuroFilter_Beta"].ToString();
            CutoffTextbox.Text = results["OneEuroFilter_Cutoff"].ToString();
            VelocityCutoffTextbox.Text = results["OneEuroFilter_Velocity_Cutoff"].ToString();
        }

        private void OnGazePointerEvent(GazePointer sender, GazePointerEventArgs ea)
        {
            if (ea.PointerState == GazePointerState.Dwell)
            {
                _gazePointer.InvokeTarget(ea.HitTarget);
            }
        }

        private void GetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var results = App.SettingsValues;

            BetaTextbox.Text = results["OneEuroFilter_Beta"].ToString();
            CutoffTextbox.Text = results["OneEuroFilter_Cutoff"].ToString();
            VelocityCutoffTextbox.Text = results["OneEuroFilter_Velocity_Cutoff"].ToString();
        }

        private void SetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            App.SettingsValues = new ValueSet
            {
                { "OneEuroFilter_Beta", float.Parse(BetaTextbox.Text) },
                { "OneEuroFilter_Cutoff", float.Parse(CutoffTextbox.Text) },
                { "OneEuroFilter_Velocity_Cutoff", float.Parse(VelocityCutoffTextbox.Text) }
            };
        }
    }
}
