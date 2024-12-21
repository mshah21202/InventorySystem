namespace Domain;

public class Item
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public int Quantity { get; set; }
    public Guid? GroupId { get; set; }
}