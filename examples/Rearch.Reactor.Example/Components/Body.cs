using MauiReactor;
using Rearch.Reactor.Example.Models;
using Rearch.Reactor.Components;
using static Rearch.Reactor.Example.Capsules.TodoCapsules;

namespace Rearch.Reactor.Example.Components;

partial class Body : CapsuleConsumer
{
    public override VisualNode Render(ICapsuleHandle use)
    {
        return Grid("Auto, *", "*",
            new TodoEditor(OnCreatedNewTask),

            new TodoList()
            .GridRow(1)
        );

        void OnCreatedNewTask(Todo todo)
        {
            var (AddTodo, _, _) = use.Invoke(TodoItemsManagerCapsule);

            AddTodo(todo);
        }
    }
}
