using System.Linq;
using System.Text;

namespace SlackCommander.Web.FullContact
{
    public static class FullContactPersonResultExtensions
    {
        public static string FormattedSummary(this FullContactPersonResult person)
        {
            var fullName = person.Result.ContactInfo.FullName;
            var location = person.Result.Demographics.LocationGeneral;
            var currentOrganizations = person.Result.Organizations
                .Where(o => o.Current == true)
                .OrderByDescending(o => o.IsPrimary == true);
            var totalFollowers = person.Result.SocialProfiles.Sum(profile => profile.Followers);
            var photo = person.Result.Photos
                .OrderByDescending(p => p.IsPrimary == true)
                .FirstOrDefault();

            var text = new StringBuilder();
            if (!fullName.Missing())
            {
                text.AppendFormat("*{0}*", fullName);
                if (!location.Missing())
                {
                    text.AppendFormat(" _{0}_", location);
                }
                text.Append("\n");
            }
            foreach (var organization in currentOrganizations)
            {
                if (organization != null &&
                    !organization.Description.Missing())
                {
                    text.AppendFormat("{0}\n", organization.Description);
                }
            }
            if (totalFollowers.HasValue && totalFollowers.Value > 0)
            {
                text.AppendFormat("{0} followers on social media", totalFollowers.Value);
                if (totalFollowers > 1000)
                {
                    text.Append(" (wow!)");
                }
                text.Append("\n");
            }
            if (photo != null)
            {
                text.AppendFormat("<{0}|Profile photo>\n", photo.Url);
            }

            return text.ToString().TrimEnd('\n');
        }
    }
}