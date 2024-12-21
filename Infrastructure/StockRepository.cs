using System.Globalization;
using Application;
using CsvHelper;
using Domain;

namespace Infrastructure;

public class StockRepository : IStockRepository
{
    private const string ItemsFilePath = "items.csv";
    private const string GroupsFilePath = "groups.csv";
    public string ItemsFullPath { get; } = Path.GetFullPath(ItemsFilePath);
    public string GroupsFullPath { get; } = Path.GetFullPath(GroupsFilePath);
    
    public async Task<int> AddItemAsync(Item item)
    {
        try
        {
            var items = GetItems();
            items.Add(item);
            await WriteItemsAsync(items);
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }

    public async Task<int> RemoveItemAsync(Guid id)
    {
        try
        {
            var items = GetItems();
            var item = items.FirstOrDefault(i => i.Id == id);
            if (item == null)
            {
                return 1;
            }
            var result = items.Remove(item);
            await WriteItemsAsync(items);
            return result ? 0 : 1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }

    public Item? GetItem(Guid id) => GetItems().FirstOrDefault(i => i.Id == id);

    public List<Item> GetItems()
    {
        if (!File.Exists(ItemsFilePath))
        {
            // Create file
            File.Create(ItemsFilePath).Close();
        }
        
        using var reader = new StreamReader(ItemsFilePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return csv.GetRecords<Item>().ToList();
    }

    public async Task<int> UpdateItemAsync(Item item)
    {
        try
        {
            var items = GetItems();
            var index = items.FindIndex(i => i.Id == item.Id);
            if (index == -1)
            {
                return 1;
            }
            items[index] = item;
            await WriteItemsAsync(items);
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }

    public async Task<int> AddGroupAsync(Group group)
    {
        try
        {
            var groups = GetGroups();
            groups.Add(group);
            await WriteGroupsAsync(groups);
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }

    public async Task<int> RemoveGroupAsync(Guid id)
    {
        try
        {
            var groups = GetGroups();
            var group = groups.FirstOrDefault(g => g.Id == id);
            if (group == null)
            {
                return 1;
            }
            var result = groups.Remove(group);
            await WriteGroupsAsync(groups);
            return result ? 0 : 1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }

    public Group? GetGroup(Guid id) => GetGroups().FirstOrDefault(g => g.Id == id);

    public List<Group> GetGroups()
    {
        if (!File.Exists(GroupsFilePath))
        {
            // Create file
            File.Create(GroupsFilePath).Close();
            return [];
        }
        
        using var reader = new StreamReader(GroupsFilePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return csv.GetRecords<Group>().ToList();
    }

    public async Task<int> UpdateGroupAsync(Group group)
    {
        try
        {
            var groups = GetGroups();
            var index = groups.FindIndex(g => g.Id == group.Id);
            if (index == -1)
            {
                return 1;
            }
            groups[index] = group;
            await WriteGroupsAsync(groups);
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }

    public async Task ClearItemsAsync()
    {
        if (!File.Exists(ItemsFilePath))
        {
            return;
        }
        
        await using var writer = new StreamWriter(ItemsFilePath);
        await writer.WriteAsync(string.Empty);
    }

    public async Task ClearGroupsAsync()
    {
        if (!File.Exists(GroupsFilePath))
        {
            return;
        }
        
        await using var writer = new StreamWriter(GroupsFilePath);
        await writer.WriteAsync(string.Empty);
    }

    public ItemInfoDto? GetItemInfo(Guid id)
    {
        var item = GetItem(id);
        if (item == null)
        {
            return null;
        }
        
        var group = GetGroup(item.GroupId ?? Guid.Empty);
        return new ItemInfoDto
        {
            Id = item.Id,
            Name = item.Name,
            Group = group?.Name ?? "None",
            Quantity = item.Quantity
        };
    }

    public List<ItemInfoDto> GetItemsInfo(Guid? groupId = null)
    {
        var items = GetItems();
        var groups = GetGroups();
        return items
            .Where(i => groupId == null || i.GroupId == groupId)
            .Select(i => new ItemInfoDto
            {
                Id = i.Id,
                Name = i.Name,
                Group = groups.FirstOrDefault(g => g.Id == i.GroupId)?.Name ?? "None",
                Quantity = i.Quantity
            })
            .ToList();
    }

    private async Task WriteItemsAsync(List<Item> items)
    {
        await using var writer = new StreamWriter(ItemsFilePath);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(items);
    }
    
    private async Task WriteGroupsAsync(List<Group> groups)
    {
        await using var writer = new StreamWriter(GroupsFilePath);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(groups);
    }
}