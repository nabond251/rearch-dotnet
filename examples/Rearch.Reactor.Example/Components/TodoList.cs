using System.Linq;
using MauiReactor;
using Rearch.Reactor.Example.Models;
using Rearch.Reactor.Components;
using static Rearch.Reactor.Example.Capsules.TodoCapsules;
using Rearch.Types;

namespace Rearch.Reactor.Example.Components;

partial class TodoList : CapsuleConsumer
{
    public override VisualNode Render(ICapsuleHandle use)
    {
        var completionFilter = use.Invoke(FilterCapsule).Filter.CompletionStatus;
        var completionText = completionFilter ? "completed" : "incomplete";

        var todoListCount = use.Invoke(TodoListCountCapsule);
        var todoQuery = use.Invoke(TodoQueryCapsule);

        var todoListWidget = todoListCount.GetData().Select(_ =>
        {
            return CollectionView().ItemsSource(
                todoQuery,
                i => new TodoItem(i));
        }).AsNullable();

        var infoWidget = todoListCount.Match(
            onLoading: _ => Label("Loading..."),
            onError: (error, _) => Label(error.ToString()),
            onData: count => count == 0 ?
                Label($"No {completionText} todos found") :
                null);

        return StackLayout(
            children: (todoListWidget != null ?
                [todoListWidget] :
                Enumerable.Empty<VisualNode>())
                .Concat(infoWidget != null ?
                [Frame(infoWidget)] :
                Enumerable.Empty<VisualNode>())
                .ToArray());
    }
}
