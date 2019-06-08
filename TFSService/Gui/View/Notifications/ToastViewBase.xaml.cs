using System;
using System.Windows;
using System.Windows.Input;
using ToastNotifications.Core;
using FlowDirection = System.Windows.Forms.FlowDirection;

namespace Gui.View.Notifications
{
    /// <summary>
    ///     Interaction logic for ToastViewBase.xaml
    /// </summary>
    public partial class ToastViewBase : NotificationDisplayPart
    {
        public ToastViewBase()
        {
            InitializeComponent();

            Loaded += SubscribeOnce;
        }

        private void SubscribeOnce(object sender, RoutedEventArgs e)
        {
            Loaded -= SubscribeOnce;

            MouseDown += (s, args) => DetectClosingSlide(args);
            MouseUp += (s, args) =>
            {
                DetectClosingSlide(args);

                if (!_isClosed && Notification.CanClose)
                    Notification.Options?.NotificationClickAction?.Invoke(Notification as NotificationBase);
            };
        }

        #region Slide closing

        private void DetectClosingSlide(MouseButtonEventArgs e)
        {
            var isPressed = e.LeftButton == MouseButtonState.Pressed
                            || e.RightButton == MouseButtonState.Pressed;

            var pos = e.GetPosition(this);

            if (_isClosed) e.Handled = true;

            if (!TryClose(pos, isPressed)) TryStartDragging(pos, isPressed);
        }

        private bool _isClosed;
        private bool _isDragging;
        private Point _lastPos;

        private bool TryStartDragging(Point pos, bool isPressed)
        {
            if (_isDragging
                || !isPressed)
                return false;

            _isClosed = false;
            _isDragging = true;
            _lastPos = pos;
            CaptureMouse();
            return _isDragging;
        }

        private bool TryClose(Point pos, bool isPressed)
        {
            if (_isDragging
                && DetectClosingSlide(pos, System.Windows.Forms.FlowDirection.TopDown))
            {
                _isDragging = false;
                _isClosed = true;
                OnClose();
                ReleaseMouseCapture();
                return true;
            }

            return false;
        }

        private bool DetectClosingSlide(Point endPoint, params FlowDirection[] directions)
        {
            var delta = _lastPos - endPoint;

            foreach (var o in directions)
                switch (o)
                {
                    case System.Windows.Forms.FlowDirection.LeftToRight:
                        return ActualWidth / 2 < Math.Abs(delta.X);

                    case System.Windows.Forms.FlowDirection.TopDown:
                        return ActualHeight / 2 < Math.Abs(delta.Y);

                    case System.Windows.Forms.FlowDirection.RightToLeft:
                        return ActualWidth / 2 < delta.X;

                    case System.Windows.Forms.FlowDirection.BottomUp:
                        return ActualHeight / 2 < delta.Y;

                    default:
                        throw new Exception("Wrong parameter");
                }

            return false;
        }

        #endregion
    }
}