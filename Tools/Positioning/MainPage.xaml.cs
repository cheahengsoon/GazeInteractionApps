﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using Windows.Devices.Input.Preview;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static Positioning.GazeHidInputReportHelpers;

namespace Positioning
{
    public sealed partial class MainPage : Page
    {
        private GazeInputSourcePreview gazeInputSourcePreview;
        const int GazeHidPage = 0x12;

        private DisplayInformation displayInformation;
        Size screenSize;
        float screenSizeInchesWidth;
        float screenSizeInchesHeight;

        float screenSizeMicrometersWidth;
        float screenSizeMicrometersHeight;

        public MainPage()
        {
            this.InitializeComponent();

            gazeInputSourcePreview = GazeInputSourcePreview.GetForCurrentView();
            gazeInputSourcePreview.GazeMoved += GazeInputSourcePreview_GazeMoved;

            displayInformation = DisplayInformation.GetForCurrentView();
            screenSize = new Size((int)displayInformation.ScreenWidthInRawPixels,
                                  (int)displayInformation.ScreenHeightInRawPixels);
            screenSizeInchesWidth = screenSize.Width / displayInformation.RawDpiX;
            screenSizeInchesHeight = screenSize.Height / displayInformation.RawDpiY;

            screenSizeMicrometersWidth = screenSizeInchesWidth * 25400;
            screenSizeMicrometersHeight = screenSizeInchesHeight * 25400;
        }

