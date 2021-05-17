using System;
using IcpcResolver.AppConstants;

namespace IcpcResolver.Window
{
    public class ResolverConfig
    {
        public int TeamGridHeight = AppConst.TeamGridHeight;
        public int MaxDisplayCount = AppConst.MaxDisplayCount;

        /// <summary>
        /// team count to render on init
        /// </summary>
        public int MaxRenderCount = AppConst.MaxDisplayCount + 5;

        /// <summary>
        /// see `ScrollDownAnimation`
        /// </summary>
        public int ScrollDownDuration = 200;

        /// <summary>
        /// see `ScrollDownAnimation`
        /// </summary>
        public int ScrollDownInterval = 0;

        /// <summary>
        /// see `CursorUpAnimation`
        /// </summary>
        public int CursorUpDuration = 500;

        /// <summary>
        /// see `UpdateTeamRankAnimation`
        /// </summary>
        public int UpdateTeamRankDuration = 1000;

        /// <summary>
        /// frame rate for each animation
        /// </summary>
        public int AnimationFrameRate = 120;

        /// <summary>
        /// (duration before highlight problem, duration before update problem status)
        /// </summary>
        public Tuple<int, int> UpdateProblemStatusDuration = new(400, 600);

        /// <summary>
        /// auto update team status until rank le this
        /// </summary>
        public int AutoUpdateTeamStatusUntilRank;
    }
}