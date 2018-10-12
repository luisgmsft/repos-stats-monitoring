using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Pnp.VSTSSync
{
    public static class SEWebHook
    {
        private static TraceWriter _log;
        private static IDictionary<string, string> users = new Dictionary<string, string>()
                        {
                            {
                                "atoakley", "anoakley@microsoft.com"
                            }
                        };

        private static WebHookSettings Settings = new WebHookSettings();

        private static WorkItemTrackingHttpClient CreateClient()
        {
            VssCredentials creds = new VssCredentials(new VssBasicCredential(string.Empty, Settings.VSTSToken));
            var connection = new VssConnection(new Uri(Settings.VSTSBaseUrl), creds);
            var workitemClient = connection.GetClient<WorkItemTrackingHttpClient>();
            return workitemClient;
        }
        private static bool IsValidGithubMessage(string requestBody, string githubSignature, dynamic data)
        {
            if (requestBody == null)
            {
                throw new ArgumentNullException(nameof(requestBody));
            }

            var bodyBytes = System.Text.Encoding.ASCII.GetBytes(requestBody);
                // string requestBody = System.Text.Encoding.UTF8.GetString(bodyBytes);
            string signature;
            
            var repoSecret = Environment.GetEnvironmentVariable(("SECRET_" + data.repository.name).ToUpper());
            if (repoSecret == null)
            {
                _log.Info("Secret not found for repo " + data.repository.name);
                return false;
            }

            byte[] keyParts = System.Text.Encoding.ASCII.GetBytes(repoSecret);

            using (var hmac = new System.Security.Cryptography.HMACSHA1(keyParts))
            {
                // We'll be a little lazy and take the performance hit of BitConverter
                signature = "sha1=" + BitConverter.ToString(
                    hmac.ComputeHash(bodyBytes)).Replace("-", "");
            }

            return string.Compare(githubSignature, signature, true) == 0;
        }

        private static async Task HandleAssigned(dynamic data)
        {
            string gitHubItemType = null;
            dynamic node = null;
            _log.Info("Handle Assigned");
            if (data.issue != null)
            {
                // We are an issue
                gitHubItemType = "Issue";
                node = data.issue;
            }
            else if (data.pull_request != null)
            {
                // We are a pull request
                gitHubItemType = "Pull Request";
                node = data.pull_request;
            }

            using (var workItemClient = CreateClient())
            {
                var (found, workItemId) = await FindWorkItemByGitHubURI((string)node.html_url, workItemClient);

                if(found)
                    _log.Info("Work Item already created - id: " + workItemId);

                if(found || node.assignees == null)
                    return;

                foreach(var assignee in node.assignees)
                {
                    if(assignee.login == users.First().Key)
                    {
                        await CreateVSTSWorkItem(gitHubItemType, node);
                        break;
                    }
                }
            }
        }

        private static async Task<WorkItem> CreateVSTSWorkItem(string gitHubItemType, dynamic node)
        {
            // Common fields
            var fields = new Dictionary<string, object>()
            {
                {
                    "/fields/ContentDevelopment.GitHubItemType", gitHubItemType
                },
                {
                    "/fields/ContentDevelopment.GitHubURI", (string)node.html_url
                },
                {
                    "/fields/System.AreaPath", $"{Settings.VSTSProjectName}\\{Settings.VSTSRootAreaPath}"
                },
                {
                    "/fields/System.Description", (string)node.body
                },
                {
                    "/fields/System.Tags", "from-github"
                },
                {
                    "/fields/System.Title", (string)node.title
                },
                {
                    "/relations/-", new Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItemRelation()
                    {
                        Rel = "Hyperlink",
                        Url = (string)node.html_url
                    }
                }
            };

            using (var workItemClient = CreateClient())
            {
                var document = WorkItems.BuildCreateDocument(fields);
                var workitem = await workItemClient.CreateWorkItemAsync(
                    document, Settings.VSTSProjectName, Settings.GitHubWorkItemType)
                    .ConfigureAwait(false);
                _log.Info("VSTS workitem created with Id: " + workitem.Id);
                return workitem;
            }
        }

        private static async Task HandleClose(dynamic data)
        {
            _log.Info("Handle Close");
            using (var workItemClient = CreateClient())
            {
                dynamic node = null;
                if (data.issue != null)
                {
                    // We are an issue
                    node = data.issue;
                }
                else if (data.pull_request != null)
                {
                    // We are a pull request
                    node = data.pull_request;
                }

                var (found, workItemId) = await FindWorkItemByGitHubURI((string)node.html_url, workItemClient);

                if (found)
                {
                    var fields = new Dictionary<string, object>()
                    {
                        {
                            "/fields/System.State", "Done"
                        }
                    };

                    var sender = (string)data?.sender?.login;
                    if (sender != null)
                    {
                        if (users.TryGetValue(sender, out string upn))
                        {
                            fields.Add("/fields/System.AssignedTo", upn);
                        }
                    }

                    var document = WorkItems.BuildUpdateDocument(fields);
                    var workItem = await workItemClient.UpdateWorkItemAsync(document, workItemId)
                        .ConfigureAwait(false);
                    _log.Info("work item closed - id: " + workItem.Id);
                }
                else
                {
                    _log.Info("VSTS work item not found: " + workItemId);
                }
            }
        }

        private static async Task HandleGitHubEvent(dynamic data)
        {
            var action = (string)data.action;

            if ((data.issue == null) && (data.pull_request == null))
            {
                // Nothing to do
                return;
            }

            switch (action)
            {
                // case "opened":
                //     await HandleOpen(data);
                //     break;
                case "assigned":                
                    await HandleAssigned(data);
                    break;
                case "closed":
                    await HandleClose(data);
                    break;
                default:
                    _log.Info("Action not handled: " + action);
                    return;
            }
        }

        private static async Task<(bool found, int workItemId)> FindWorkItemByGitHubURI(
            string uri,
            WorkItemTrackingHttpClient workItemClient)
        {
            var results = await workItemClient.QueryByWiqlAsync(new Wiql()
            {
                Query = $@"SELECT [System.ID]
                            FROM WorkItems
                            WHERE [System.TeamProject] = '{Settings.VSTSProjectName}'
                            AND [System.WorkItemType] = '{Settings.GitHubWorkItemType}'
                            AND [ContentDevelopment.GitHubURI] = '{uri}'"
            });

            if (results.WorkItems.Count() == 1)
            {
                var workItemRef = results.WorkItems.Single();
                return (true, workItemRef.Id);
            }
            else
            {
                return (false, 0);
            }
        }

        [FunctionName("SyncTasks")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req,
            TraceWriter log)
        {
            _log = log;
            log.Info("C# HTTP trigger function processed a request.");
            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                string githubSignature = req.Headers["X-Hub-Signature"].FirstOrDefault() ?? string.Empty;
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                if (!IsValidGithubMessage(requestBody, githubSignature, data))
                {
                    log.Info("Invalid GitHub Signature: " + githubSignature);
                    return new UnauthorizedResult();
                }

                log.Info("Handle GitHub Event Start");
                await HandleGitHubEvent(data);
                //return new OkObjectResult(JsonConvert.SerializeObject(data));
                log.Info("Handle GitHub Event Finish");
                return new OkResult();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }

            return new BadRequestResult();
        }
    }
}
