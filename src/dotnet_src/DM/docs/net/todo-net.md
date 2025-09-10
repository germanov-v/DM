1.   var span = CollectionsMarshal.AsSpan(rows);         - обалденная штука 
2. CollectionsMarshal.GetValueRefOrAddDefault(dict, item.Id, out bool exists); - оптимизация
3. Повторить Span (работы со строками в Dapper Mapper) - и еще раз пересмотреть ValueStringBuilder
4. 