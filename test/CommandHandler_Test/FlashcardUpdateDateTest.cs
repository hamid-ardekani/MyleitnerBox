using MyLeitner.Domain.Flashcards;
using Should;

namespace CommandHandler_Test;

public class FlashcardUpdateDateTest
{
    private static Flashcard NewCard() => Flashcard.ForAdd(1, 1, "front", "back", "front", "back", ["t"]);

    [Fact]
    public void A_new_card_has_no_edit_date_yet()
    {
        NewCard().UpdateDate.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void Editing_the_content_records_when()
    {
        var card = NewCard();
        var before = DateTime.UtcNow;

        card.Update("front 2", "back 2", "front 2", "back 2", ["t"]);

        card.UpdateDate.HasValue.ShouldBeTrue();
        (card.UpdateDate >= before).ShouldBeTrue();
    }

    [Fact]
    public void Reordering_is_not_an_edit()
    {
        var card = NewCard();

        card.UpdateOrder(3);

        card.UpdateDate.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void Tags_are_optional()
    {
        Flashcard.ForAdd(1, 1, "f", "b", "f", "b", []).Tags.ShouldEqual("");
        Flashcard.ForAdd(1, 1, "f", "b", "f", "b", null!).Tags.ShouldEqual("");

        var card = NewCard();
        card.Update("f", "b", "f", "b", [" ", " a "]);
        card.Tags.ShouldEqual("a");
    }

    [Fact]
    public void Moving_to_another_box_changes_box_and_order_only()
    {
        var card = NewCard();

        card.MoveToBox(22, 7);

        card.BoxId.ShouldEqual(22L);
        card.Order.ShouldEqual((short)7);
        card.Front.ShouldEqual("front");
    }

    [Fact]
    public void Moving_needs_a_valid_order()
    {
        Assert.Throws<MyLeitner.Domain.Boxes.Exceptions.OrderUnValidException>(() => NewCard().MoveToBox(22, 0));
    }
}
