using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SecretSanta_Backend.Models;

namespace SecretSanta_Backend
{
    public partial class ApplicationContext : DbContext
    {
        public ApplicationContext()
        {
        }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Address> Addresses { get; set; } = null!;
        public virtual DbSet<Event> Events { get; set; } = null!;
        public virtual DbSet<Member> Members { get; set; } = null!;
        public virtual DbSet<MemberEvent> MemberEvents { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("Address");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Apartment)
                    .HasMaxLength(5)
                    .HasColumnName("apartment");

                entity.Property(e => e.City)
                    .HasMaxLength(50)
                    .HasColumnName("city");

                entity.Property(e => e.MemberId).HasColumnName("member_id");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(10)
                    .HasColumnName("phone_number");

                entity.Property(e => e.Region)
                    .HasMaxLength(50)
                    .HasColumnName("region");

                entity.Property(e => e.Street)
                    .HasMaxLength(50)
                    .HasColumnName("street");

                entity.Property(e => e.Zip)
                    .HasMaxLength(50)
                    .HasColumnName("zip");

                entity.HasOne(d => d.Member)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Address_Member_1");
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("Event");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Description)
                    .HasMaxLength(140)
                    .HasColumnName("description");

                entity.Property(e => e.EndEvent).HasColumnName("end_event");

                entity.Property(e => e.EndRegistration).HasColumnName("end_registration");

                entity.Property(e => e.Reshuffle)
                    .HasColumnName("reshuffle")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.SendFriends)
                    .HasColumnName("send_friends")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.SumPrice).HasColumnName("sum_price");

                entity.Property(e => e.Tracking)
                    .HasColumnName("tracking")
                    .HasDefaultValueSql("false");
            });

            modelBuilder.Entity<Member>(entity =>
            {
                entity.ToTable("Member");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .HasColumnName("email");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Patronymic)
                    .HasMaxLength(50)
                    .HasColumnName("patronymic");

                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .HasColumnName("role");

                entity.Property(e => e.Surname)
                    .HasMaxLength(50)
                    .HasColumnName("surname");
            });

            modelBuilder.Entity<MemberEvent>(entity =>
            {
                entity.ToTable("Member_Event");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.DeliveryService)
                    .HasMaxLength(100)
                    .HasColumnName("delivery_service");

                entity.Property(e => e.EventId).HasColumnName("event_id");

                entity.Property(e => e.MemberAttend)
                    .HasColumnName("member_attend")
                    .HasDefaultValueSql("true");

                entity.Property(e => e.MemberId).HasColumnName("member_id");

                entity.Property(e => e.Preference).HasColumnName("preference");

                entity.Property(e => e.Recipient).HasColumnName("recipient");

                entity.Property(e => e.SendDay).HasColumnName("send_day");

                entity.Property(e => e.TrackNumber)
                    .HasMaxLength(100)
                    .HasColumnName("track_number");

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.MemberEvents)
                    .HasForeignKey(d => d.EventId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Member_Event_Event_1");

                entity.HasOne(d => d.Member)
                    .WithMany(p => p.MemberEvents)
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Member_Event_Member_1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
