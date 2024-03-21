using MauiReactor;
using Rearch.Reactor.Components;
using Rearch.Reactor.Example.Models;

namespace Rearch.Reactor.Example.Pages;

partial class CreateTodoPage : CapsuleConsumer<CreateTodoPageProps>
{
    private string title = string.Empty;
    private string description = string.Empty;

    public override VisualNode Render(ICapsuleHandle use)
    {
        return ContentPage(
            "Create Todo",

            VerticalStackLayout(
                Entry()
                .OnTextChanged(newTitle => title = newTitle),

                Entry()
                .OnTextChanged(newDescription => description = newDescription),

                HorizontalStackLayout(
                    Button("Cancel")
                    .OnClicked(() => ContainerPage?.Navigation.PopModalAsync()),

                    Button("Save")
                    .OnClicked(() =>
                    {
                        this.Props.TodoCreator(new Todo
                        {
                            Title = this.title,
                            Description = string.IsNullOrEmpty(this.description) ? null : description,
                            Completed = false,
                        });

                        this.ContainerPage?.Navigation.PopModalAsync();
                    })
                )
            )
        );
    }
}
