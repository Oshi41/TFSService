using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TfsAPI.TFS
{
    public class CapacitySearcher
    {
        private readonly string url;
        private readonly Guid project;
        private readonly Guid teamId;
        private readonly Guid iteration;

        public CapacitySearcher(string connectionUrl,
                                Project project, 
                                TeamFoundationIdentity teamId, 
                                TeamSettingsIteration iteration)
        {
            this.url = connectionUrl;
            this.project = project.Guid;
            this.teamId = teamId.TeamFoundationId;
            this.iteration = iteration.Id;
        }

        public CapacitySearcher(string connectionUrl, Guid project, Guid teamId, Guid iterationID)
        {
            this.url = connectionUrl;
            this.project = project;
            this.teamId = teamId;
            this.iteration = iterationID;
        }

        public List<TeamMemberCapacity> QuerryCapacity(ICredentials creds)
        {
            var request = $"{url}/{project}/{teamId}/_apis/work/teamsettings/iterations/{iteration}/capacities";
            
            var webReq = WebRequest.CreateHttp(request) as HttpWebRequest;

            webReq.Method = "GET";
            webReq.Credentials = creds;
            webReq.ContentType = "text/html";

            // FederatedCookieHelper.EnsureFederatedIdentityCookies(teamProjectCollection, httpWebRequest);

            try
            {
                var resp = webReq.GetResponse() as HttpWebResponse;
                using (var reader = new StreamReader(resp.GetResponseStream()))
                {
                    var result =  reader.ReadToEnd();

                    return Parse(result);
                }
            }
            catch (Exception e)
            {
                // throw;
                return new List<TeamMemberCapacity>();
            }
        }

        public static List<TeamMemberCapacity> Parse(string jsonRaw)
        {
            var json = JObject.Parse(jsonRaw);

            var members = json["value"];

            var result = JsonConvert.DeserializeObject<List<TeamMemberCapacity>>(members.ToString());
            return result;
        }
    }
}
