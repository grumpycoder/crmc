namespace crmc.data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userphototype : DbMigration
    {
        public override void Up()
        {
            AddColumn("Security.Users", "UserPhotoType", c => c.String(maxLength: 8000, unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("Security.Users", "UserPhotoType");
        }
    }
}
