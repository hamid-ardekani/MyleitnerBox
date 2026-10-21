using MyLeitner.Application.Boxes.Queries.Models;
using MyLeitner.Application.Flashcards.Dtos;

namespace MyLeitner.Api.Models;
 
public readonly record struct AddFlashcardRequest(short Order,DocumentDto  Front,DocumentDto Rear,List<string> Tags);
public readonly record struct UpdateFlashcardRequest(long FlashcardId,string Front, string Rear,List<string> Tags);