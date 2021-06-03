using System.Collections.Generic;

namespace IcpcResolver.Window
{
    public class ResolverConfig
    {
        public AwardUtilities Awards;
        public ResolverAnimationConfig AnimationConfig = new();
        public ContestSummary Contest;
        public List<Organization> Organizations;
    }

    public class Organization
    {
        public string Id;
        public string Name;
    }
}