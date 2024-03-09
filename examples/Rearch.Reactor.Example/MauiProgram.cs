using MauiReactor;
using Microsoft.Extensions.Logging;
using ReactorData.Sqlite;
using System;
using System.IO;
using Rearch.Reactor.Components;
using Rearch.Reactor.Example.Models;
using Rearch.Reactor.Example.Pages;
using Rearch.Reactor.Example.Helpers;

namespace Rearch.Reactor.Example;

public static class MauiProgram
{
    static readonly string _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "todo_app.db");

    public static MauiApp CreateMauiApp()
    {
        //if (File.Exists(_dbPath))
        //{
        //    File.Delete(_dbPath);
        //}

        var builder = MauiApp.CreateBuilder();
        builder
            .UseRearchReactorApp<MainPage>(app =>
            {
                //app.AddResource("Resources/Styles/Colors.xaml");
                //app.AddResource("Resources/Styles/Styles.xaml");
            })
#if DEBUG
            .EnableMauiReactorHotReload()
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemiBold");
            });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
        //ReactorData
        builder.Services.AddReactorDataWithSqlite(
            connectionString: $"Data Source={_dbPath}",
            configure: _ => _.Model<Todo>(),
            modelContextConfigure: options =>
            {
                options.Dispatcher = action =>
                {
                    if (MauiControls.Application.Current?.Dispatcher.IsDispatchRequired == true)
                    {
                        MauiControls.Application.Current?.Dispatcher.Dispatch(action);
                    }
                    else
                    {
                        action();
                    }
                };
                options.ConfigureContext = context => context.Load<Todo>();
            });


        var mauiApp = builder.Build();

        ServiceHelper.Initialize(mauiApp.Services);

        return mauiApp;
    }
}
