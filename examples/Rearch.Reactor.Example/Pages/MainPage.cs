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
using Rearch.Reactor.Components;

namespace Rearch.Reactor.Example.Pages;

partial class MainPage : CapsuleConsumer
{
    private async Task<IModelContext> ContextAsyncCapsule(ICapsuleHandle use) =>
        await Task.Run(Services.GetRequiredService<IModelContext>);

    private AsyncValue<IModelContext> ContextWarmUpCapsule(ICapsuleHandle use)
    {
        var task = use.Invoke(ContextAsyncCapsule);
        return use.Task(task);
    }

    private IModelContext ContextCapsule(ICapsuleHandle use) =>
        use.Invoke(ContextWarmUpCapsule).GetData().UnwrapOrElse(
            () => throw new InvalidOperationException(
                "ContextWarmUpCapsule was not warmed up!"));

    private IQuery<Todo> TodoQueryCapsule(ICapsuleHandle use) =>
        use.Invoke(ContextCapsule).Query<Todo>(query => query.OrderBy(_ => _.Task));

    private (
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

    private IQuery<Todo> TodoItemsCapsule(ICapsuleHandle use)
    {
        var query = use.Invoke(TodoQueryCapsule);

        var todos = use.Memo(() => query, [query]);

        return todos;
    }

    public override VisualNode Render(ICapsuleHandle use)
    {
        var todoItems = use.Invoke(TodoItemsCapsule);

        return new List<AsyncValue<IModelContext>>
        {
            use.Invoke(ContextWarmUpCapsule)
        }
        .ToWarmUpComponent(
            child: ContentPage(
                Grid("Auto, *, Auto", "*",
                    new TodoEditor(OnCreatedNewTask),

                    CollectionView()
                        .ItemsSource(todoItems, i => new Item(i, OnItemDoneChanged))
                        .GridRow(1),

                    Button("Clear List")
                        .OnClicked(OnClearList)
                        .GridRow(2)

                )
            ),
            loading: Label("Loading"),
            errorBuilder: errors => VStack(
                children: errors.Select(error => Label(error.Error.ToString())).ToArray()));

        void OnItemDoneChanged(Todo item, bool done)
        {
            var (_, UpdateTodo, _) = use.Invoke(TodoItemsManagerCapsule);

            item.Done = done;

            UpdateTodo(item);
        }

        void OnCreatedNewTask(Todo todo)
        {
            var (AddTodo, _, _) = use.Invoke(TodoItemsManagerCapsule);

            AddTodo(todo);
        }

        void OnClearList()
        {
            var (_, _, DeleteTodos) = use.Invoke(TodoItemsManagerCapsule);

            var todoItems = use.Invoke(TodoItemsCapsule);

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
