namespace IcpcResolver.Window
{
    public class ResolverStatus
    {
        public bool AnimationDone = true;
        public bool ScrollDown = false;
        public bool ShouldCursorUp = false;
        public bool ShouldShowAward = false;
        public int CurrentTeamIdx = -1;
        public int CursorIdx = -1;

        public void AniStart()
        {
            AnimationDone = false;
        }

        public void AniEnd()
        {
            AnimationDone = true;
        }
    }
}