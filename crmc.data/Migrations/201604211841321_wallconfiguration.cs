namespace crmc.data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class wallconfiguration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.WallConfigurations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MinFontSize = c.Int(nullable: false),
                        MaxFontSize = c.Int(nullable: false),
                        KioskEntryTopMargin = c.Int(nullable: false),
                        ScreenBottomMargin = c.Int(nullable: false),
                        GeneralRotationDelay = c.Double(nullable: false),
                        PriorityRotationDelay = c.Double(nullable: false),
                        KioskDisplayRecycleCount = c.Int(nullable: false),
                        Volume = c.Double(nullable: false),
                        GrowAnimationDuration = c.Double(nullable: false),
                        ShrinkAnimationDuration = c.Double(nullable: false),
                        FallAnimationDurationTimeModifier = c.Double(nullable: false),
                        ConfigurationMode = c.Int(nullable: false),
                        IsCurrent = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.WallConfigurations");
        }
    }
}
