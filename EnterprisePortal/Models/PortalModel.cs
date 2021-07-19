using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace EnterprisePortal.Models
{
    public partial class PortalModel : DbContext
    {
        public PortalModel()
            : base("name=PortalModel")
        {
        }

        public virtual DbSet<ActivitiesApplication> ActivitiesApplications { get; set; }
        public virtual DbSet<Activity> Activities { get; set; }
        public virtual DbSet<Announcement> Announcements { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<MessageList> MessageLists { get; set; }
        public virtual DbSet<QuickLink> QuickLinks { get; set; }
        public virtual DbSet<QuickLinksCategory> QuickLinksCategories { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<RolePermission> RolePermissions { get; set; }
        public virtual DbSet<ToDoList> ToDoLists { get; set; }
        public virtual DbSet<UserAccount> UserAccounts { get; set; }
        public virtual DbSet<UserLog> UserLogs { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<Voting> Votings { get; set; }
        public virtual DbSet<VotingContent> VotingContents { get; set; }
        public virtual DbSet<VotingForm> VotingForms { get; set; }
        public virtual DbSet<VotingResult> VotingResults { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}