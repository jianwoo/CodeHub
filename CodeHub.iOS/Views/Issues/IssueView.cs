using System;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Issues;
using GitHubSharp.Models;
using UIKit;
using CodeFramework.iOS.ViewControllers;
using MonoTouch.Dialog;
using CodeFramework.iOS.Utils;
using CodeFramework.iOS.Elements;
using System.Linq;

namespace CodeHub.iOS.Views.Issues
{
	public class IssueView : ViewModelDrivenDialogViewController
    {
		private readonly HeaderView _header;
		private WebElement _descriptionElement;
		private WebElement2 _commentsElement;


        public new IssueViewModel ViewModel
        {
            get { return (IssueViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public IssueView()
        {
			Root.UnevenRows = true;
			_header = new HeaderView() { ShadowImage = false };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			var content = System.IO.File.ReadAllText("WebCell/body.html", System.Text.Encoding.UTF8);
			_descriptionElement = new WebElement(content);
			_descriptionElement.UrlRequested = ViewModel.GoToUrlCommand.Execute;

			var content2 = System.IO.File.ReadAllText("WebCell/comments.html", System.Text.Encoding.UTF8);
			_commentsElement = new WebElement2(content2);
			_commentsElement.UrlRequested = ViewModel.GoToUrlCommand.Execute;

			NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Compose, (s, e) => ViewModel.GoToEditCommand.Execute(null));
            NavigationItem.RightBarButtonItem.Enabled = false;
            ViewModel.Bind(x => x.Issue, RenderIssue);
            ViewModel.BindCollection(x => x.Comments, (e) => RenderComments());
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            Title = "Issue #" + ViewModel.Id;
        }

        public void RenderComments()
        {
            var comments = ViewModel.Comments.Select(x => new { 
                avatarUrl = x.User.AvatarUrl, 
                login = x.User.Login, 
                updated_at = x.CreatedAt.ToDaysAgo(), 
                body = ViewModel.ConvertToMarkdown(x.Body)
            });

			var s = Cirrious.CrossCore.Mvx.Resolve<CodeFramework.Core.Services.IJsonSerializationService>();
			var data = s.Serialize(comments);
			InvokeOnMainThread(() => {
				_commentsElement.Value = data;
				if (_commentsElement.GetImmediateRootElement() == null)
					RenderIssue();
			});
        }

        public void RenderIssue()
        {
			if (ViewModel.Issue == null)
				return;

            NavigationItem.RightBarButtonItem.Enabled = true;

			var root = new RootElement(Title);
			_header.Title = ViewModel.Issue.Title;
			_header.Subtitle = "Updated " + ViewModel.Issue.UpdatedAt.ToDaysAgo();
			root.Add(new Section(_header));


			var secDetails = new Section();

			if (!string.IsNullOrEmpty(ViewModel.Issue.Body))
			{
				_descriptionElement.Value = ViewModel.MarkdownDescription;
				secDetails.Add(_descriptionElement);
			}

			var milestone = ViewModel.Issue.Milestone;
			var milestoneStr = milestone != null ? milestone.Title : "No Milestone";
			var milestoneElement = new StyledStringElement("Milestone", milestoneStr, UITableViewCellStyle.Value1) {Image = Images.Milestone, Accessory = UITableViewCellAccessory.DisclosureIndicator};
			milestoneElement.Tapped += () => ViewModel.GoToMilestoneCommand.Execute(null);

			var assigneeElement = new StyledStringElement("Assigned", ViewModel.Issue.Assignee != null ? ViewModel.Issue.Assignee.Login : "Unassigned".t(), UITableViewCellStyle.Value1) {
				Image = Images.Person,
				Accessory = UITableViewCellAccessory.DisclosureIndicator
			};
			assigneeElement.Tapped += () => ViewModel.GoToAssigneeCommand.Execute(null);


			var labels = ViewModel.Issue.Labels.Count == 0 ? "None" : string.Join(", ", ViewModel.Issue.Labels.Select(i => i.Name));
			var labelsElement = new StyledStringElement("Lables", labels, UITableViewCellStyle.Value1) {
				Image = Images.Tag,
				Accessory = UITableViewCellAccessory.DisclosureIndicator
			};
			labelsElement.Tapped += () => ViewModel.GoToLabelsCommand.Execute(null);

			secDetails.Add(assigneeElement);
			secDetails.Add(milestoneElement);
			secDetails.Add(labelsElement);
			root.Add(secDetails);

			if (ViewModel.Comments.Any())
			{
				root.Add(new Section { _commentsElement });
			}

			var addComment = new StyledStringElement("Add Comment") { Image = Images.Pencil };
			addComment.Tapped += AddCommentTapped;
			root.Add(new Section { addComment });
			Root = root;
        }

        void AddCommentTapped()
        {
			var composer = new Composer();
			composer.NewComment(this, async (text) => {
				try
				{
					await composer.DoWorkAsync("Commenting...".t(), () => ViewModel.AddComment(text));
					composer.CloseComposer();
				}
				catch (Exception e)
				{
					MonoTouch.Utilities.ShowAlert("Unable to post comment!", e.Message);
				}
				finally
				{
					composer.EnableSendButton = true;
				}
			});
        }

        public override UIView InputAccessoryView
        {
            get
            {
                var u = new UIView(new CoreGraphics.CGRect(0, 0, 320f, 27)) { BackgroundColor = UIColor.White };
                return u;
            }
        }
    }
}

