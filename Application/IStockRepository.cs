using Domain;

namespace Application;

public interface IStockRepository
{
    string ItemsFullPath { get; }
    string GroupsFullPath { get; }
    Task<int> AddItemAsync(Item item);
    Task<int> RemoveItemAsync(Guid id);
    Item? GetItem(Guid id);
    List<Item> GetItems();
    Task<int> UpdateItemAsync(Item item);
    Task<int> AddGroupAsync(Group group);
    Task<int> RemoveGroupAsync(Guid id);
    Group? GetGroup(Guid id);
    List<Group> GetGroups();
    Task<int> UpdateGroupAsync(Group group);
    Task ClearItemsAsync();
    Task ClearGroupsAsync();
    ItemInfoDto? GetItemInfo(Guid id);
    List<ItemInfoDto> GetItemsInfo(Guid? groupId = null);
}