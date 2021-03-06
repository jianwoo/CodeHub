﻿using System;
using ReactiveUI;
using Humanizer;

namespace CodeHub.Core.ViewModels.App
{
    public class FeedbackItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Title { get; private set; }

        public string ImageUrl { get; private set; }

        public IReactiveCommand<object> GoToCommand { get; private set; }

        public DateTimeOffset Created { get; private set; }

        public string CreatedString { get; private set; }

        internal FeedbackItemViewModel(Octokit.Issue issue, Action gotoAction)
        {
            Title = issue.Title;
            ImageUrl = issue.User.AvatarUrl;
            Created = issue.CreatedAt;
            CreatedString = Created.Humanize();
            GoToCommand = ReactiveCommand.Create().WithSubscription(_ => gotoAction());
        }
    }
}

