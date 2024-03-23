using MauiReactor;
using Rearch.Reactor.Example.Models;
using Rearch.Reactor.Components;
using static Rearch.Reactor.Example.Capsules.TodoCapsules;

namespace Rearch.Reactor.Example.Components;

partial class TodoList : CapsuleConsumer
{
    public override VisualNode Render(ICapsuleHandle use)
    {
        var todoItems = use.Invoke(TodoQueryCapsule);

        return CollectionView()
            .ItemsSource(todoItems, i => new Item(i, OnItemDoneChanged));

        void OnItemDoneChanged(Todo item, bool done)
        {
            var (_, UpdateTodo, _) = use.Invoke(TodoItemsManagerCapsule);

            item.Completed = done;

            UpdateTodo(item);
        }
    }
}
