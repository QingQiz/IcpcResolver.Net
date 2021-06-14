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
        // team photo
        public string TeamPhotoPath;
        public bool EnableTeamPhoto, EnableTeamPhotoFallback;
        // ReSharper restore FieldCanBeMadeReadOnly.Global
    }
}