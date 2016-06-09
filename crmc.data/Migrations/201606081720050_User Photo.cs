namespace crmc.data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserPhoto : DbMigration
    {
        public override void Up()
        {
            AddColumn("Security.Users", "UserPhoto", c => c.Binary());
        }
        
        public override void Down()
        {
            DropColumn("Security.Users", "UserPhoto");
        }
    }
}
