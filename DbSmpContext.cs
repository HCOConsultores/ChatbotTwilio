
using chatBotTwilio.Models.BPI;
using chatBotTwilio.Models.WhatsApp;
using Microsoft.EntityFrameworkCore;


public class DbSmpContext : DbContext
    {
        public DbSmpContext()
        { }

        public DbSmpContext(DbContextOptions<DbSmpContext> options)
        : base(options)
        { }

        public virtual DbSet<Contract> Contracts { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<Oilfield> Oilfields { get; set; }
        public virtual DbSet<ConversationState> ConversationStates { get; set; }
        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<Logbook> Logbooks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>()
                    .HasOne(p => p.NavOilfield)
                    .WithMany()
                    .HasForeignKey(p => p.IdOilfield);

            modelBuilder.Entity<Project>()
               .HasOne(p => p.NavContract)
               .WithMany()
               .HasForeignKey(p => p.IdContrato);

            modelBuilder.Entity<Contract>()
               .HasOne(p => p.NavProvider)
               .WithMany()
               .HasForeignKey(p => p.IdProvider);


            modelBuilder.Entity<ConversationState>(entity =>
            {
                entity.Property(e => e.PhoneNumber).IsRequired();
                entity.Property(e => e.CurrentState).IsRequired(false);
                entity.Property(e => e.PreviousState).IsRequired(false);
                entity.Property(e => e.CurrentContract).IsRequired(false);
                entity.Property(e => e.CurrentProject).IsRequired(false);
                entity.Property(e => e.CurrentNoteType).IsRequired(false);
                entity.Property(e => e.Supervisor).IsRequired(false);
                entity.Property(e => e.Ot).IsRequired(false);
                entity.Property(e => e.Descripcorta).IsRequired(false);
         
            });

        }
   
}
