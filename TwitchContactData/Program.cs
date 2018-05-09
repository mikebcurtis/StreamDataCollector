using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;

namespace TwitchContactData
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                Console.WriteLine("Please provide a file path.");
                return;
            }

            IEnumerable<TwitchContact> contacts;
            IEnumerable<TwitchContact> updatedContacts;
            Console.WriteLine("Accessing Twitch API to find new contacts.");

            string[] optionalGameIds = null;

            if (args.Length > 1)
            {
                optionalGameIds = new string[args.Length - 1];
                int idx = 0;
                for (int i = 1; i < args.Length; i++)
                {
                    optionalGameIds[idx++] = args[i];
                }
            }

            if (File.Exists(args[0]))
            {
                using (StreamReader stream = new StreamReader(args[0]))
                {
                    CsvReader csv = new CsvReader(stream);
                    contacts = csv.GetRecords<TwitchContact>();
                    updatedContacts = MergeContacts(contacts, TwitchAPIAccessor.GetLiveChannelData(10, optionalGameIds));
                }
            }
            else
            {
                updatedContacts = MergeContacts(new List<TwitchContact>(), TwitchAPIAccessor.GetLiveChannelData(10, optionalGameIds));
            }

            updatedContacts = TwitchAPIAccessor.GetDisplayNames(updatedContacts);

            using (StreamWriter writer = new StreamWriter(args[0], false)) // false indicates overwrite instead of append
            {
                CsvWriter csvWriter = new CsvWriter(writer);
                csvWriter.WriteRecords<TwitchContact>(updatedContacts);
            }

            Console.ReadLine(); // this just stops the debugger in visual studio
        }

        static IEnumerable<TwitchContact> MergeContacts(IEnumerable<TwitchContact> original, IEnumerable<TwitchContact> other)
        {
            List<TwitchContact> updatedList = new List<TwitchContact>(original);
            int numAdded = 0;
            int numUpdated = 0;
            foreach (TwitchContact contact in other)
            {
                int contactIdx = updatedList.FindIndex(contact.MatchesId);
                if (contactIdx < 0)
                {
                    numAdded++;
                    Console.WriteLine("New contact added.");
                    updatedList.Add(contact);
                }
                else
                {
                    numUpdated++;
                    Console.WriteLine("Updated contact.");
                    updatedList.ElementAt<TwitchContact>(contactIdx).Merge(contact);
                }
            }

            Console.WriteLine(string.Format("Summary: New Contacts - {0}, Updated contacts - {1}", numAdded, numUpdated));

            return updatedList;
        }
    }
}
