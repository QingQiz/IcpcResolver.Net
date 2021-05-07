using System;
using System.Collections.Generic;
using System.Linq;
using IcpcResolver.Net.AppConstants;
using IcpcResolver.Net.UserControl;

namespace IcpcResolver.Net.Window
{
    /// <summary>
    /// Interaction logic for Loader.xaml
    /// </summary>
    public partial class Loader : System.Windows.Window
    {
        public Loader()
        {
            InitializeComponent();

            var teams = ResolverDto.DataGenerator(13, 20);

            const int teamGridHeight = AppConst.TeamGridHeight;

            var resolver = new Resolver(new ResolverDto
            {
                ResolverConfig = new ResolverConfig
                {
                    TeamGridHeight = teamGridHeight,
                    MaxDisplayCount = AppConst.MaxDisplayCount + 1,
                    MaxRenderCount = AppConst.MaxDisplayCount + 5,
                    ScrollDownDuration = 200,
                    ScrollDownDurationAdjust = 0,
                    CursorUpDuration = 500,
                    UpdateTeamRankDuration = 1000,
                    AnimationFrameRate = 120,
                    UpdateProblemStatusDuration = new Tuple<int, int>(400, 600),
                    Awards = new List<Tuple<int, string>>() 
                },
                Teams = teams.Select(t => new Team(t)
                {
                    Height = teamGridHeight
                }).ToList()
            });

            resolver.Show();
            Close();
        }
    }
}
