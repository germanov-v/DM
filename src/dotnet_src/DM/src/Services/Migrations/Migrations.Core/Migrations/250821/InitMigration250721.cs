using FluentMigrator;

namespace Migrations.Core.Migrations._250821;

[Migration(20250721, "Init Migration")]
public class InitMigration250721 : Migration
{
    public override void Up()
    {
        Execute.Script("./Migrations/250821/up_files.sql");

        Execute.Script("./Migrations/250821/up_telegram.sql");
        Execute.Script("./Migrations/250821/up_identityV2.sql");
        Execute.Script("./Migrations/250821/up_references.sql");
        Execute.Script("./Migrations/250821/up_chats.sql");
      // Execute.Script("./Migrations/250721/up_blog.sql");
       // Execute.Script("./Migrations/250721/up_files.sql");
       //
       // Execute.Script("./Migrations/250721/up_chats.sql");
      
      
    }

    public override void Down()
    {
        Execute.Script("./Migrations/250821/down_create_empty_db.sql");
    }
}