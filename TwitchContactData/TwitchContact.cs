using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchContactData
{
    class TwitchContact
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // affliliate, partner, or none
        public string PrimaryEmail { get; set; }
        public string OtherEmails { get; set; }
        public string TwitterHandle { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime LastContactedDate { get; set; }
        public bool DoNotContact { get; set; }
        public string Notes { get; set; }

        public TwitchContact Merge(TwitchContact other)
        {
            if (string.IsNullOrEmpty(other.Id) == false)
            {
                Id = other.Id;
            }

            if (string.IsNullOrEmpty(other.Name) == false)
            {
                Name = other.Name;
            }

            if (string.IsNullOrEmpty(other.Type) == false)
            {
                Type = other.Type;
            }

            if (string.IsNullOrEmpty(other.PrimaryEmail) == false)
            {
                PrimaryEmail = other.PrimaryEmail;
            }

            if (string.IsNullOrEmpty(other.OtherEmails) == false)
            {
                OtherEmails = other.OtherEmails;
            }

            if (string.IsNullOrEmpty(other.TwitterHandle) == false)
            {
                TwitterHandle = other.TwitterHandle;
            }

            if (LastContactedDate < other.LastContactedDate)
            {
                LastContactedDate = other.LastContactedDate;
            }

            DoNotContact |= other.DoNotContact; // keep this sticky. If either has a true value, keep it true.

            if (string.IsNullOrEmpty(other.Notes) == false)
            {
                Notes = other.Notes;
            }

            LastUpdated = DateTime.Now;

            return this;
        }

        public bool MatchesId(object obj)
        {
            TwitchContact other = obj as TwitchContact;
            if (other == null)
            {
                return false;
            }

            return other.Id == Id;
        }

        public override bool Equals(object obj)
        {
            TwitchContact other = obj as TwitchContact;
            if (other == null)
            {
                return false;
            }

            return other.Id == Id &&
                   other.Name == Name &&
                   other.Type == Type &&
                   other.PrimaryEmail == PrimaryEmail &&
                   other.OtherEmails == OtherEmails &&
                   other.TwitterHandle == TwitterHandle &&
                   other.LastContactedDate == LastContactedDate &&
                   other.LastUpdated == LastUpdated &&
                   other.DoNotContact == DoNotContact &&
                   other.Notes == Notes;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
