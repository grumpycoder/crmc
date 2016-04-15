namespace crmc.data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Persons",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    AccountId = c.String(maxLength: 8000, unicode: false),
                    Firstname = c.String(maxLength: 8000, unicode: false),
                    Lastname = c.String(maxLength: 8000, unicode: false),
                    EmailAddress = c.String(maxLength: 8000, unicode: false),
                    Zipcode = c.String(maxLength: 8000, unicode: false),
                    IsDonor = c.Boolean(),
                    IsPriority = c.Boolean(),
                    FuzzyMatchValue = c.Decimal(precision: 18, scale: 2),
                    SortOrder = c.Guid(),
                    DateCreated = c.DateTime(nullable: false, precision: 7, storeType: "datetime2", defaultValueSql: "getdate()"),
                })
                .PrimaryKey(t => t.Id);
        }

        public override void Down()
        {
            DropTable("dbo.Persons");
        }
    }
}