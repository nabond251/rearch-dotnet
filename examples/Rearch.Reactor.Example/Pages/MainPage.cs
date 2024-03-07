using MauiReactor;
using ReactorData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rearch.Reactor.Example.Models;
using Rearch.Reactor.Components;
using Microsoft.Extensions.DependencyInjection;
using Rearch.Types;

namespace Rearch.Reactor.Example.Pages;

public static class ServiceHelper
{
    private static readonly TaskCompletionSource<IServiceProvider> serviceProviderTcs = new();

    public static Task<IServiceProvider> ServicesAsync => serviceProviderTcs.Task;

    public static void Initialize(IServiceProvider serviceProvider) =>
        serviceProviderTcs.TrySetResult(serviceProvider);
}

public static class DataAccess
{
    internal static async Task<IModelContext> ContextAsyncCapsule(ICapsuleHandle use)
    {
        await Task.Delay(1000);
        return (await ServiceHelper.ServicesAsync).GetRequiredService<IModelContext>();
    }

    internal static AsyncValue<IModelContext> ContextWarmUpCapsule(ICapsuleHandle use)
    {
        var task = use.Invoke(ContextAsyncCapsule);
        return use.Task(task);
    }

    internal static IModelContext ContextCapsule(ICapsuleHandle use) =>
        use.Invoke(ContextWarmUpCapsule).GetData().UnwrapOrElse(
            () => throw new InvalidOperationException(
                "ContextWarmUpCapsule was not warmed up!"));

    internal static IQuery<Todo> TodoQueryCapsule(ICapsuleHandle use) =>
        use.Invoke(ContextCapsule).Query<Todo>(query => query.OrderBy(_ => _.Task));

    internal static (
        Action<Todo> AddTodo,
        Action<Todo> UpdateTodo,
        Action<IEnumerable<Todo>> DeleteTodos)
        TodoItemsManagerCapsule(ICapsuleHandle use)
    {
        var context = use.Invoke(ContextCapsule);

        return (
            AddTodo: todo =>
            {
                context.Add(todo);
                context.Save();
            },
            UpdateTodo: todo =>
            {
                context.Replace(todo, new Todo { Id = todo.Id, Task = todo.Task, Done = todo.Done });
                context.Save();
            },
            DeleteTodos: todos =>
            {
                context.Delete([.. todos]);
                context.Save();
            }
        );
    }

    internal static IQuery<Todo> TodoItemsCapsule(ICapsuleHandle use)
    {
        var query = use.Invoke(TodoQueryCapsule);

        var todos = use.Memo(() => query, [query]);

        return todos;
    }
}

partial class MainPage : CapsuleConsumer
{
    public override VisualNode Render(ICapsuleHandle use)
    {
        return NavigationPage(
            ContentPage(
                new GlobalWarmUps(new Body())
            )
            .Title("Rearch Todos"));
    }
}

partial class GlobalWarmUps(VisualNode child) : CapsuleConsumer
{
    public override VisualNode Render(ICapsuleHandle use)
    {
        return new List<AsyncValue<IModelContext>>
        {
            use.Invoke(DataAccess.ContextWarmUpCapsule)
        }
        .ToWarmUpComponent(
            child: child,
            loading: Label("Loading").Center(),
            errorBuilder: errors =>
            VStack(
                children: errors
                .Select(error => Label(error.Error.ToString()))
                .ToArray())
            .Center());
    }
}

partial class Body : CapsuleConsumer
{
    public override VisualNode Render(ICapsuleHandle use)
    {
        var todoItems = use.Invoke(DataAccess.TodoItemsCapsule);

        return Grid("Auto, *, Auto", "*",
            new TodoEditor(OnCreatedNewTask),

            CollectionView()
                .ItemsSource(todoItems, i => new Item(i, OnItemDoneChanged))
                .GridRow(1),

            Button("Clear List")
                .OnClicked(OnClearList)
                .GridRow(2)
        );

        void OnItemDoneChanged(Todo item, bool done)
        {
            var (_, UpdateTodo, _) = use.Invoke(DataAccess.TodoItemsManagerCapsule);

            item.Done = done;

            UpdateTodo(item);
        }

        void OnCreatedNewTask(Todo todo)
        {
            var (AddTodo, _, _) = use.Invoke(DataAccess.TodoItemsManagerCapsule);

            AddTodo(todo);
        }

        void OnClearList()
        {
            var (_, _, DeleteTodos) = use.Invoke(DataAccess.TodoItemsManagerCapsule);

            var todoItems = use.Invoke(DataAccess.TodoItemsCapsule);

            DeleteTodos(todoItems);
        }
    }
}

internal class Item(Todo item, Action<Todo, bool> onItemDoneChanged) : CapsuleConsumer
{
    public override Grid Render(ICapsuleHandle use)
        => Grid("54", "Auto, *",
            CheckBox()
                .IsChecked(item.Done)
                .OnCheckedChanged((s, args) => onItemDoneChanged(item, args.Value)),
            Label(item.Task)
                .TextDecorations(item.Done ? TextDecorations.Strikethrough : TextDecorations.None)
                .VCenter()
                .GridColumn(1));
}

internal class TodoEditor(Action<Todo> created) : CapsuleConsumer
{
    public override VisualNode Render(ICapsuleHandle use)
        => Render<string>(state =>
            Grid("*", "*,Auto",
                Entry()
                    .Text(state.Value ?? string.Empty)
                    .OnTextChanged(text => state.Set(s => text, false)),
                Button("Create")
                    .GridColumn(1)
                    .OnClicked(() =>
                    {
                        created(new Todo
                        {
                            Task = string.IsNullOrEmpty(state.Value) ?
                                "New Task" :
                                state.Value
                        });
                        state.Set(s => string.Empty);
                    })
                )
            );
}
