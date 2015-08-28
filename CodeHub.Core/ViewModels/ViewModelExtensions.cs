using System;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Services;
using GitHubSharp;
using System.Collections.Generic;
using CodeFramework.Core.Services;
using System.ComponentModel;
using System.Collections.Specialized;
using Foundation;

public static class ViewModelExtensions
{
    private static readonly NSObject _obj = new NSObject();

    public static Task RequestModel<TRequest>(this MvxViewModel viewModel, GitHubRequest<TRequest> request, bool forceDataRefresh, Action<GitHubResponse<TRequest>> update) where TRequest : new()
    {
        if (forceDataRefresh)
        {
            request.CheckIfModified = false;
            request.RequestFromCache = false;
        }

        var application = Mvx.Resolve<IApplicationService>();

		return Task.Run(async () =>
        {
			var result = await application.Client.ExecuteAsync(request).ConfigureAwait(false);
            _obj.InvokeOnMainThread(() => update(result));

            if (result.WasCached)
            {
                request.RequestFromCache = false;

				Task.Run(async () =>
                {
                    try
                    {
						var r = await application.Client.ExecuteAsync(request).ConfigureAwait(false);
                        _obj.InvokeOnMainThread(() => update(r));
                    }
					catch (NotModifiedException)
					{
						System.Diagnostics.Debug.WriteLine("Not modified: " + request.Url);
					}
                }).FireAndForget();
            }
        });
	}

    public static void CreateMore<T>(this MvxViewModel viewModel, GitHubResponse<T> response, 
									 Action<Action> assignMore, Action<T> newDataAction) where T : new()
    {
        if (response.More == null)
        {
            assignMore(null);
            return;
        }

		Action task = () =>
        {
            response.More.UseCache = false;
			var moreResponse = Mvx.Resolve<IApplicationService>().Client.ExecuteAsync(response.More).Result;
            viewModel.CreateMore(moreResponse, assignMore, newDataAction);
            newDataAction(moreResponse.Data);
        };

        assignMore(task);
    }

    public static Task SimpleCollectionLoad<T>(this CollectionViewModel<T> viewModel, GitHubRequest<List<T>> request, bool forceDataRefresh) where T : new()
    {
        return viewModel.RequestModel(request, forceDataRefresh, response =>
        {
            viewModel.CreateMore(response, m => viewModel.MoreItems = m, viewModel.Items.AddRange);
            viewModel.Items.Reset(response.Data);
        });
    }

    public static void Bind<T, TR>(this T viewModel, System.Linq.Expressions.Expression<Func<T, TR>> outExpr, Action b, bool activateNow = false) where T : INotifyPropertyChanged
    {
        var expr = (System.Linq.Expressions.MemberExpression) outExpr.Body;
        var prop = (System.Reflection.PropertyInfo) expr.Member;
        var name = prop.Name;
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName.Equals(name))
            {
                try
                {
                    b();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        };

        if (activateNow)
        {
            try
            {
                b();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    public static void Bind<T, TR>(this T viewModel, System.Linq.Expressions.Expression<Func<T, TR>> outExpr, Action<TR> b, bool activateNow = false) where T : INotifyPropertyChanged
    {
        var expr = (System.Linq.Expressions.MemberExpression) outExpr.Body;
        var prop = (System.Reflection.PropertyInfo) expr.Member;
        var name = prop.Name;
        var comp = outExpr.Compile();
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName.Equals(name))
            {
                try
                {
                    b(comp(viewModel));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        };

        if (activateNow)
        {
            try
            {
                b(comp(viewModel));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    public static void BindCollection<T>(this T viewModel, System.Linq.Expressions.Expression<Func<T, INotifyCollectionChanged>> outExpr, Action<NotifyCollectionChangedEventArgs> b, bool activateNow = false) where T : INotifyPropertyChanged
    {
        var exp = outExpr.Compile();
        var m = exp(viewModel);
        m.CollectionChanged += (sender, e) =>
        {
            try
            {
                b(e);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        };

        if (activateNow)
        {
            try
            {
                b(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    public static IApplicationService GetApplication(this BaseViewModel vm)
    {
        return Mvx.Resolve<IApplicationService>();
    }
}

