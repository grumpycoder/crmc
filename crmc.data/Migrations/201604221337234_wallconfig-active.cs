namespace crmc.data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class wallconfigactive : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WallConfigurations", "Active", c => c.Boolean(nullable: false));
            DropColumn("dbo.WallConfigurations", "IsCurrent");
        }
        
        public override void Down()
        {
            AddColumn("dbo.WallConfigurations", "IsCurrent", c => c.Boolean(nullable: false));
            DropColumn("dbo.WallConfigurations", "Active");
        }
    }
}
