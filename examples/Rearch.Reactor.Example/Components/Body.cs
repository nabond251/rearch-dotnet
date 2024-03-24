using MauiReactor;
using Rearch.Reactor.Example.Models;
using Rearch.Reactor.Components;
using static Rearch.Reactor.Example.Capsules.TodoCapsules;
using Rearch.Reactor.Example.Pages;
using System;

namespace Rearch.Reactor.Example.Components;

partial class Body : CapsuleConsumer
{
    public override VisualNode Render(ICapsuleHandle use)
    {
        var (isSearching, setIsSearching) = use.State(false);

        var (AddTodo, _, _) = use.Invoke(TodoItemsManagerCapsule);

        var (
            filter,
            _,
            toggleCompletionStatus) =
            use.Invoke(FilterCapsule);
        var completionStatus = filter.CompletionStatus;

        return NavigationPage(
            ContentPage(
                ToolbarItem(completionStatus ?
                    "Complete" :
                    "Incomplete")
                .OnClicked(toggleCompletionStatus),

                ToolbarItem("Search")
                .OnClicked(() => setIsSearching(!isSearching)),

                ToolbarItem("Create")
                .OnClicked(() => ShowCreateTodoDialogAsync(
                    ContainerPage, AddTodo)),

                new TodoList()
            )
            .Title("rearch todos")
        );

        void ShowCreateTodoDialogAsync(
            MauiControls.Page? containerPage,
            Action<Todo> todoCreator)
        {
            containerPage?.Navigation.PushModalAsync<CreateTodoPage, CreateTodoPageProps>(p => p.TodoCreator = todoCreator);
        }
    }
}
