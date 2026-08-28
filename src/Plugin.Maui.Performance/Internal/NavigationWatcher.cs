#if ANDROID || IOS
using Microsoft.Maui.Controls;
#endif

namespace Plugin.Maui.Performance;

sealed class NavigationWatcher : IDisposable
{
    readonly IMauiPerformanceListener _listener;
    bool _started;

    public NavigationWatcher(IMauiPerformanceListener listener) => _listener = listener;

    public void Start()
    {
        if (_started)
        {
            return;
        }

        _started = true;
#if ANDROID || IOS
        try
        {
            if (Application.Current is { } app)
            {
                app.PageAppearing += OnPageAppearing;
                app.PageDisappearing += OnPageDisappearing;
            }
        }
        catch
        {
            _started = false;
        }
#endif
    }

    public void Dispose()
    {
        if (!_started)
        {
            return;
        }

#if ANDROID || IOS
        try
        {
            if (Application.Current is { } app)
            {
                app.PageAppearing -= OnPageAppearing;
                app.PageDisappearing -= OnPageDisappearing;
            }
        }
        catch
        {
            // Host may already be tearing down.
        }
#endif
        _started = false;
    }

#if ANDROID || IOS
    void OnPageAppearing(object? sender, Page page)
    {
        if (IsContainer(page))
        {
            return;
        }

        var name = NameOf(page);
        _listener.OnPageAppearing(name, page);

        void OnLoaded(object? _, EventArgs __)
        {
            page.Loaded -= OnLoaded;
            _listener.OnPageLoaded(name, page);
        }

        if (page.IsLoaded)
        {
            _listener.OnPageLoaded(name, page);
        }
        else
        {
            page.Loaded += OnLoaded;
        }
    }

    void OnPageDisappearing(object? sender, Page page)
    {
        if (IsContainer(page))
        {
            return;
        }

        _listener.OnPageDisappearing(NameOf(page), page);
    }

    static bool IsContainer(Page page) =>
        page is NavigationPage or FlyoutPage or TabbedPage or Shell;

    static string NameOf(Page page)
    {
        if (!string.IsNullOrWhiteSpace(page.Title))
        {
            return page.Title.EndsWith(" Page", StringComparison.OrdinalIgnoreCase)
                ? page.Title
                : page.Title + " Page";
        }

        var typeName = page.GetType().Name;
        if (typeName.EndsWith("Page", StringComparison.Ordinal) && typeName.Length > 4)
        {
            return typeName[..^4] + " Page";
        }

        return typeName;
    }
#endif
}
