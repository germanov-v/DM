1.   var span = CollectionsMarshal.AsSpan(rows);         - обалденная штука 
2. CollectionsMarshal.GetValueRefOrAddDefault(dict, item.Id, out bool exists); - оптимизация