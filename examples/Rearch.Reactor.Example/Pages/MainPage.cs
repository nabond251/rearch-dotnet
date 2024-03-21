using MauiReactor;
using Rearch.Reactor.Components;
using Rearch.Reactor.Example.Components;
using static Rearch.Reactor.Example.Capsules.TodoCapsules;

namespace Rearch.Reactor.Example.Pages;

partial class MainPage : CapsuleConsumer
{
    public override VisualNode Render(ICapsuleHandle use)
    {
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

                new GlobalWarmUps(new Body())
            )
            .Title("rearch todos"));

        void OnClearList()
        {
            var (_, _, DeleteTodos) = use.Invoke(TodoItemsManagerCapsule);

            var todoItems = use.Invoke(TodoQueryCapsule);

            DeleteTodos(todoItems);
        }
    }
}
