using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls.Expressions;
using Nancy;

namespace SlackCommander.Web.FullContact
{
    public class FullContactPersonResult
    {
        public string WebhookId { get; set; }
        public ResultSegment Result { get; set; }

        public FullContactPersonResult()
        {
            Result = new ResultSegment();
        }
    }

    public class ResultSegment
    {
        public int Status { get; set; }
        public double Likelihood { get; set; }
        public ContactInfoSegment ContactInfo { get; set; }
        public DemographicsSegment Demographics { get; set; }
        public SocialProfile[] SocialProfiles { get; set; }
        public Organization[] Organizations { get; set; }
        public Photo[] Photos { get; set; }

        public ResultSegment()
        {
            ContactInfo = new ContactInfoSegment();
            Demographics = new DemographicsSegment();
            SocialProfiles = new SocialProfile[0];
            Organizations = new Organization[0];
            Photos = new Photo[0];
        }

        public class ContactInfoSegment
        {
            public string FamilyName { get; set; }
            public string GivenName { get; set; }
            public string FullName { get; set; }
            public Website[] Websites { get; set; }

            public ContactInfoSegment()
            {
                Websites = new Website[0];
            }

            public class Website
            {
                public string Url { get; set; }
            }
        }

        public class DemographicsSegment
        {
            public string LocationGeneral { get; set; }
            public LocationDeducedSegment LocationDeduced { get; set; }
            public string Age { get; set; }
            public string Gender { get; set; }
            public string AgeRange { get; set; }

            public DemographicsSegment()
            {
                LocationDeduced = new LocationDeducedSegment();
            }

            public class LocationDeducedSegment
            {
                public string NormalizedLocation { get; set; }
                public string DeducedLocation { get; set; }
                public double Likelihood { get; set; }
                // ...
            }
        }

        public class SocialProfile
        {
            public string TypeId { get; set; }
            public string TypeName { get; set; }
            public string Url { get; set; }
            public string Username { get; set; }
            public string Bio { get; set; }
            public int? Followers { get; set; }
            // ...
        }

        public class Organization
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public string StartDate { get; set; } // YYYY-MM
            public string EndDate { get; set; }   // YYYY-MM
            public bool? IsPrimary { get; set; }
            public bool? Current { get; set; }

            public string Description
            {
                get
                {
                    if (!Title.Missing() && !Name.Missing())
                    {
                        return string.Format("{0} at {1}", Title, Name);
                    }
                    if (!Title.Missing())
                    {
                        return Title;
                    }
                    if (!Name.Missing())
                    {
                        return Name;
                    }
                    return string.Empty;
                }
            }
        }

        public class Photo
        {
            public string Url { get; set; }
            public bool? IsPrimary { get; set; }
            // ...
        }
    }
}