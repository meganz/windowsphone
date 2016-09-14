using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp.Services
{
    /// <summary>
    /// Class to do some operations over Uri strings or URLs.
    /// </summary>
    class UriService
    {
        /// <summary>
        /// Converts a "mega://" URL to a "https://mega.nz" URL.
        /// If the source URL type is not "mega://" returns the source URL.
        /// </summary>
        /// <param name="Uri">Source URL.</param>
        /// <returns>Final URL.</returns>
        public static String ReformatUri(String Uri)
        {
            if(!String.IsNullOrWhiteSpace(Uri))
            {
                // Avoid the last "/" character introduced by some browsers
                if (Uri.EndsWith("/"))
                    Uri = Uri.Remove(Uri.Length - 1, 1);

                // Reformat the URL begining            
                if (Uri.Contains("#"))
                {
                    String uriBegin = Uri.Split('#').First();
                    if (!String.IsNullOrWhiteSpace(uriBegin))
                        Uri = Uri.Replace(uriBegin, "https://mega.nz/");
                    else
                        Uri = String.Format("https://mega.nz/" + Uri);
                }
                else
                {
                    // Support for old links
                    if (Uri.StartsWith("mega:///"))
                        Uri = Uri.Replace("mega:///", "https://mega.nz/#");
                    else if (Uri.StartsWith("mega://"))
                        Uri = Uri.Replace("mega://", "https://mega.nz/#");
                }
            }

            return Uri;
        }
    }
}
