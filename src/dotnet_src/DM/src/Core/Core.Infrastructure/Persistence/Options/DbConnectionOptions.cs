using System.ComponentModel.DataAnnotations;

namespace Core.Application.Options.Db;

public class DbConnectionOptions
{
	[Required]
	public required string ConnectionString { get; set; }
}
