using System;
using System.Windows;
using IcpcResolver.Net.AppConstants;

namespace IcpcResolver.Net.UserControl
{
    public partial class Problem : System.Windows.Controls.UserControl
    {
        private Problem()
        {
            InitializeComponent();
        }
        
        public Problem(ProblemDto problem) : this()
        {
            _status = problem.Status;
            _label = problem.Label;
            _time = problem.Time;
            _try = problem.Try;

            Color = GetStatusColor(_status);
            FontColor = _status == ProblemStatus.NotTried ? Colors.Gray : Colors.White;
            BorderColor = _status == ProblemStatus.FirstBlood ? GetStatusColor(ProblemStatus.Accept) : Color;
            LabelOrContent = _status == ProblemStatus.NotTried ? _label : $"{_try} - {_time}";
        }

        private static string GetStatusColor(ProblemStatus status)
        {
            return status switch
            {
                ProblemStatus.Accept     => Colors.Green,
                ProblemStatus.UnAccept   => Colors.Red,
                ProblemStatus.Pending    => Colors.Yellow,
                ProblemStatus.NotTried   => Colors.DarkGray,
                ProblemStatus.FirstBlood => Colors.DarkGreen,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }

        private int _try;
        private int _time;
        private string _label;
        private ProblemStatus _status;

        public string BorderColor
        {
            get => (string) GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        private static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register("BorderColor", typeof(string), typeof(Problem));


        public string FontColor
        {
            get => (string) GetValue(FontColorProperty);
            set => SetValue(FontColorProperty, value);
        }

        private static readonly DependencyProperty FontColorProperty =
            DependencyProperty.Register("FontColor", typeof(string), typeof(Problem));


        public string Color
        {
            get => (string) GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        private static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(string), typeof(Problem));

        public string LabelOrContent
        {
            get => (string) GetValue(LabelOrContentProperty);
            set => SetValue(LabelOrContentProperty, value);
        }

        private static readonly DependencyProperty LabelOrContentProperty =
            DependencyProperty.Register("LabelOrContent", typeof(string), typeof(Problem));
    }
}