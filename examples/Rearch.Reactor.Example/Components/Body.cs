using MauiReactor;
using Rearch.Reactor.Example.Models;
using Rearch.Reactor.Components;
using static Rearch.Reactor.Example.Capsules.TodoCapsules;

namespace Rearch.Reactor.Example.Components;

partial class Body : CapsuleConsumer
{
    public override VisualNode Render(ICapsuleHandle use)
    {
        var todoItems = use.Invoke(TodoItemsCapsule);

        return Grid("Auto, *", "*",
            new TodoEditor(OnCreatedNewTask),

            CollectionView()
                .ItemsSource(todoItems, i => new Item(i, OnItemDoneChanged))
                .GridRow(1)
        );

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
    }
}
