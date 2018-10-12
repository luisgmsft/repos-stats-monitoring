using System;

namespace Pnp.VSTSSync
{
    public class WebHookSettings
    {
        public WebHookSettings()
        {
        }

        private static string GetSetting(string settingName)
        {
            if (string.IsNullOrWhiteSpace(settingName))
            {
                throw new ArgumentException($"{nameof(settingName)} cannot be null, empty, or only whitespace");
            }

            var value = Environment.GetEnvironmentVariable(settingName);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Setting '{value}' cannot be null, empty, or only whitespace");
            }

            return value;
        }

        public string GitHubWebHookSecret
        {
            get => GetSetting("GITHUB_WEBHOOK_SECRET");
        }

        public string GitHubWorkItemType
        {
            get => GetSetting("GITHUB_WORK_ITEM_TYPE");
        }

        public string VSTSToken
        {
            get => GetSetting("VSTS_TOKEN");
        }

        public string VSTSBaseUrl
        {
            get => GetSetting("VSTS_BASEURL");
        }

        public string VSTSProjectName
        {
            get => GetSetting("VSTS_PROJECT_NAME");
        }

        public string VSTSRootAreaPath
        {
            get => GetSetting("VSTS_ROOT_AREA_PATH");
        }
    }
}
