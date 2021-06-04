using System.Collections.Generic;

namespace IcpcResolver.Window
{
    public class ResolverConfig
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        public AwardUtilities Awards;
        public ResolverAnimationConfig AnimationConfig = new();
        public ContestSummary Contest;
        public List<Organization> Organizations;
        // school icon
        public string OrganizationIconPath;
        public bool EnableOrganizationIcon, EnableOrganizationIconFallback;
        // ReSharper restore FieldCanBeMadeReadOnly.Global
    }

    public class Organization
    {
        public string Id;
        public string Name;
    }
}