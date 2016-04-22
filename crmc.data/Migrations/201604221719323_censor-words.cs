namespace crmc.data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class censorwords : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Censors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Word = c.String(maxLength: 8000, unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Censors");
        }
    }
}
