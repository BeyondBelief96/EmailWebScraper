using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;

namespace EmailWebScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            const string baseQueryString = "https://www.google.com/search?q=";
            const string gmailEmailString = "@gmail.com";
            List<string> queryWords = new List<string>();
            List<string> urls = new List<string>();

            using (StreamReader reader = new StreamReader(@"C:\Users\brand\Desktop\words.txt"))
            {
                for(int i=0; i < 100000; i++)
                {
                    var word = reader.ReadLine();
                    queryWords.Add(word);
                }
            }

            foreach(var word in queryWords)
            {
                urls.Add(baseQueryString + word + gmailEmailString);
            }

            foreach (var url in urls)
            {
                Timer timer = new Timer();
                List<string> emails = GetEmails(url);

                using (StreamWriter file = new StreamWriter(@"C:\Users\brand\Desktop\emails.txt", true))
                {
                    foreach(string entry in emails)
                    {
                        Console.WriteLine(entry);
                        file.WriteLine(entry);
                    }
                }
            }
        }
        
        private static List<string> GetEmails(string url)
        {
            List<string> emails = new List<string>();
            WebClient client = new WebClient();
            byte[] data = client.DownloadData(url);
            string download = Encoding.ASCII.GetString(data);

            download = download.Replace("<em>", "");
            download = download.Replace("</em>", "");
            download = download.Replace("<b>", "");
            download = download.Replace("</b>", "");

            MatchCollection collection = default(MatchCollection);
            const string MatchEmailPattern =
           @"(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
           + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
             + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
           + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})";
            collection = Regex.Matches(download, MatchEmailPattern);

            var distinct = collection.OfType<Match>().Select(m => m.Value).Distinct();

            foreach(string x in distinct)
            {
                emails.Add(x.ToString());
            }

            return emails;
        }


        static string[] GetWords(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"\b[\w']*\b");

            var words = from m in matches.Cast<Match>()
                        where !string.IsNullOrEmpty(m.Value)
                        select TrimSuffix(m.Value);

            return words.ToArray();
        }

        static string TrimSuffix(string word)
        {
            int apostropheLocation = word.IndexOf('\'');
            if (apostropheLocation != -1)
            {
                word = word.Substring(0, apostropheLocation);
            }

            return word;
        }
    }
}
