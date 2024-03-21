using MauiReactor;
using Rearch.Reactor.Components;
using Rearch.Reactor.Example.Components;
using Rearch.Reactor.Example.Models;
using System;
using static Rearch.Reactor.Example.Capsules.TodoCapsules;

namespace Rearch.Reactor.Example.Pages;

partial class MainPage : CapsuleConsumer
{
    public override VisualNode Render(ICapsuleHandle use)
    {
        var (isSearching, setIsSearching) = use.State(false);

        var (_, UpdateTodo, DeleteTodos) = use.Invoke(TodoItemsManagerCapsule);

        var (
            filter,
            _,
            toggleCompletionStatus) =
            use.Invoke(FilterCapsule);
        var completionStatus = filter.CompletionStatus;

        return NavigationPage(
            ContentPage(
                ToolbarItem("Clear List")
                .OnClicked(OnClearList),

                ToolbarItem(completionStatus ?
                    "Complete" :
                    "Incomplete")
                .OnClicked(toggleCompletionStatus),

                ToolbarItem("Search")
                .OnClicked(() => setIsSearching(!isSearching)),

                ToolbarItem("Edit")
                .OnClicked(() => ShowCreateTodoDialogAsync(UpdateTodo)),

                new GlobalWarmUps(new Body())
            )
            .Title("rearch todos"));

        void OnClearList()
        {
            var todoItems = use.Invoke(TodoQueryCapsule);

            DeleteTodos(todoItems);
        }
    }

    private void ShowCreateTodoDialogAsync(Action<Todo> updateTodo)
    {
        throw new NotImplementedException();
    }
}
