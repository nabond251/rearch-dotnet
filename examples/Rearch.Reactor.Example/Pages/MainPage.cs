using MauiReactor;
using Rearch.Reactor.Components;
using Rearch.Reactor.Example.Components;
using static Rearch.Reactor.Example.Capsules.TodoCapsules;

namespace Rearch.Reactor.Example.Pages;

partial class MainPage : CapsuleConsumer
{
    public override VisualNode Render(ICapsuleHandle use)
    {
        return NavigationPage(
            ContentPage(
                ToolbarItem("Clear List")
                .OnClicked(OnClearList),

                new GlobalWarmUps(new Body())
            )
            .Title("Rearch Todos"));

        void OnClearList()
        {
            var (_, _, DeleteTodos) = use.Invoke(TodoItemsManagerCapsule);

            var todoItems = use.Invoke(TodoItemsCapsule);

            DeleteTodos(todoItems);
        }
    }
}
