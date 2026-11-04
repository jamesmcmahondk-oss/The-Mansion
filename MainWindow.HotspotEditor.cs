using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using The_Mansion.Data;
using The_Mansion.Models;
using System.Windows.Threading;

namespace The_Mansion
{
    // Debug hotspot editor: drag on the scene to measure a new hotspot and copy its code.
    // (Part of MainWindow - see MainWindow.xaml.cs.)
    public partial class MainWindow
    {
        private void SceneCanvas_MouseMove(
            object sender,
            MouseEventArgs e)
        {
            Point currentPosition =
                e.GetPosition(SceneCanvas);

            if (_hoveredHotspot != null)
            {
                UpdateHoverLabel(currentPosition);
            }

            if (_showHotspotDebug)
            {
                Point imagePosition = ScreenToImage(
                    new Rect(currentPosition, new Size(0, 0))).Location;

                DebugCoordinates.Text =
                    $"Image: X={imagePosition.X:0} " +
                    $"Y={imagePosition.Y:0}  " +
                    $"(screen {currentPosition.X:0},{currentPosition.Y:0})";
            }

            if (!_showHotspotDebug ||
                !_isCreatingHotspot ||
                _selectionRectangle == null)
            {
                return;
            }

            double movementX =
                Math.Abs(currentPosition.X - _dragStartPoint.X);

            double movementY =
                Math.Abs(currentPosition.Y - _dragStartPoint.Y);

            if (movementX >= DragThreshold ||
                movementY >= DragThreshold)
            {
                _hasDragged = true;
            }

            UpdateSelectionRectangle(currentPosition);
        }

        private void SceneCanvas_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (!_showHotspotDebug)
            {
                return;
            }

            /*
             * If the mouse is over an existing hotspot, require Shift
             * to start drawing a new selection over it.
             */
            if (e.OriginalSource is Rectangle existingRectangle &&
                existingRectangle.Tag is Hotspot &&
                !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                return;
            }

            _dragStartPoint =
                ClampPointToCanvas(
                    e.GetPosition(SceneCanvas)
                );

            _isCreatingHotspot = true;
            _hasDragged = false;

            RemoveSelectionRectangle();

            _selectionRectangle = new Rectangle
            {
                Width = 0,
                Height = 0,

                Fill = new SolidColorBrush(
                    Color.FromArgb(80, 0, 200, 255)
                ),

                Stroke = Brushes.Cyan,
                StrokeThickness = 3,

                StrokeDashArray =
                    new DoubleCollection
                    {
                        5,
                        3
                    },

                IsHitTestVisible = false
            };

            Canvas.SetLeft(
                _selectionRectangle,
                _dragStartPoint.X
            );

            Canvas.SetTop(
                _selectionRectangle,
                _dragStartPoint.Y
            );

            Panel.SetZIndex(
                _selectionRectangle,
                100
            );

            SceneCanvas.Children.Add(
                _selectionRectangle
            );

            SceneCanvas.CaptureMouse();

            DebugSelection.Text =
                $"Start: X={_dragStartPoint.X:0} " +
                $"Y={_dragStartPoint.Y:0}";

            e.Handled = true;
        }

        private void SceneCanvas_MouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            if (!_showHotspotDebug ||
                !_isCreatingHotspot ||
                _selectionRectangle == null)
            {
                return;
            }

            Point endPoint =
                ClampPointToCanvas(
                    e.GetPosition(SceneCanvas)
                );

            UpdateSelectionRectangle(endPoint);

            _isCreatingHotspot = false;

            SceneCanvas.ReleaseMouseCapture();

            if (!_hasDragged)
            {
                RemoveSelectionRectangle();

                DebugSelection.Text =
                    "Drag to create a hotspot";

                return;
            }

            double x =
                Canvas.GetLeft(_selectionRectangle);

            double y =
                Canvas.GetTop(_selectionRectangle);

            double width =
                _selectionRectangle.Width;

            double height =
                _selectionRectangle.Height;

            /*
             * Scenes.cs stores hotspots in source-image pixels,
             * so convert the on-screen selection first.
             */
            Rect imageRect = ScreenToImage(
                new Rect(x, y, width, height));

            x = imageRect.X;
            y = imageRect.Y;
            width = imageRect.Width;
            height = imageRect.Height;

            /*
             * Round the coordinates because whole-number positions
             * are easier to use in Scenes.cs.
             */
            x = Math.Round(x);
            y = Math.Round(y);
            width = Math.Round(width);
            height = Math.Round(height);

            string hotspotCode =
                CreateHotspotCode(
                    x,
                    y,
                    width,
                    height
                );

            DebugSelection.Text =
                $"Selection: X={x:0} Y={y:0} " +
                $"W={width:0} H={height:0}\n" +
                "Hotspot code copied to clipboard.";

            try
            {
                Clipboard.SetText(hotspotCode);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"The hotspot was created, but its code " +
                    $"could not be copied to the clipboard.\n\n" +
                    $"{ex.Message}",
                    "Clipboard error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }

            e.Handled = true;
        }

        private void UpdateSelectionRectangle(
            Point currentPosition)
        {
            if (_selectionRectangle == null)
            {
                return;
            }

            currentPosition =
                ClampPointToCanvas(currentPosition);

            double x =
                Math.Min(
                    _dragStartPoint.X,
                    currentPosition.X
                );

            double y =
                Math.Min(
                    _dragStartPoint.Y,
                    currentPosition.Y
                );

            double width =
                Math.Abs(
                    currentPosition.X -
                    _dragStartPoint.X
                );

            double height =
                Math.Abs(
                    currentPosition.Y -
                    _dragStartPoint.Y
                );

            Canvas.SetLeft(
                _selectionRectangle,
                x
            );

            Canvas.SetTop(
                _selectionRectangle,
                y
            );

            _selectionRectangle.Width = width;
            _selectionRectangle.Height = height;

            DebugSelection.Text =
                $"Selection: X={x:0} Y={y:0} " +
                $"W={width:0} H={height:0}";
        }

        private Point ClampPointToCanvas(Point point)
        {
            double x = Math.Max(
                0,
                Math.Min(
                    point.X,
                    SceneCanvas.ActualWidth
                )
            );

            double y = Math.Max(
                0,
                Math.Min(
                    point.Y,
                    SceneCanvas.ActualHeight
                )
            );

            return new Point(x, y);
        }

        private string CreateHotspotCode(
            double x,
            double y,
            double width,
            double height)
        {
            /*
             * InvariantCulture ensures periods are used as decimal
             * separators regardless of Windows regional settings.
             */
            string xText =
                x.ToString(
                    "0",
                    CultureInfo.InvariantCulture
                );

            string yText =
                y.ToString(
                    "0",
                    CultureInfo.InvariantCulture
                );

            string widthText =
                width.ToString(
                    "0",
                    CultureInfo.InvariantCulture
                );

            string heightText =
                height.ToString(
                    "0",
                    CultureInfo.InvariantCulture
                );

            // Paste into a scene in Data/Scenes.cs (see SCENE_GUIDE.md).
            return
$@"            .Hotspot(""New Hotspot"", {xText}, {yText}, {widthText}, {heightText})
                .Describe(""Describe the hotspot here."")";
        }

        private void RemoveSelectionRectangle()
        {
            if (_selectionRectangle == null)
            {
                return;
            }

            SceneCanvas.Children.Remove(
                _selectionRectangle
            );

            _selectionRectangle = null;
        }

        private void UpdateDebugVisibility()
        {
            DebugPanel.Visibility =
                _showHotspotDebug
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }
    }
}
