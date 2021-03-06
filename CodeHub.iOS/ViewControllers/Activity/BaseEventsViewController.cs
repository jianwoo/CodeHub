using System;
using CodeHub.Core.ViewModels.Activity;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.Views;
using UIKit;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.iOS.ViewControllers.Activity
{
    public abstract class BaseEventsViewController<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : BaseEventsViewModel
    {
        protected BaseEventsViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.RadioTower.ToEmptyListImage(), "There is no activity."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.WhenAnyValue(x => x.ViewModel.Events)
                .Select(x => new EventTableViewSource(TableView, x))
                .BindTo(TableView, x => x.Source);
        }
    }
}