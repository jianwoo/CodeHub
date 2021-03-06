﻿using CodeHub.Core.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using CodeHub.Core.ViewModels;

namespace CodeHub.Core.Services
{
    public interface ISessionService
    {
        GitHubAccount Account { get; }

        GitHubSharp.Client Client { get; }

        Octokit.IGitHubClient GitHubClient { get; }

        BaseViewModel StartupViewModel { get; set; }

        void Track(string eventName, IDictionary<string, string> properties = null);

        Task SetSessionAccount(GitHubAccount account);

        Task RegisterForNotifications();
    }
}