        private void GazeInputSourcePreview_GazeMoved(GazeInputSourcePreview sender, GazeMovedPreviewEventArgs args)
        {
            var sb = new StringBuilder();

            sb.Append("GazePos (");
            if (args.CurrentPoint.EyeGazePosition != null)
            {
                Canvas.SetLeft(GazePositionEllipse, args.CurrentPoint.EyeGazePosition.Value.X);
                Canvas.SetTop(GazePositionEllipse, args.CurrentPoint.EyeGazePosition.Value.Y);

                sb.Append($"{args.CurrentPoint.EyeGazePosition.Value.X}, {args.CurrentPoint.EyeGazePosition.Value.Y})");

                GazePositionEllipse.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                GazePositionEllipse.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            sb.AppendLine(")");

            if (args.CurrentPoint.HidInputReport != null)
            {
                var hidReport = args.CurrentPoint.HidInputReport;
                var sourceDevice = args.CurrentPoint.SourceDevice;

                var leftEyePositionParser = new GazePositionHidParser(sourceDevice, GazeExtendedUsages.Usage_LeftEyePosition);
                var rightEyePositionParser = new GazePositionHidParser(sourceDevice, GazeExtendedUsages.Usage_RightEyePosition);

                var leftEyePosition = leftEyePositionParser.GetPosition(hidReport);
                var rightEyePosition = rightEyePositionParser.GetPosition(hidReport);

                // The return values for the left and right eye are in micrometers by default
                // They are references such that X and Y origin are the top left hand corner 
                // of the calibrated display. The Z origin is the centerpoint of the display
                // (not the tracker). As such, there is a minor difference between the actual
                // sensor-to-eye distance vs the reported distance for left/right eye position.

                sb.Append("LeftEyePos  (");
                if (leftEyePosition != null)
                {
                    sb.Append($"{leftEyePosition.Value.X.ToString("F2", CultureInfo.InvariantCulture)}, {leftEyePosition.Value.Y.ToString("F2", CultureInfo.InvariantCulture)}, {leftEyePosition.Value.Z.ToString("F2", CultureInfo.InvariantCulture)})");

                    if (leftEyePosition.Value.X >= 0 &&
                        leftEyePosition.Value.X <= screenSizeMicrometersWidth &&
                        leftEyePosition.Value.Y >= 0 &&
                        leftEyePosition.Value.Y <= screenSizeMicrometersHeight)
                    {
                        var newX = MapRange(0, screenSizeMicrometersWidth, 0, ActualWidth, leftEyePosition.Value.X);
                        var newY = MapRange(0, screenSizeMicrometersHeight, 0, ActualHeight, leftEyePosition.Value.Y);

                        var newZ = 0;
                        if (leftEyePosition.Value.Z < 400000)
                        {
                            newZ = 1;
                            LeftEyePositionEllipse.Fill = new SolidColorBrush(Colors.Red);
                        }
                        else if (leftEyePosition.Value.Z < 500000)
                        {
                            newZ = 2;
                            LeftEyePositionEllipse.Fill = new SolidColorBrush(Colors.Yellow);
                        }
                        else if (leftEyePosition.Value.Z < 700000)
                        {
                            newZ = 3;
                            LeftEyePositionEllipse.Fill = new SolidColorBrush(Colors.Green);
                        }
                        else if (leftEyePosition.Value.Z < 800000)
                        {
                            newZ = 2;
                            LeftEyePositionEllipse.Fill = new SolidColorBrush(Colors.Yellow);
                        }
                        else if (leftEyePosition.Value.Z < 900000)
                        {
                            newZ = 1;
                            LeftEyePositionEllipse.Fill = new SolidColorBrush(Colors.Red);
                        }

                        sb.Append($" ({newX.ToString("F0", CultureInfo.InvariantCulture)}, {newY.ToString("F0", CultureInfo.InvariantCulture)}, {newZ})");

                        Canvas.SetLeft(LeftEyePositionEllipse, newX);
                        Canvas.SetTop(LeftEyePositionEllipse, newY);

                        LeftEyePositionEllipse.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                    else
                    {
                        LeftEyePositionEllipse.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                }
                sb.AppendLine(")");

                sb.Append("RightEye (");
                if (rightEyePosition != null)
                {
                    sb.Append($"{rightEyePosition.Value.X.ToString("F2", CultureInfo.InvariantCulture)}, {rightEyePosition.Value.Y.ToString("F2", CultureInfo.InvariantCulture)}, {rightEyePosition.Value.Z.ToString("F2", CultureInfo.InvariantCulture)})");

                    if (rightEyePosition.Value.X >= 0 &&
                        rightEyePosition.Value.X <= screenSizeMicrometersWidth &&
                        rightEyePosition.Value.Y >= 0 &&
                        rightEyePosition.Value.Y <= screenSizeMicrometersHeight)
                    {
                        var newX = MapRange(0, screenSizeMicrometersWidth, 0, ActualWidth, rightEyePosition.Value.X);
                        var newY = MapRange(0, screenSizeMicrometersHeight, 0, ActualHeight, rightEyePosition.Value.Y);

                        var newZ = 0;
                        if (rightEyePosition.Value.Z < 400000)
                        {
                            newZ = 1;
                            RightEyePositionEllipse.Fill = new SolidColorBrush(Colors.Red);
                        }
                        else if (rightEyePosition.Value.Z < 500000)
                        {
                            newZ = 2;
                            RightEyePositionEllipse.Fill = new SolidColorBrush(Colors.Yellow);
                        }
                        else if (rightEyePosition.Value.Z < 700000)
                        {
                            newZ = 3;
                            RightEyePositionEllipse.Fill = new SolidColorBrush(Colors.Green);
                        }
                        else if (rightEyePosition.Value.Z < 800000)
                        {
                            newZ = 2;
                            RightEyePositionEllipse.Fill = new SolidColorBrush(Colors.Yellow);
                        }
                        else if (rightEyePosition.Value.Z < 900000)
                        {
                            newZ = 1;
                            RightEyePositionEllipse.Fill = new SolidColorBrush(Colors.Red);
                        }

                        sb.Append($" ({newX.ToString("F0", CultureInfo.InvariantCulture)}, {newY.ToString("F0", CultureInfo.InvariantCulture)}, {newZ}");

                        Canvas.SetLeft(RightEyePositionEllipse, newX);
                        Canvas.SetTop(RightEyePositionEllipse, newY);

                        RightEyePositionEllipse.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                    else
                    {
                        RightEyePositionEllipse.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                }
                sb.AppendLine(")");

                var headPostitionParser = new GazePositionHidParser(sourceDevice, GazeExtendedUsages.Usage_HeadPosition);
                var headPosition = headPostitionParser.GetPosition(hidReport);
                if (headPosition != null)
                {
                    sb.AppendLine($"HeadPosition ({headPosition.Value.X.ToString("F2", CultureInfo.InvariantCulture)}, {headPosition.Value.Y.ToString("F2", CultureInfo.InvariantCulture)}, {headPosition.Value.Z.ToString("F2", CultureInfo.InvariantCulture)})");
                }

                var headRotationParser = new GazeRotationHidParser(sourceDevice, GazeExtendedUsages.Usage_HeadDirectionPoint);
                var headRotation = headRotationParser.GetRotation(hidReport);

                if (headRotation != null)
                {
                    sb.AppendLine($"HeadRotation ({headRotation.Value.X.ToString("F2", CultureInfo.InvariantCulture)}, {headRotation.Value.Y.ToString("F2", CultureInfo.InvariantCulture)}, {headRotation.Value.Z.ToString("F2", CultureInfo.InvariantCulture)})");
                }
            }

            StatusTextBlock.Text = sb.ToString();
        }

        private static double MapRange(double oldStart, double oldEnd, double newStart, double newEnd, double valueToMap)
        {
            double scalingFactor = (double)(newEnd - newStart) / (oldEnd - oldStart);
            return (double)(newStart + ((valueToMap - oldStart) * scalingFactor));
        }

        private void ExitButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            App.Current.Exit();
        }
    }
}