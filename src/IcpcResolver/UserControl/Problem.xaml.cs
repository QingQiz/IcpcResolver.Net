using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Colors = IcpcResolver.AppConstants.Colors;

namespace IcpcResolver.UserControl
{
    public partial class Problem
    {
        private Problem()
        {
            InitializeComponent();
        }

        public Problem(ProblemDto problem) : this()
        {
            _label = problem.Label;
            _time = problem.Time;
            _try = problem.Try;
            Status = problem.Status;
        }

        private static string GetStatusColor(ProblemStatus status)
        {
            return status switch
            {
                ProblemStatus.Accept => Colors.Green,
                ProblemStatus.UnAccept => Colors.Red,
                ProblemStatus.Pending => Colors.Yellow,
                ProblemStatus.NotTried => Colors.DarkGray,
                ProblemStatus.FirstBlood => Colors.DarkGreen,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }

        private int _try;
        private int _time;
        private readonly string _label;

        private ProblemStatus _status;

        private ProblemStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                BgColor = Colors.ToColorBrush(GetStatusColor(_status));
                FontColor = Colors.ToColorBrush(_status == ProblemStatus.NotTried ? Colors.Gray : Colors.White);
                BorderColor = _status == ProblemStatus.FirstBlood
                    ? Colors.ToColorBrush(GetStatusColor(ProblemStatus.Accept))
                    : BgColor;
                LabelOrContent = _status == ProblemStatus.NotTried ? _label : $"{_try} - {_time}";
            }
        }

        public SolidColorBrush BorderColor
        {
            get => (SolidColorBrush) GetValue(BorderColorProperty);
            private set => SetValue(BorderColorProperty, value);
        }

        private static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register("BorderColor", typeof(SolidColorBrush), typeof(Problem));


        public SolidColorBrush FontColor
        {
            get => (SolidColorBrush) GetValue(FontColorProperty);
            private set => SetValue(FontColorProperty, value);
        }

        private static readonly DependencyProperty FontColorProperty =
            DependencyProperty.Register("FontColor", typeof(SolidColorBrush), typeof(Problem));


        public SolidColorBrush BgColor
        {
            get => (SolidColorBrush) GetValue(BgColorProperty);
            private set => SetValue(BgColorProperty, value);
        }

        private static readonly DependencyProperty BgColorProperty =
            DependencyProperty.Register("BgColor", typeof(SolidColorBrush), typeof(Problem));

        public string LabelOrContent
        {
            get => (string) GetValue(LabelOrContentProperty);
            private set => SetValue(LabelOrContentProperty, value);
        }

        private static readonly DependencyProperty LabelOrContentProperty =
            DependencyProperty.Register("LabelOrContent", typeof(string), typeof(Problem));

        public async Task UpdateStatusAnimation(ProblemDto to, int durationBeforeHighlight, int durationBeforeUpdate)
        {
            if (Status != ProblemStatus.Pending || to.Status == ProblemStatus.Pending) return;

            await Task.Delay(durationBeforeHighlight);
            BorderColor = Colors.ToColorBrush(Colors.LightYellow);

            await Task.Delay(durationBeforeUpdate);
            Status = to.Status;
            _try = to.Try;
            _time = to.Time;
        }
    }
}