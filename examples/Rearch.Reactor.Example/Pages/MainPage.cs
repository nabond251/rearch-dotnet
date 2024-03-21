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

        var (AddTodo, _, DeleteTodos) = use.Invoke(TodoItemsManagerCapsule);

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
                .OnClicked(() => ShowCreateTodoDialogAsync(
                    ContainerPage, AddTodo)),

                new GlobalWarmUps(new Body())
            )
            .Title("rearch todos"));

        void OnClearList()
        {
            var todoItems = use.Invoke(TodoQueryCapsule);

            DeleteTodos(todoItems);
        }
    }

    private void ShowCreateTodoDialogAsync(
        MauiControls.Page? containerPage,
        Action<Todo> todoCreator)
    {
        containerPage?.Navigation.PushModalAsync<CreateTodoPage, CreateTodoPageProps>(p => p.TodoCreator = todoCreator);
    }
}

internal class CreateTodoPageProps()
{
    public Action<Todo> TodoCreator { get; set; }
}