using System;
using System.Threading.Tasks;
using System.Windows;
using Colors = IcpcResolver.Net.AppConstants.Colors;

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
        private string _label;

        private ProblemStatus _status;
        public ProblemStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                BgColor = GetStatusColor(_status);
                FontColor = _status == ProblemStatus.NotTried ? Colors.Gray : Colors.White;
                BorderColor = _status == ProblemStatus.FirstBlood ? GetStatusColor(ProblemStatus.Accept) : BgColor;
                LabelOrContent = _status == ProblemStatus.NotTried ? _label : $"{_try} - {_time}";
            }
        }

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


        public string BgColor
        {
            get => (string) GetValue(BgColorProperty);
            set => SetValue(BgColorProperty, value);
        }

        private static readonly DependencyProperty BgColorProperty =
            DependencyProperty.Register("BgColor", typeof(string), typeof(Problem));

        public string LabelOrContent
        {
            get => (string) GetValue(LabelOrContentProperty);
            set => SetValue(LabelOrContentProperty, value);
        }

        private static readonly DependencyProperty LabelOrContentProperty =
            DependencyProperty.Register("LabelOrContent", typeof(string), typeof(Problem));

        public async Task UpdateStatusAnimation(ProblemDto to, int durationBeforeHighlight, int durationBeforeUpdate)
        {
            if (Status != ProblemStatus.Pending || to.Status == ProblemStatus.Pending) return;

            await Task.Delay(durationBeforeHighlight);
            BorderColor = Colors.LightYellow;

            await Task.Delay(durationBeforeUpdate);
            Status = to.Status;
            _try = to.Try;
            _time = to.Time;
        }
    }
}