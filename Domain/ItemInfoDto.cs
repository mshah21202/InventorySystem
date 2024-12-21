namespace Domain;

public class ItemInfoDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Group { get; set; }
    public int Quantity { get; set; }
}