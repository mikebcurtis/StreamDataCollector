using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;

namespace TwitchContactData
{
    class TwitchAPIAccessor
    {
        private static string ClientIdHeader = "Client-ID";
        private static string ClientIdValue = "pxa5la9qqsrqerq15fre01o89fmff0";
        private static string StreamsEndpoint = "https://api.twitch.tv/helix/streams";
        private static string UserEndpoint = "https://api.twitch.tv/helix/users";

        public static IEnumerable<TwitchContact> GetLiveChannelData(int viewerThreshold = 10, string[] gameIds = null)
        {
            List<TwitchContact> resultSet = new List<TwitchContact>();
            bool stopRequests = false;
            string paginationCursor = "";

            while (stopRequests == false)
            {
                string url = StreamsEndpoint + "?";
                if (string.IsNullOrEmpty(paginationCursor) == false)
                {
                    url += "after=" + paginationCursor;
                }
                if (gameIds != null)
                {
                    foreach(string gameId in gameIds)
                    {
                        url += "game_id=" + gameId + "&";
                    }
                }
                Console.WriteLine(string.Format("Making request to {0} for live channel information.", url));
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Headers.Add(ClientIdHeader, ClientIdValue);
                WebResponse response = webRequest.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream);
                string rawResponse = streamReader.ReadToEnd();

                Console.WriteLine("Got response, parsing JSON.");
                JObject json = JObject.Parse(rawResponse);
                IList<JToken> jsonContacts = json["data"].Children().ToList();

                if (jsonContacts.Count <= 0)
                {
                    Console.WriteLine("No results, assuming this means we've gotten all results.");
                    stopRequests = true;
                }

                foreach (JToken token in jsonContacts)
                {
                    if (token["viewer_count"].ToObject<int>() >= viewerThreshold)
                    {
                        TwitchContact contact = new TwitchContact
                        {
                            Id = token["user_id"].ToString(),
                            LastUpdated = DateTime.Now
                        };
                        resultSet.Add(contact);
                    }
                    else
                    {
                        stopRequests = true;
                        break;
                    }
                }

                string rateLimitValueString = response.Headers["RateLimit-Remaining"];
                string rateLimitRefresh = response.Headers["RateLimit-Reset"];
                paginationCursor = json["pagination"]["cursor"].ToString();
                streamReader.Dispose();
                responseStream.Dispose();
                response.Dispose();

                if (stopRequests)
                {
                    break;
                }

                int rateLimitValue = -1;
                if (int.TryParse(rateLimitValueString, out rateLimitValue) == false)
                {
                    Console.WriteLine("Unable to parse the RateLimit-Remaining attribute, quitting.");
                    break;
                }

                if (rateLimitValue <= 1)
                {
                    int sleepSeconds = 70;
                    Console.WriteLine("Rate limit reached, waiting for refresh.");
                    Console.WriteLine(string.Format("Sleeping for {0} seconds.", sleepSeconds));
                    System.Threading.Thread.Sleep(sleepSeconds * 1000);
                }
            }

            return resultSet;
        }

        public static IEnumerable<TwitchContact> GetDisplayNames(IEnumerable<TwitchContact> contacts)
        {
            Console.WriteLine("Attempting to retrieve display names for Twitch Contacts...");
            foreach(TwitchContact contact in contacts)
            {
                if (string.IsNullOrEmpty(contact.Name) == false || string.IsNullOrEmpty(contact.Id))
                {
                    continue;
                }

                string url = UserEndpoint + "?id=" + contact.Id;

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Headers.Add(ClientIdHeader, ClientIdValue);
                WebResponse response = webRequest.GetResponse();
                string rateLimitValueString = response.Headers["RateLimit-Remaining"];
                string rateLimitRefresh = response.Headers["RateLimit-Reset"];
                Stream responseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream);
                string rawResponse = streamReader.ReadToEnd();

                Console.WriteLine("Got response, parsing JSON.");
                JObject json = JObject.Parse(rawResponse);
                JToken token = json["data"].Children().ToList()[0]; // returns a list, but there should only be one item in the list

                if (token["display_name"] != null)
                {
                    contact.Name = token["display_name"].ToObject<string>();
                    Console.WriteLine("Got display name.");
                }

                // cleanup
                streamReader.Dispose();
                responseStream.Dispose();
                response.Dispose();

                // check rate limit
                int rateLimitValue = -1;
                if (int.TryParse(rateLimitValueString, out rateLimitValue) == false)
                {
                    Console.WriteLine("Unable to parse the RateLimit-Remaining attribute, quitting.");
                    break;
                }

                if (rateLimitValue <= 1)
                {
                    int sleepSeconds = 70;
                    Console.WriteLine("Rate limit reached, waiting for refresh.");
                    Console.WriteLine(string.Format("Sleeping for {0} seconds.", sleepSeconds));
                    System.Threading.Thread.Sleep(sleepSeconds * 1000);
                }
            }

            return contacts;
        }
    }
}
